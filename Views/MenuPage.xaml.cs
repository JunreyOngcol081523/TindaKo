using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class MenuPage : ContentPage
{
	public MenuPage(MenuViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}