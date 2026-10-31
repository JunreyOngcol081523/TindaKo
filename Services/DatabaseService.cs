using Microsoft.Maui.Storage;
using SQLite;
using System.Diagnostics;
using TindaKo.Helpers;
using TindaKo.Models;

namespace TindaKo.Services
{
    /// <summary>
    /// Owns the single SQLite connection. Everything that opens, closes, replaces,
    /// exports or resets the database goes through here, guarded by one lock.
    /// </summary>
    public static class DatabaseService
    {
        public const string DbFileName = "tindako.db";

        /// Bump when the schema changes in a way an older app version can't read.
        public const int SchemaVersion = 1;

        private static readonly SemaphoreSlim _gate = new(1, 1);
        private static volatile bool _tablesReady;

        // Still public so existing code that touches DatabaseService._db keeps compiling.
        // New code should always use GetConnectionAsync().
        public static volatile SQLiteAsyncConnection? _db;

        // Tables a file must contain to be accepted as a TindaKo backup.
        private static readonly Type[] RequiredTables =
        {
            typeof(Product), typeof(Sell), typeof(Transaction), typeof(Users)
        };

        public static string DbPath => Path.Combine(FileSystem.AppDataDirectory, DbFileName);

        // ───────────────────────── Connection ─────────────────────────

        /// Kept for compatibility: now also creates the tables, so InitAsync + _db is safe too.
        public static Task InitAsync() => GetConnectionAsync();

        public static async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            // Fast path: no lock, no table checks after the first successful init.
            var db = _db;
            if (db != null && _tablesReady) return db;

            await _gate.WaitAsync();
            try
            {
                return await OpenAndPrepareAsync();
            }
            finally
            {
                _gate.Release();
            }
        }

        // Caller MUST hold _gate.
        private static async Task<SQLiteAsyncConnection> OpenAndPrepareAsync()
        {
            if (_db != null && _tablesReady) return _db;

            var conn = _db ?? new SQLiteAsyncConnection(DbPath);
            try
            {
                await conn.CreateTableAsync<Product>();
                await conn.CreateTableAsync<Users>();
                await conn.CreateTableAsync<Sell>();
                await conn.CreateTableAsync<Transaction>();
                await conn.CreateTableAsync<Buyer>();
                await conn.CreateTableAsync<Creditor>();
                await conn.CreateTableAsync<IncomingStocks>();
                await conn.CreateTableAsync<IncomingStocksProductsList>();

                await conn.ExecuteAsync($"PRAGMA user_version = {SchemaVersion}");
            }
            catch
            {
                if (!ReferenceEquals(conn, _db))
                    await conn.CloseAsync();
                throw;
            }

            _db = conn;
            _tablesReady = true;
            return conn;
        }

        // Caller MUST hold _gate.
        private static async Task CloseCurrentAsync()
        {
            _tablesReady = false;
            var old = _db;
            _db = null;
            if (old != null)
                await old.CloseAsync();
        }

