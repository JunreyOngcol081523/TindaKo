using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class UserSettingsViewModel:ObservableObject
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
        private Users selectedUser;
        public Users SelectedUser
        {
            get => selectedUser;
            set => SetProperty(ref selectedUser, value);
        }

        public UserSettingsViewModel() {
            LoadUsers();
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
        }

        [RelayCommand]
        private void EditNewUser(Users user)
        {
            
            IsAddCommand = false;
            SelectedUser = user; // This fills the form
            NewUserPassword = string.Empty;
            ConfirmPassword = string.Empty;
            SaveButtonText = "Save Changes";
        }


    }
}
