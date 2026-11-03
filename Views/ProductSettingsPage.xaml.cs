using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ProductSettingsPage : ContentPage
{
    private readonly ProductSettingsViewModel _vm;

    public ProductSettingsPage()
    {
        InitializeComponent();
        _vm = new ProductSettingsViewModel();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _vm.LoadCategoriesAsync();   // refresh every time the tab is shown
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Category load failed: {ex}");
        }
    }
}