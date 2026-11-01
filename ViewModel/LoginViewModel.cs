using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using TindaKo.Helpers;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        // Brute-force brake. The counters are static so they survive the ViewModel being
        // recreated (e.g. after logout). They reset when the app restarts.
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromSeconds(30);
        private static int failedAttempts;
        private static DateTime lockedUntilUtc = DateTime.MinValue;

        private IDispatcherTimer? lockoutTimer;

        [ObservableProperty] private string userName = string.Empty;
        [ObservableProperty] private string userFullName = string.Empty;
        [ObservableProperty] private string userPassword = string.Empty;
        [ObservableProperty] private string selectedRole = UserSettingsViewModel.OwnerRole; // "May-ari" or "Tindera"
        [ObservableProperty] private bool rememberLogin;

        // Lockout state shown on the login page
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool isLockedOut;

        [ObservableProperty] private string lockoutMessage = string.Empty;

        public LoginViewModel()
        {
            LoadSettings();
            _ = WarmUpDatabaseAsync();

            // Page was recreated while a lockout is still running: resume the countdown.
            if (DateTime.UtcNow < lockedUntilUtc)
                StartLockoutCountdown();
        }

        // ------------------------------------------------------------------
        // LOGIN
        // ------------------------------------------------------------------
        private bool CanLogin() => !IsLockedOut;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(UserPassword) ||
                string.IsNullOrWhiteSpace(SelectedRole))
            {
                await ToastHelper.ShowError("Please fill in all fields.");
                return;
            }

            // Safety net: Execute() on the command (e.g. keyboard "Done") does not check CanExecute.
            if (DateTime.UtcNow < lockedUntilUtc)
            {
                StartLockoutCountdown();
                return;
            }

            try
            {
                await DatabaseService.GetConnectionAsync(); // make sure the DB is ready (cheap if already open)

                var user = await UserServices.GetUserByUsernameAsync(UserName.Trim());

                bool passwordValid = user is not null &&
                    await Task.Run(() => PasswordHasher.Verify(UserPassword, user.UserPassword));

                if (user is null || !passwordValid || user.UserRole != SelectedRole)
                {
                    RegisterFailedAttempt();
                    await ToastHelper.ShowError("Invalid username, password, or role.");
                    return;
                }

                failedAttempts = 0;

                // Upgrade a legacy plaintext password. A failure here must not block a valid login.
                if (!PasswordHasher.IsHashed(user.UserPassword))
                {
                    try
                    {
                        user.UserPassword = await Task.Run(() => PasswordHasher.Hash(UserPassword));
                        await UserServices.UpdateUserAsync(user);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Password upgrade failed: {ex.Message}");
                    }
                }

                UserFullName = user.UserFullName;

                Preferences.Set("userrole", user.UserRole);
                Preferences.Set("userfullname", UserFullName);
                Preferences.Set("remember", RememberLogin);

                // "Remember me" must actually mean something: only keep the username when it is ticked.
                if (RememberLogin) Preferences.Set("username", user.UserName);
                else Preferences.Remove("username");

                Preferences.Remove("password"); // never keep a password (cleans up old versions)

                UserPassword = string.Empty;
                lockoutTimer?.Stop();

                await ToastHelper.ShowMessage($"Welcome, {user.UserName}!");

                App.SetNavigator(new NavigationPage(new MenuPage(new MenuViewModel())));
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError($"Login failed: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------
        // LOCKOUT COUNTDOWN
        // ------------------------------------------------------------------
        private void RegisterFailedAttempt()
        {
            failedAttempts++;
            if (failedAttempts >= MaxFailedAttempts)
            {
                failedAttempts = 0;
                lockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
                StartLockoutCountdown();
            }
        }

        private void StartLockoutCountdown()
        {
            UpdateLockoutState();
            if (!IsLockedOut) return;

            lockoutTimer ??= Application.Current!.Dispatcher.CreateTimer();
            lockoutTimer.Interval = TimeSpan.FromSeconds(1);
            lockoutTimer.Tick -= OnLockoutTick;
            lockoutTimer.Tick += OnLockoutTick;
            lockoutTimer.Start();
        }

        private void OnLockoutTick(object? sender, EventArgs e) => UpdateLockoutState();

        private void UpdateLockoutState()
        {
            var remaining = lockedUntilUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                IsLockedOut = false;
                LockoutMessage = string.Empty;
                lockoutTimer?.Stop();
                return;
            }

            IsLockedOut = true;
            LockoutMessage = $"Too many attempts. Try again in {(int)Math.Ceiling(remaining.TotalSeconds)}s";
        }

        // ------------------------------------------------------------------
        // STARTUP
        // ------------------------------------------------------------------
        private void LoadSettings()
        {
            RememberLogin = Preferences.Get("remember", false);
            UserName = RememberLogin ? Preferences.Get("username", string.Empty) : string.Empty;
        }

        // Was an un-awaited call in the constructor (exceptions were lost). Now observed and logged.
        private static async Task WarmUpDatabaseAsync()
        {
            try
            {
                await DatabaseService.GetConnectionAsync();
                await PasswordMigration.HashLegacyPasswordsAsync(); // hashes anyone who hasn't logged in yet
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DB warm-up failed: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------
        // RESET DATABASE
        // ------------------------------------------------------------------
        [RelayCommand]
        public async Task ResetDBAsync()
        {
            var confirm = await App.Navigator.DisplayAlert(
                "Confirm Reset",
                "Are you sure you want to reset the database? This action cannot be undone.",
                "Yes", "No");

            if (!confirm)
            {
                await ToastHelper.ShowMessage("Database reset cancelled.");
                return;
            }

            // Second gate: this button is reachable by anyone who can open the app.
            var typed = await App.Navigator.DisplayPromptAsync(
                "Final confirmation",
                "This permanently deletes ALL data (products, sales, utang, users). Type RESET to continue.",
                accept: "Delete everything",
                cancel: "Cancel",
                placeholder: "RESET",
                maxLength: 10);

            if (!string.Equals(typed?.Trim(), "RESET", StringComparison.Ordinal))
            {
                await ToastHelper.ShowMessage("Database reset cancelled.");
                return;
            }

            try
            {
                await DatabaseService.ResetDB();
                await DatabaseService.GetConnectionAsync();

                UserPassword = string.Empty;
                await ToastHelper.ShowMessage("Database has been reset.");
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError($"Reset failed: {ex.Message}");
            }
        }
    }
}