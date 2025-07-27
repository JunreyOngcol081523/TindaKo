using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ProductSettingsPage : ContentPage
{
	public ProductSettingsPage()
	{
		InitializeComponent();
		BindingContext = new ProductSettingsViewModel();

    }
}