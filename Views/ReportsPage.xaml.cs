using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class ReportsPage : ContentPage
{
    private readonly ReportsViewModel _vm;

    public ReportsPage(ReportsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    // Reload every time the page is shown, so sales made since last visit are included.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _vm.RefreshAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReportsPage] refresh failed: {ex}");
        }
    }
}