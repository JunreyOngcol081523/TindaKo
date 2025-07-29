using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class UserSettingsViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Users> users;
        [ObservableProperty] private string newUsername;
        [ObservableProperty] private string newUserPassword;
        [ObservableProperty] private string newUserFullname;
        [ObservableProperty] private string newUserRole;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string saveButtonText;
        [ObservableProperty] private List<string> userRoles;
        [ObservableProperty] private bool isAddCommand;
        [ObservableProperty] private bool userFormVisible;
        private Users selectedUser;
        public Users SelectedUser
        {
            get => selectedUser;
            set => SetProperty(ref selectedUser, value);
        }

        public UserSettingsViewModel() {
            LoadUsers();
            UserFormVisible = false;
        }
        private async void LoadUsers()
        {
            var results = await DatabaseService.GetUsersAsync();
            Users = new ObservableCollection<Users>();
            foreach (var user in results)
            {
                Users.Add(user);
            }
            UserRoles = new List<string> { "May-ari", "Tindera" };
        }
        [RelayCommand]
        private void AddNewUser()
        {
            IsAddCommand = true;
            SelectedUser = new Users(); // new empty user
            NewUserPassword = string.Empty;
            ConfirmPassword = string.Empty;
            SaveButtonText = "Save New";
            UserFormVisible = true;
        }

        [RelayCommand]
        private void EditNewUser(Users user)
        {

            IsAddCommand = false;
            SelectedUser = user; // This fills the form
            NewUserPassword = string.Empty;
            ConfirmPassword = string.Empty;
            SaveButtonText = "Save Changes";
            SelectedUser = user;
            UserFormVisible = true;
        }
        [RelayCommand]
        private async Task SaveUserAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedUser?.UserName))
            {
                await ToastHelper.ShowError("Username is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedUser?.UserFullName))
            {
                await ToastHelper.ShowError("Full name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedUser?.UserRole))
            {
                await ToastHelper.ShowError("User role is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewUserPassword))
            {
                await ToastHelper.ShowError("Password is required.");
                return;
            }

            if (!NewUserPassword.Equals(ConfirmPassword))
            {
                await ToastHelper.ShowError($"Password not match");
                return;
            }

            try
            {
                if (IsAddCommand)
                {
                    var newuser = new Users
                    {
                        UserName = SelectedUser.UserName,
                        UserFullName = SelectedUser.UserFullName,
                        UserPassword = NewUserPassword,
                        UserRole = SelectedUser.UserRole
                    };

                    await DatabaseService.AddNewUserAsync(newuser);
                }
                else
                {
                    // Directly update SelectedUser (includes correct UserId)
                    SelectedUser.UserPassword = NewUserPassword;
                    await DatabaseService.UpdateUserAsync(SelectedUser);
                }

                await ToastHelper.ShowMessage(IsAddCommand ? "User added." : "User updated.");

                // Refresh list and hide form
                LoadUsers();
                UserFormVisible = false;
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError($"Saving user: {ex.Message}");
            }
        }
        [RelayCommand]
        private async Task CloseUserFormAsync()
        {
            UserFormVisible = false;
        }
    }
}
