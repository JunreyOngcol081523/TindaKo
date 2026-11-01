using TindaKo.ViewModel;

namespace TindaKo.Views;

public partial class UserSettingsPage : ContentPage
{
    private readonly UserSettingsViewModel _viewModel = new();

    public UserSettingsPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;
    }

    // Reloads every time the tab is shown (e.g. after a backup restore).
    // LoadUsersAsync handles its own exceptions, so async void is safe here.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUsersCommand.ExecuteAsync(null);
    }
}