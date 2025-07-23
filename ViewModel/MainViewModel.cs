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
    public partial class MainViewModel : ObservableObject
    {
        DatabaseService databaseService;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string userPassword;

        [ObservableProperty]
        private string selectedRole; // "May-ari" or "Tindera"

        public MainViewModel() {
            selectedRole = "May-ari";
        }

        //------------user login-------------//
        [RelayCommand]
        public async Task LoginAsync()
        {
            await DatabaseService.InitAsync(); // Make sure DB is ready

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserPassword) || string.IsNullOrWhiteSpace(SelectedRole))
            {
                await Shell.Current.DisplayAlert("Login Failed", "Please fill in all fields.", "OK");
                return;
            }

            var user = await DatabaseService.GetUserAsync(UserName, UserPassword);

            if (user != null && user.UserRole == SelectedRole)
            {
                await Shell.Current.DisplayAlert("Login Successful", $"Welcome, {user.UserRole}!", "OK");

                // TODO: Navigate to appropriate page
                // Example:
                // await Shell.Current.GoToAsync("///MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Login Failed", "Invalid username, password, or role.", "OK");
            }
        }
    }
}
