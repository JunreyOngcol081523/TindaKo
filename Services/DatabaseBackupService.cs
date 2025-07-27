using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Services
{
    public static class DatabaseBackupService
    {
        private static string DbFileName => "tindako.db";

        public static string GetDatabasePath() =>
            Path.Combine(FileSystem.AppDataDirectory, DbFileName);

        public static async Task<bool> BackupDatabaseAsync(string destinationPath)
        {
            try
            {
                var dbPath = GetDatabasePath();

                if (!File.Exists(dbPath))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Database file not found.", "OK");
                    return false;
                }

                File.Copy(dbPath, destinationPath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Backup failed: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Backup Error", ex.Message, "OK");
                return false;
            }
        }

        public static async Task<bool> RestoreDatabaseAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Backup file not found.", "OK");
                    return false;
                }

                var dbPath = GetDatabasePath();

                // Close existing connection before replacing the DB
                if (DatabaseService._db != null)
                {
                    await DatabaseService._db.CloseAsync();
                    DatabaseService._db = null;
                }

                File.Copy(backupFilePath, dbPath, overwrite: true);

                // Re-initialize database
                await DatabaseService.InitAsync();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Restore failed: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Restore Error", ex.Message, "OK");
                return false;
            }
        }
    }

}
// For backup
//string backupPath = Path.Combine(FileSystem.CacheDirectory, "backup_tindako.db");
//bool backupSuccess = await DatabaseBackupService.BackupDatabaseAsync(backupPath);

//if (backupSuccess)
//    await Application.Current.MainPage.DisplayAlert("Backup", "Backup successful!", "OK");

//// For restore
//bool restoreSuccess = await DatabaseBackupService.RestoreDatabaseAsync(backupPath);

//if (restoreSuccess)
//    await Application.Current.MainPage.DisplayAlert("Restore", "Database restored!", "OK");
