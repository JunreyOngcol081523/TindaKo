using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class IncomingStocksLogsViewModel:ObservableObject
    {
        public List<string> Months { get; set; }
        [ObservableProperty] string selectedMonth;
        [ObservableProperty] int selectedYear;
        [ObservableProperty] List<IncomingStocks> incomingStocksList;
        [ObservableProperty] List<IncomingStocksProductsList> incomingStocksProductsList;
        [ObservableProperty] string _username;
        [ObservableProperty] DateTime dateStocksIn;
        [ObservableProperty] string supplierName;
        [ObservableProperty] decimal totalAmount;
        public List<int> Years { get; set; }
        //constructor
        public IncomingStocksLogsViewModel()
        {
            Months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
            .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();
            SelectedMonth = DateTime.Now.ToString("MMMM", CultureInfo.CurrentCulture);
            //populate the list of years
            Years = Enumerable.Range(DateTime.Now.Year - 10, 11).ToList();
            SelectedYear = DateTime.Now.Year;
            Username = Preferences.Get("userfullname", ""); // Assuming you store the username in preferences
        }
        private bool _isFilterEnabled;

        public bool IsFilterEnabled
        {
            get => _isFilterEnabled;
            set
            {
                _isFilterEnabled = value;
                OnPropertyChanged();
            }
        }
        public async Task LoadIncomingStocksLogsAsync()
        {
           
            if (_isFilterEnabled)
            {
                IncomingStocksList = await Services.IncomingStocksLogsService.GetIncomingStocksLogsByMonthYearAsync(SelectedMonth, SelectedYear);
            }
            else
            {
                IncomingStocksList = await Services.IncomingStocksLogsService.GetIncomingStocksLogsAsync();
            }

        }
        public async Task LoadIncomingStocksProductsListAsync(int incomingStocksId)
        {
            IncomingStocksProductsList = await Services.IncomingStocksLogsService.GetIncomingStocksProductsListAsync(incomingStocksId);
        }
        [RelayCommand]
        private async Task ItemTappedAsync(IncomingStocks selectedIncomingStocks)
        {
            if (selectedIncomingStocks == null)
                return;
            // Load the products for the selected incoming stocks
            DateStocksIn = selectedIncomingStocks.DateStocksIn;
            SupplierName = selectedIncomingStocks.SupplierName;
            TotalAmount = selectedIncomingStocks.TotalAmount;
            await LoadIncomingStocksProductsListAsync(selectedIncomingStocks.IncomingStocksId);
            IncomingStocksDetailsPage detailsPage = new IncomingStocksDetailsPage(this);
            await Application.Current.MainPage.Navigation.PushModalAsync(detailsPage);
        }
        [RelayCommand]
        private async Task ShowReportAsync() {
            await LoadIncomingStocksLogsAsync();
        }
        [RelayCommand]
        private async Task CloseDetailsAsync()
        {
                       // Close the details page
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
    }
}
