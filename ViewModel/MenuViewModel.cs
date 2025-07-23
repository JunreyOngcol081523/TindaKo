using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class MenuViewModel:ObservableObject
    {
        [ObservableProperty] private string userName;
        public MenuViewModel() { 
            UserName =$"Welcome! {Preferences.Get("username",string.Empty)}";
        }
        [RelayCommand]
        private async Task NavigateSellAsync()
        {
            

        }
        [RelayCommand]
        private async Task NavigateProductAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ProductPage(new ProductViewModel()));
        }
        [RelayCommand]
        private async Task NavigateSalesAsync()
        {

        }
        [RelayCommand]
        private async Task NavigateInventoryAsync()
        {

        }
        [RelayCommand]
        private async Task NavigateSettingsAsync()
        {

        }
        [RelayCommand]
        private async Task NavigateReportsAsync()
        {

        }
        [RelayCommand]
        private async Task LogoutAsync()
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage(new LoginViewModel()));
        }
    }
}
