using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.Views
{

    public partial class ProductAdjustViewModel : ObservableObject
    {

        public ProductAdjustViewModel(Product product)
        {
            _product = product ?? throw new ArgumentNullException(nameof(product));
            ReadMe = "StocksIn – Items added to inventory (restocks, returns)\r\nStocksOut – Items manually removed (damaged, lost, expired)";
        }

        public Product _product { get; }

        [ObservableProperty]
        private int newStocksIn = 0;

        [ObservableProperty]
        private int newStocksOut = 0;

        [ObservableProperty] private string readMe;
        [RelayCommand]
        private async Task AdjustProductStocksAsync()
        {
            if (_product == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No Selected Product to Adjust.", "OK");
            }
            else
            {
                _product.StocksIn += NewStocksIn;
                _product.StocksOut += NewStocksOut;
                await DatabaseService.UpdateProductAsync(_product);
                // Optionally: save to DB
                await Application.Current.MainPage.DisplayAlert("Success", "Stocks adjusted.", "OK");

                await Application.Current.MainPage.Navigation.PopModalAsync();
            }

        }
    }

}
