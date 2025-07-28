using System.Diagnostics;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.ViewModel;

namespace TindaKo
{
    public partial class App : Application
    {
        public static NavigationPage Navigator;
        public App()
        {
            InitializeComponent();
            Navigator = new NavigationPage(new LoginPage(new LoginViewModel()));
            MainPage = Navigator;
            Application.Current.UserAppTheme = AppTheme.Light;
            
        }


    }
}