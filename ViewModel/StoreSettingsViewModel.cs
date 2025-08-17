using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;

namespace TindaKo.ViewModel
{
    public partial class StoreSettingsViewModel:ObservableObject
    {
        [ObservableProperty]
        private string _storeName;
        [ObservableProperty]
        private string _storeAddress;
        [ObservableProperty]
        private string _storeEmail;
        [ObservableProperty]
        private string _storePhoneNum;
        public StoreSettingsViewModel()
        { 
            StoreName = Preferences.Get("StoreName", String.Empty);
            StoreAddress = Preferences.Get("StoreAddress", String.Empty);
            StoreEmail = Preferences.Get("StoreEmail", String.Empty);
            StorePhoneNum = Preferences.Get("StorePhoneNum", String.Empty);
        }
        [RelayCommand]
        private async Task SaveStoreSettings()
        {
            Preferences.Set("StoreName", StoreName);
            Preferences.Set("StoreAddress", StoreAddress);
            Preferences.Set("StoreEmail", StoreEmail);
            Preferences.Set("StorePhoneNum", StorePhoneNum);
            await ToastHelper.ShowMessage("Store Settings Saved");
        }
    }
}
