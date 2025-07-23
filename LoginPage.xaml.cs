using TindaKo.ViewModel;

namespace TindaKo
{
    public partial class LoginPage : ContentPage
    {

        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

    }
}
