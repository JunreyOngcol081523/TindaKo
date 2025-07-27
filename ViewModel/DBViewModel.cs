
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                //  show a toast or log success
                await Toast.Make("Backup Done.", ToastDuration.Short).Show();
            }
        }

        [RelayCommand]
        private async Task Restore() {
           bool success = await DatabaseBackupService.RestoreDatabaseAsync(destinationPath);
            if (success)
            {
                //  show a toast or log success
                await Toast.Make("Restoration Done.", ToastDuration.Short).Show();
            }
        } 

        [RelayCommand]
        private async Task SendToEmail() => await DatabaseBackupService.EmailAllDbBackupsAsync();

        [RelayCommand]
        private async Task ShareVia() => await DatabaseBackupService.ShareAllDbBackupsAsync();
    }
}
