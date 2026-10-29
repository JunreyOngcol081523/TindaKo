
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
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class DBViewModel : ObservableObject
    {
        [ObservableProperty] private string emailAd;
        string destinationPath = Path.Combine(FileSystem.AppDataDirectory, $"tindako_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
        public DBViewModel() {
            _ = LoadBackupFilesAsync();
            _=RestoreEmailAsync();
        }
        [RelayCommand]
        private async Task Backup() { 
           bool success= await DatabaseBackupService.BackupDatabaseAsync(destinationPath);
            if (success)
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    await App.Navigator.DisplayAlert("Info", "Backup save.", "OK");
                    
                }
                else
                {
                    await ToastHelper.ShowMessage("Backup saved.");
                }
                _=LoadBackupFilesAsync();
            }
        }
        [RelayCommand]
        private async Task SaveEmailAsync()
        {
            Preferences.Set("emailad",EmailAd);
            await ToastHelper.ShowMessage("Email saved"); 
        }
        private async Task RestoreEmailAsync() {
            EmailAd = Preferences.Get("emailad", string.Empty);
        }


        [RelayCommand]
        private async Task SendToEmail() => await DatabaseBackupService.EmailAllDbBackupsAsync(EmailAd);

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
                await ToastHelper.ShowMessage("Database deleted.");

            }
        }

        [RelayCommand]
        public async Task RestoreBackupAsync(BackupFileItem file)
        {
            var success = await DatabaseBackupService.RestoreDatabaseAsync(file.FullPath);
            if (success)
                await ToastHelper.ShowMessage("DB restored");

        }
        [RelayCommand]
        private async Task EmailBackup(BackupFileItem file)
        {
            var filePath = file.FullPath;
            if (!File.Exists(filePath))
                return;

            try
            {
                var message = new EmailMessage
                {
                    Subject = "TindaKo Backup File",
                    Body = "Attached is a backup file from TindaKo.",
                    To = new List<string> { "jhezavellvelonomansueto@gmail.com" }
                };

                message.Attachments.Add(new EmailAttachment(filePath));
                await Email.Default.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
#if WINDOWS || MACCATALYST
        // Fallback: Open file location and ask user to email manually
        await ToastHelper.ShowMessage("Email not supported on this platform. Please manually send the file.");
        await Launcher.Default.OpenAsync(new OpenFileRequest
        {
            File = new ReadOnlyFile(filePath)
        });
#else
                await ToastHelper.ShowError("Email is not supported on this device.");
#endif
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
                    await App.Navigator.DisplayAlert("Error", $"Sharing failed: {ex.Message}", "OK");
                }
            }
        }

    }
}
