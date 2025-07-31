using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class MenuPage : ContentPage
{
    private readonly MenuViewModel _viewModel;
    public MenuPage(MenuViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        _viewModel = vm;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Example: Load today's total sales
        await _viewModel.LoadDataFromDB();
    }
}