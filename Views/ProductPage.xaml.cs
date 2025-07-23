using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ProductPage : ContentPage
{
	public ProductPage(ProductViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}