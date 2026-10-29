using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TindaKo.Helpers;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {

        [ObservableProperty]
        private string userName;
        [ObservableProperty]
        private string userFullName;

        [ObservableProperty]
        private string userPassword;

        [ObservableProperty]
        private string selectedRole; // "May-ari" or "Tindera"

        [ObservableProperty]
        private bool rememberLogin;
        public LoginViewModel() {
            selectedRole = "May-ari";
            LoadSettings();
        }

        //------------user login-------------//
        [RelayCommand]
        public async Task LoginAsync()
        {

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserPassword) || string.IsNullOrWhiteSpace(SelectedRole))
            {
                await ToastHelper.ShowError("Please fill in all fields.");
                return;
            }

            var user = await UserServices.GetUserAsync(UserName, UserPassword);
            
            if (user != null && user.UserRole == SelectedRole)
            {
                UserFullName = user.UserFullName;
                Preferences.Set("userrole", user.UserRole);
                Preferences.Set("remember", RememberLogin);
                Preferences.Set("username", UserName);
                Preferences.Set("password", UserPassword);
                Preferences.Set("userfullname", UserFullName);
                await ToastHelper.ShowMessage($"Welcome, {user.UserName}!");
                App.SetNavigator(new NavigationPage(new MenuPage(new MenuViewModel())));
                
            }
            else
            {
                await ToastHelper.ShowError("Invalid username, password, or role.");
            }
        }

        //-----------load remember user----------//
        private void LoadSettings() {
            DatabaseService.GetConnectionAsync();
            RememberLogin = Preferences.Get("remember",false);
            UserName = Preferences.Get("username",string.Empty);
        }
        [RelayCommand]
        public async Task ResetDBAsync()
        {
            //confirmation dialog
            var confirm = await App.Navigator.DisplayAlert("Confirm Reset", "Are you sure you want to reset the database? This action cannot be undone.", "Yes", "No");
            if (confirm)
            {
                await DatabaseService.ResetDB();
               
            }

            else
            {
                await ToastHelper.ShowMessage("Database reset cancelled.");
            }
            await DatabaseService.GetConnectionAsync();
        }
    }
}
