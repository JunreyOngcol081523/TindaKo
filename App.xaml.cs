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
        }

    }
}