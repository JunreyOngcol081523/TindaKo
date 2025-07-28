using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
using TindaKo.Models;

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
                    await ToastHelper.ShowError("Database file not found.");
                    return false;
                }

                File.Copy(dbPath, destinationPath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Backup failed: {ex.Message}");
                await ToastHelper.ShowError(ex.Message);
                return false;
            }
        }

        public static async Task<bool> RestoreDatabaseAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    await ToastHelper.ShowError("Backup file not found.");
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
                await ToastHelper.ShowError(ex.Message);
                return false;
            }
        }

        public static async Task EmailAllDbBackupsAsync()
        {
            var folder = FileSystem.AppDataDirectory;

            // Filter: starts with "tindako" and ends with ".db"
            var dbFiles = Directory.GetFiles(folder)
                                   .Where(f => Path.GetFileName(f).StartsWith("tindako", StringComparison.OrdinalIgnoreCase) &&
                                               Path.GetFileName(f).EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            if (dbFiles.Count == 0)
            {
                await ToastHelper.ShowError("No tindako database files found.");
                return;
            }

            var message = new EmailMessage
            {
                Subject = "TindaKo Backup Files",
                Body = "Attached are your TindaKo backup database files.",
                To = new List<string> { "junreyongcol@gmail.com"}// Optional: add default recipient
            };

            foreach (var file in dbFiles)
            {
                message.Attachments.Add(new EmailAttachment(file));
            }

            await Email.ComposeAsync(message);
        }
        public static async Task ShareAllDbBackupsAsync()
        {
            try
            {
                var folder = FileSystem.AppDataDirectory;

                // Get matching files
                var dbFiles = Directory.GetFiles(folder)
                                       .Where(f => Path.GetFileName(f).StartsWith("tindako", StringComparison.OrdinalIgnoreCase) &&
                                                   Path.GetFileName(f).EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                                       .ToList();

                if (dbFiles.Count == 0)
                {
                    await ToastHelper.ShowError("No tindako database files found.");
                    return;
                }

                // On most platforms (especially iOS), Share only supports one file at a time.
                // So if you want to share all files, you can zip them OR share one-by-one.

                foreach (var file in dbFiles)
                {
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "Share TindaKo DB",
                        File = new ShareFile(file)
                    });
                }
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex.StackTrace);
                
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
