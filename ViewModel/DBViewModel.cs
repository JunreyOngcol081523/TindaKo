
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class DBViewModel : ObservableObject
    {
        string destinationPath = Path.Combine(FileSystem.AppDataDirectory, $"tindako_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");

        [RelayCommand]
        private async Task Backup() { 
           bool success= await DatabaseBackupService.BackupDatabaseAsync(destinationPath);
            if (success)
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "Backup save.", "OK");
                    
                }
                else
                {
                    await Toast.Make("Backup save.").Show();
                }
                _=LoadBackupFilesAsync();
            }
        }

        //[RelayCommand]
        //private async Task Restore() {
        //   bool success = await DatabaseBackupService.RestoreDatabaseAsync(destinationPath);
        //    if (success)
        //    {
        //        //  show a toast or log success
        //        await Toast.Make("Restoration Done.", ToastDuration.Short).Show();
        //    }
        //} 

        [RelayCommand]
        private async Task SendToEmail() => await DatabaseBackupService.EmailAllDbBackupsAsync();

        [RelayCommand]
        private async Task ShareVia() => await DatabaseBackupService.ShareAllDbBackupsAsync();


        public ObservableCollection<BackupFileItem> BackupFiles { get; } = new();

        [RelayCommand]
        public async Task LoadBackupFilesAsync()
        {
            BackupFiles.Clear();
            var files = Directory.GetFiles(FileSystem.AppDataDirectory, "tindako*.db");
            var mainDbPath = DatabaseBackupService.GetDatabasePath();

            foreach (var file in files)
            {
                BackupFiles.Add(new BackupFileItem
                {
                    FileName = Path.GetFileName(file),
                    FullPath = file,
                    IsMainDatabase = Path.GetFullPath(file) == Path.GetFullPath(mainDbPath)
                });
            }
        }


        [RelayCommand]
        public async Task DeleteBackupAsync(BackupFileItem file)
        {
            if (File.Exists(file.FullPath))
            {
                File.Delete(file.FullPath);
                BackupFiles.Remove(file);
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await Application.Current.MainPage.DisplayAlert("Backup Deleted", "Backup deleted.", "OK");
                }
                else
                {
                    await Toast.Make("Backup deleted.").Show();
                }

            }
        }

        [RelayCommand]
        public async Task RestoreBackupAsync(BackupFileItem file)
        {
            var success = await DatabaseBackupService.RestoreDatabaseAsync(file.FullPath);
            if (success)
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "Backup restored.", "OK");
                }
                else
                {
                    await Toast.Make("Backup restored.").Show();
                }

        }
        [RelayCommand]
        private async Task EmailBackup(BackupFileItem file)
        {
            var filePath = file.FullPath;
            if (File.Exists(filePath))
            {
                var message = new EmailMessage
                {
                    Subject = "TindaKo Backup File",
                    Body = "Attached is a backup file from TindaKo.",
                    To = new List<string> { "example@example.com" } // Optional default recipient
                };
                message.Attachments.Add(new EmailAttachment(filePath));

                try
                {
                    await Email.ComposeAsync(message);
                }
                catch (FeatureNotSupportedException)
                {
                    await Application.Current.MainPage.DisplayAlert("Not Supported", "Email is not supported on this device.", "OK");
                }
            }
        }

        [RelayCommand]
        private async Task ShareBackup(BackupFileItem file)
        {
            var filePath = file.FullPath;
            if (File.Exists(filePath))
            {
                try
                {
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "Share Backup",
                        File = new ShareFile(filePath)
                    });
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Sharing failed: {ex.Message}", "OK");
                }
            }
        }

    }
}
