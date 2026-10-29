using System.Diagnostics;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.ViewModel;
using TindaKo.Views;

namespace TindaKo
{
    public partial class App : Application
    {
        public static NavigationPage Navigator { get; private set; } = null!;
        public App()
        {
            InitializeComponent();
            Task.Run(async () => await DatabaseService.GetConnectionAsync());
            bool isRemembered = Preferences.Get("remember", false);

            if (isRemembered)
            {
                // Auto-login
                Navigator = new NavigationPage(new MenuPage(new MenuViewModel()));
            }
            else
            {
                Navigator = new NavigationPage(new LoginPage(new LoginViewModel()));
            }
            
            UserAppTheme = AppTheme.Light;
            
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(Navigator);
        }

        public static void SetNavigator(NavigationPage navigator)
        {
            Navigator = navigator;
            if (Current?.Windows.FirstOrDefault() is Window window)
            {
                window.Page = navigator;
            }
        }

    }
}