        private static void DeleteSidecarFiles()
        {
            foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
            {
                var path = DbPath + suffix;
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        // ───────────────────────── Backup / Restore primitives ─────────────────────────

        /// Writes a consistent snapshot of the live database to destinationPath.
        /// VACUUM INTO is safe while the DB is open (a plain File.Copy is not).
        public static async Task ExportBackupAsync(string destinationPath)
        {
            var db = await GetConnectionAsync();

            var dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            try
            {
                var escaped = destinationPath.Replace("'", "''");
                await db.ExecuteAsync($"VACUUM INTO '{escaped}'");
            }
            catch (SQLiteException ex)
            {
                // SQLite older than 3.27 has no VACUUM INTO -> fall back to a locked plain copy.
                Debug.WriteLine($"VACUUM INTO failed, falling back to copy: {ex.Message}");
                await _gate.WaitAsync();
                try
                {
                    File.Copy(DbPath, destinationPath, overwrite: true);
                }
                finally
                {
                    _gate.Release();
                }
            }
        }

        /// Returns null when the file is a usable TindaKo backup, otherwise the reason it is not.
        public static async Task<string?> ValidateBackupAsync(string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                return "Hindi nahanap ang backup file.";

            SQLiteAsyncConnection? probe = null;
            try
            {
                probe = new SQLiteAsyncConnection(backupFilePath, SQLiteOpenFlags.ReadOnly);

                var integrity = await probe.ExecuteScalarAsync<string>("PRAGMA integrity_check");
                if (!string.Equals(integrity, "ok", StringComparison.OrdinalIgnoreCase))
                    return "Sira ang backup file (bumagsak sa integrity check).";

                var version = await probe.ExecuteScalarAsync<int>("PRAGMA user_version");
                if (version > SchemaVersion)
                    return "Galing ang backup sa mas bagong bersyon ng TindaKo. I-update muna ang app.";

                foreach (var type in RequiredTables)
                {
                    var name = new TableMapping(type).TableName;
                    var count = await probe.ExecuteScalarAsync<int>(
                        "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = ?", name);

                    if (count == 0)
                        return $"Hindi ito TindaKo backup (kulang ang table na '{name}').";
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Backup validation failed: {ex.Message}");
                return "Hindi mabasa ang file bilang TindaKo database.";
            }
            finally
            {
                if (probe != null)
                    await probe.CloseAsync();
            }
        }

        /// Replaces the live database with the given (already validated) backup.
        /// Keeps a safety copy of the current DB and rolls back if anything fails.
        /// Run this when no other DB work is in flight (e.g. from a Settings page).
        public static async Task ReplaceDatabaseAsync(string backupFilePath)
        {
            await _gate.WaitAsync();
            try
            {
                var safetyCopy = Path.Combine(FileSystem.AppDataDirectory, "tindako_pre_restore.bak");

                await CloseCurrentAsync();

                if (File.Exists(DbPath))
                    File.Copy(DbPath, safetyCopy, overwrite: true);

                try
                {
                    DeleteSidecarFiles();
                    File.Copy(backupFilePath, DbPath, overwrite: true);
                    await OpenAndPrepareAsync(); // also migrates an older backup to the current schema
                }
                catch
                {
                    await RollbackAsync(safetyCopy);
                    throw;
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        // Caller MUST hold _gate.
        private static async Task RollbackAsync(string safetyCopy)
        {
            try
            {
                await CloseCurrentAsync();
                DeleteSidecarFiles();

                if (File.Exists(safetyCopy))
                    File.Copy(safetyCopy, DbPath, overwrite: true);

                await OpenAndPrepareAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Rollback after failed restore also failed: {ex.Message}");
            }
        }

        // ───────────────────────── Reset ─────────────────────────

        /// DESTRUCTIVE: wipes all data (including Users). Always ask the user to confirm
        /// before calling this, and make sure a default user is seeded afterwards.
        public static async Task ResetDB()
        {
            string? error = null;

            await _gate.WaitAsync();
            try
            {
                var db = await OpenAndPrepareAsync();

                // All-or-nothing: either every table is dropped or none is.
                await db.RunInTransactionAsync(conn =>
                {
                    conn.DropTable<Product>();
                    conn.DropTable<Users>();
                    conn.DropTable<Sell>();
                    conn.DropTable<Transaction>();
                    conn.DropTable<Buyer>();
                    conn.DropTable<Creditor>();
                    conn.DropTable<IncomingStocks>();
                    conn.DropTable<IncomingStocksProductsList>();
                });

                // Recreate the empty tables straight away.
                _tablesReady = false;
                await OpenAndPrepareAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ResetDB failed: {ex}");
                error = ex.Message;
            }
            finally
            {
                _gate.Release();
            }

            if (error is null)
                await ToastHelper.ShowMessage("Na-reset ang database.");
            else
                await ToastHelper.ShowError($"Hindi na-reset ang database: {error}");
        }
    }
}