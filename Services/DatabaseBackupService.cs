using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Globalization;
using TindaKo.Helpers;

namespace TindaKo.Services
{
    public static class DatabaseBackupService
    {
        public static string GetDatabasePath() => DatabaseService.DbPath;

        // ───────────────────────── Helpers ─────────────────────────

        private static string NewBackupFileName() =>
            $"tindako_backup_{DateTime.Now.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture)}.db";

        /// Creates a fresh, consistent snapshot in the cache folder (old snapshots are removed).
        /// The cache folder is temporary on purpose: it is only a staging area for share/email/save.
        private static async Task<string> CreateSnapshotAsync()
        {
            foreach (var old in Directory.GetFiles(FileSystem.CacheDirectory, "tindako_backup_*.db"))
            {
                try { File.Delete(old); } catch { /* still in use by a share target -> ignore */ }
            }

            var path = Path.Combine(FileSystem.CacheDirectory, NewBackupFileName());
            await DatabaseService.ExportBackupAsync(path);
            return path;
        }

        // ───────────────────────── Backup ─────────────────────────

        /// Writes a snapshot to an explicit path (kept for existing callers).
        public static async Task<bool> BackupDatabaseAsync(string destinationPath)
        {
            try
            {
                await DatabaseService.ExportBackupAsync(destinationPath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Backup failed: {ex}");
                await ToastHelper.ShowError($"Hindi na-backup: {ex.Message}");
                return false;
            }
        }

        /// Lets the user pick where to keep the backup (Downloads, Drive, SD card...).
        /// This is the real backup: app-private storage is deleted when the app is uninstalled.
        /// Returns false if the user cancelled or saving failed.
        public static async Task<bool> SaveBackupToDeviceAsync()
        {
            try
            {
                var snapshot = await CreateSnapshotAsync();

                await using var stream = File.OpenRead(snapshot);
                var result = await FileSaver.Default.SaveAsync(
                    Path.GetFileName(snapshot), stream, CancellationToken.None);

                if (!result.IsSuccessful)
                    Debug.WriteLine($"Save backup not completed: {result.Exception?.Message}");

                return result.IsSuccessful;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveBackupToDevice failed: {ex}");
                await ToastHelper.ShowError($"Hindi na-save ang backup: {ex.Message}");
                return false;
            }
        }

        // ───────────────────────── Restore ─────────────────────────

        /// Validates the file first, then swaps it in. The current data is kept as a safety
        /// copy and put back automatically if the restore fails.
        public static async Task<bool> RestoreDatabaseAsync(string backupFilePath)
        {
            try
            {
                var problem = await DatabaseService.ValidateBackupAsync(backupFilePath);
                if (problem != null)
                {
                    await ToastHelper.ShowError(problem);
                    return false;
                }

                await DatabaseService.ReplaceDatabaseAsync(backupFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Restore failed: {ex}");
                await ToastHelper.ShowError($"Hindi na-restore ang backup: {ex.Message}");
                return false;
            }
        }

        // ───────────────────────── Email / Share ─────────────────────────
        // Names kept for existing callers; both now send ONE fresh snapshot
        // instead of every tindako*.db file lying around in AppDataDirectory.

        public static async Task EmailAllDbBackupsAsync(string email)
        {
            try
            {
                if (!Email.Default.IsComposeSupported)
                {
                    await ToastHelper.ShowError("Walang email app sa device na ito.");
                    return;
                }

                var snapshot = await CreateSnapshotAsync();

                var message = new EmailMessage
                {
                    Subject = "TindaKo Backup File",
                    Body = "Attached is your TindaKo backup database file.",
                    To = string.IsNullOrWhiteSpace(email)
                        ? new List<string>()
                        : new List<string> { email.Trim() }
                };
                message.Attachments.Add(new EmailAttachment(snapshot));

                await Email.Default.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Email backup failed: {ex}");
                await ToastHelper.ShowError($"Hindi maipadala ang backup: {ex.Message}");
            }
        }

        public static async Task ShareAllDbBackupsAsync()
        {
            try
            {
                var snapshot = await CreateSnapshotAsync();

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Share TindaKo backup",
                    File = new ShareFile(snapshot)
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Share backup failed: {ex}");
                await ToastHelper.ShowError($"Hindi ma-share ang backup: {ex.Message}");
            }
        }
    }
}