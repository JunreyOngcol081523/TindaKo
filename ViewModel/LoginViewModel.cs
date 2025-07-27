using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userPassword;

        [ObservableProperty]
        private string selectedRole; // "May-ari" or "Tindera"

        [ObservableProperty]
        private bool rememberUsername;
        public LoginViewModel() {
            selectedRole = "May-ari";
            LoadSettings();
        }

        //------------user login-------------//
        [RelayCommand]
        public async Task LoginAsync()
        {
            await DatabaseService.InitAsync(); // Make sure DB is ready

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserPassword) || string.IsNullOrWhiteSpace(SelectedRole))
            {
                await Application.Current.MainPage.DisplayAlert("Login Failed", "Please fill in all fields.", "OK");
                return;
            }

            var user = await DatabaseService.GetUserAsync(UserName, UserPassword);

            if (user != null && user.UserRole == SelectedRole)
            {
                await Application.Current.MainPage.DisplayAlert("Login Successful", $"Welcome, {user.UserName}!", "OK");
                Application.Current.MainPage = new NavigationPage(new MenuPage(new MenuViewModel()));
                Preferences.Set("remember", RememberUsername);
                Preferences.Set("username",UserName);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Login Failed", "Invalid username, password, or role.", "OK");
            }
        }

        //-----------load remember user----------//
        private void LoadSettings() { 
            RememberUsername = Preferences.Get("remember",false);
            UserName = Preferences.Get("username",string.Empty);
        }
    }
}
