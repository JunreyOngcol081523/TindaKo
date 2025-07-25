using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class SalesPage : ContentPage
{
	public SalesPage(SalesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}