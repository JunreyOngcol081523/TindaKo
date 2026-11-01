using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class UserSettingsViewModel : ObservableObject
    {
        public const string OwnerRole = "May-ari";
        public const string CashierRole = "Tindera";
        private const int MinPasswordLength = 4;

        [ObservableProperty] private ObservableCollection<Users> users = new();
        [ObservableProperty] private string newUserPassword = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private string saveButtonText = "Save New";
        [ObservableProperty] private bool userFormVisible;

        // The form is bound to a DRAFT copy, never to the object that sits in the list.
        [ObservableProperty] private Users? selectedUser;

        // Fixed list: no reason to rebuild it on every reload (that can reset the Picker's SelectedItem).
        public List<string> UserRoles { get; } = new() { OwnerRole, CashierRole };

        // The real list item being edited (null = adding a new user).
        private Users? editingOriginal;
        private bool IsAddMode => editingOriginal is null;

        // Called from the page's OnAppearing. No more async void in the constructor.
        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            // Don't swap the list while the form is open: the form holds a reference to a list item.
            if (UserFormVisible) return;

            try
            {
                var results = await UserServices.GetUsersAsync();
                Users = new ObservableCollection<Users>(results);
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError($"Loading users: {ex.Message}");
            }
        }

        [RelayCommand]
        private void AddNewUser()
        {
            editingOriginal = null;
            SelectedUser = new Users();
            ResetPasswordFields();
            SaveButtonText = "Save New";
            UserFormVisible = true;
        }

        // Name kept so the existing XAML binding (EditNewUserCommand) still works.
        [RelayCommand]
        private void EditNewUser(Users? user)
        {
            if (user is null) return;

            editingOriginal = user;
            SelectedUser = new Users
            {
                UserName = user.UserName,
                UserFullName = user.UserFullName,
                UserRole = user.UserRole
            };
            ResetPasswordFields();
            SaveButtonText = "Save Changes";
            UserFormVisible = true;
        }

        [RelayCommand]
        private async Task SaveUserAsync()
        {
            if (SelectedUser is null) return;

            var userName = SelectedUser.UserName?.Trim() ?? string.Empty;
            var fullName = SelectedUser.UserFullName?.Trim() ?? string.Empty;
            var role = SelectedUser.UserRole;
            var passwordProvided = !string.IsNullOrEmpty(NewUserPassword);

            // ---- validation ----
            if (userName.Length == 0) { await ToastHelper.ShowError("Username is required."); return; }
            if (fullName.Length == 0) { await ToastHelper.ShowError("Full name is required."); return; }
            if (string.IsNullOrWhiteSpace(role)) { await ToastHelper.ShowError("User role is required."); return; }

            // New user: password required. Existing user: blank = keep current password.
            if (IsAddMode && !passwordProvided) { await ToastHelper.ShowError("Password is required."); return; }
            if (passwordProvided)
            {
                if (NewUserPassword.Length < MinPasswordLength)
                {
                    await ToastHelper.ShowError($"Password must be at least {MinPasswordLength} characters.");
                    return;
                }
                if (!NewUserPassword.Equals(ConfirmPassword))
                {
                    await ToastHelper.ShowError("Passwords do not match.");
                    return;
                }
            }

            bool usernameTaken = Users.Any(u =>
                !ReferenceEquals(u, editingOriginal) &&
                string.Equals(u.UserName?.Trim(), userName, StringComparison.OrdinalIgnoreCase));
            if (usernameTaken) { await ToastHelper.ShowError("That username is already used."); return; }

            // Never leave the store without an owner.
            if (!IsAddMode &&
                editingOriginal!.UserRole == OwnerRole && role != OwnerRole &&
                !Users.Any(u => !ReferenceEquals(u, editingOriginal) && u.UserRole == OwnerRole))
            {
                await ToastHelper.ShowError("There must be at least one May-ari.");
                return;
            }

            // ---- save ----
            Users? original = editingOriginal;
            (string? name, string? full, string? role, string? pass) snapshot = default;

            try
            {
                // PBKDF2 is intentionally slow: keep it off the UI thread.
                string? hash = passwordProvided
                    ? await Task.Run(() => PasswordHasher.Hash(NewUserPassword))
                    : null;

                if (original is null)
                {
                    await UserServices.AddNewUserAsync(new Users
                    {
                        UserName = userName,
                        UserFullName = fullName,
                        UserRole = role,
                        UserPassword = hash!
                    });
                }
                else
                {
                    snapshot = (original.UserName, original.UserFullName, original.UserRole, original.UserPassword);

                    original.UserName = userName;
                    original.UserFullName = fullName;
                    original.UserRole = role;
                    if (hash is not null) original.UserPassword = hash;

                    await UserServices.UpdateUserAsync(original);
                }

                await ToastHelper.ShowMessage(original is null ? "User added." : "User updated.");

                CloseForm();
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                // Roll back the in-memory edit so the list matches the database again.
                if (original is not null && snapshot != default)
                {
                    original.UserName = snapshot.name;
                    original.UserFullName = snapshot.full;
                    original.UserRole = snapshot.role;
                    original.UserPassword = snapshot.pass;
                }
                await ToastHelper.ShowError($"Saving user: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CloseUserForm() => CloseForm();

        private void CloseForm()
        {
            UserFormVisible = false;
            editingOriginal = null;
            SelectedUser = null;
            ResetPasswordFields();
        }

        private void ResetPasswordFields()
        {
            NewUserPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}