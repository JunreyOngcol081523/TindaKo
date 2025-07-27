using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class UserSettingsPage : ContentPage
{
	public UserSettingsPage()
	{
		InitializeComponent();
        BindingContext = new UserSettingsViewModel(); // Replace with actual ViewModel
    }
}