using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Mail;
using TindaKo.Helpers;

namespace TindaKo.ViewModel
{
    public partial class StoreSettingsViewModel : ObservableObject
    {
        [ObservableProperty] private string _storeName = AppSettings.StoreName;
        [ObservableProperty] private string _storeAddress = AppSettings.StoreAddress;
        [ObservableProperty] private string _storeEmail = AppSettings.StoreEmail;
        [ObservableProperty] private string _storePhoneNum = AppSettings.StorePhoneNum;
        [ObservableProperty] private bool _allowUtang = AppSettings.AllowUtang;
        [ObservableProperty] private int _dueDateDays = AppSettings.DueDateDays;

        [RelayCommand]
        private async Task SaveStoreSettings()
        {
            var name = (StoreName ?? string.Empty).Trim();
            var address = (StoreAddress ?? string.Empty).Trim();
            var email = (StoreEmail ?? string.Empty).Trim();
            var phone = (StorePhoneNum ?? string.Empty).Trim();

            if (name.Length == 0)
            {
                await ToastHelper.ShowError("Store name is required.");
                return;
            }

            if (email.Length > 0 && !(MailAddress.TryCreate(email, out var parsed) && parsed.Address == email))
            {
                await ToastHelper.ShowError("Store email is not valid.");
                return;
            }

            if (phone.Length > 0 && !IsValidPhone(phone))
            {
                await ToastHelper.ShowError("Store phone number is not valid.");
                return;
            }

            if (AllowUtang && DueDateDays < 1)
            {
                await ToastHelper.ShowError("Set the days before due date (at least 1).");
                return;
            }

            // Show the trimmed values back in the form, then persist.
            StoreName = name;
            StoreAddress = address;
            StoreEmail = email;
            StorePhoneNum = phone;

            AppSettings.StoreName = name;
            AppSettings.StoreAddress = address;
            AppSettings.StoreEmail = email;
            AppSettings.StorePhoneNum = phone;
            AppSettings.AllowUtang = AllowUtang;
            AppSettings.DueDateDays = DueDateDays;

            await ToastHelper.ShowMessage("Store Settings Saved");
        }

        // digits plus + - ( ) and spaces; 7 to 15 digits (PH mobile is 11, landlines vary)
        private static bool IsValidPhone(string phone)
        {
            if (!phone.All(c => char.IsDigit(c) || "+-() ".Contains(c))) return false;
            int digits = phone.Count(char.IsDigit);
            return digits is >= 7 and <= 15;
        }
    }
}