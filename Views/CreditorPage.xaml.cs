using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class CreditorPage : ContentPage
{
    private readonly CreditorViewModel _vm;

    public CreditorPage(int initialSegment = 0)
    {
        InitializeComponent();

        _vm = new CreditorViewModel { SelectedSegmentIndex = initialSegment };
        BindingContext = _vm;
    }

    // Reload every time the page is shown, so new utang and payments are always current.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _vm.LoadCreditorsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CreditorPage] load failed: {ex}");
        }
    }
}
