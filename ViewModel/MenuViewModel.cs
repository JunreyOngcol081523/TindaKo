using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class MenuViewModel:ObservableObject
    {
        [ObservableProperty] private string userName;
        public MenuViewModel() { 
            UserName =$"Welcome! {Preferences.Get("userfullname", string.Empty)}";
        }
        [RelayCommand]
        private async Task NavigateSellAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SellPage(new SellViewModel()));
        }
        [RelayCommand]
        private async Task NavigateProductAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ProductPage(new ProductViewModel()));
        }
        [RelayCommand]
        private async Task NavigateSalesAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SalesPage(new SalesViewModel()));
        }
        [RelayCommand]
        private async Task NavigateInventoryAsync()
        {

        }
        [RelayCommand]
        private async Task NavigateSettingsAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SettingsPage());
        }
        [RelayCommand]
        private async Task NavigateReportsAsync()
        {

        }
        [RelayCommand]
        private async Task NavigateDatabaseAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new DatabaseBackupPage(new DBViewModel()));
        }
        [RelayCommand]
        private async Task NavigateCreditsAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new CreditorPage());
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage(new LoginViewModel()));
        }

    }
}
