using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ProductPage : ContentPage
{
	public ProductPage(ProductViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is ProductViewModel viewModel)
		{
			await viewModel.LoadCategories();
			await viewModel.LoadProducts();
		}
	}
}