using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLitePCL;
using TindaKo.Helpers;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class MenuViewModel : ObservableObject
    {
        [ObservableProperty] private bool isAdmin;
        [ObservableProperty] private string userName;
        [ObservableProperty] private string userRole;
        [ObservableProperty] private decimal dailySales;
        [ObservableProperty] private decimal monthlySales;
        [ObservableProperty] private int dueUnpaidCredits;
        [ObservableProperty] private int lowStocksCount;
        [ObservableProperty] private int fastMovingProductsCount;
        public MenuViewModel()
        {
            UserName = Preferences.Get("userfullname", string.Empty);
            UserRole = Preferences.Get("userrole", "unkown");
            _ = GetUserRole();
        }
        public async Task LoadDataFromDB()
        {
            DailySales = await TransactionServices.GetTodayTotalSalesAsync();
            MonthlySales = await TransactionServices.GetMonthlyTotalSalesAsync();
            DueUnpaidCredits = await CreditorServices.GetOverdueUnpaidCreditsCountAsync();
            LowStocksCount = await ProductService.CountLowStockProductsAsync();
            var result = await ProductService.GetFastMovingProductsAsync();
            FastMovingProductsCount = result.Count;
        }
        public async Task GetUserRole()
        {
            if (UserRole.Equals("May-ari"))
            {
                IsAdmin = true;
            }
            else
            {
                IsAdmin = false;
            }
        }
        [RelayCommand]
        private async Task NavigateSellAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SellPage(new SellViewModel()));
        }
        [RelayCommand]
        private async Task NavigateProductAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ProductPage(new ProductViewModel()));
        }
        [RelayCommand]
        private async Task NavigateSalesAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SalesPage(new SalesViewModel()));
        }
        [RelayCommand]
        private async Task NavigateIncomingStocksAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new IncomingStocksPage(new IncomingStocksViewModel()));
        }
        [RelayCommand]
        private async Task NavigateSettingsAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SettingsPage());
        }
        [RelayCommand]
        private async Task NavigateReportsAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ReportsPage(new ReportsViewModel()));
        }
        [RelayCommand]
        private async Task NavigateDatabaseAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new DatabaseBackupPage(new DBViewModel()));
        }
        [RelayCommand]
        private async Task NavigateCreditsAsync()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new CreditorPage(0));
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            Preferences.Set("remember", false);
            Preferences.Set("username", string.Empty);
            Preferences.Set("password", string.Empty);
            Preferences.Set("userfullname", string.Empty);
            Application.Current.MainPage = new NavigationPage(new LoginPage(new LoginViewModel()));
        }

        [RelayCommand]
        private async Task NavigateFastSellingProductsAsync()
        {
            // Create popup
            var popup = new FastSellingPopup();

            // Assign a new VM to popup
            var vm = new FastSellingProductViewModel();
            popup.BindingContext = vm;

            // Load products before showing
            await vm.LoadFastSellingProductsAsync();

            popup.Show();

        }

        [RelayCommand]
        private async Task NavigateLowStockProductsAsync()
        {
            // Create popup
            var popup = new LowStockPopup();
            // Assign a new VM to popup
            var vm = new LowStockPopupViewModel();
            popup.BindingContext = vm;
            // Load products before showing
            await vm.LoadLowStockProductsAsync();
            popup.Show();
        }
        [RelayCommand]
        private async Task NavigateUnpaidDueCredits()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new CreditorPage(3));
        }
    }
}
