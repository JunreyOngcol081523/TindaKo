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
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(UserPassword) ||
                string.IsNullOrWhiteSpace(SelectedRole))
            {
                await ToastHelper.ShowError("Please fill in all fields.");
                return;
            }

            try
            {
                var user = await UserServices.GetUserByUsernameAsync(UserName.Trim());

                if (user == null)
                {
                    await ToastHelper.ShowError("Invalid username, password, or role.");
                    return;
                }

                // Verify password using PBKDF2
                bool passwordValid = await Task.Run(() =>
                    PasswordHasher.Verify(UserPassword, user.UserPassword)
                );

                if (!passwordValid || user.UserRole != SelectedRole)
                {
                    await ToastHelper.ShowError("Invalid username, password, or role.");
                    return;
                }

                // Upgrade legacy plaintext passwords automatically
                if (!PasswordHasher.IsHashed(user.UserPassword))
                {
                    user.UserPassword = await Task.Run(() =>
                        PasswordHasher.Hash(UserPassword)
                    );

                    await UserServices.UpdateUserAsync(user);
                }

                UserFullName = user.UserFullName;

                Preferences.Set("userrole", user.UserRole);
                Preferences.Set("remember", RememberLogin);
                Preferences.Set("username", user.UserName);
                Preferences.Set("userfullname", UserFullName);

                // Never save plaintext passwords
                Preferences.Remove("password");

                await ToastHelper.ShowMessage($"Welcome, {user.UserName}!");

                App.SetNavigator(
                    new NavigationPage(new MenuPage(new MenuViewModel()))
                );
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError($"Login failed: {ex.Message}");
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
