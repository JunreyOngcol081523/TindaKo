using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class StoreSettings : ContentPage
{
	public StoreSettings()
	{
		InitializeComponent();
		BindingContext = new StoreSettingsViewModel(); // Replace with actual ViewModel
    }
}