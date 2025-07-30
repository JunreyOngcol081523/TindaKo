using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TindaKo.Converters;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;
using Timer = System.Timers.Timer;
namespace TindaKo.ViewModel
{
    public partial class IncomingStocksViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProductDisplay> productList = new();

        [ObservableProperty]
        private ObservableCollection<ProductDisplay> cartList = new();
        private List<ProductDisplay> _allProducts;

        [ObservableProperty]
        private int cartCount;
        [ObservableProperty]
        private string searchText;
        private Timer _debounceTimer;
        [ObservableProperty] private string supplierName;
        public IncomingStocksViewModel()
        {
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var products = await DatabaseService.GetProductsAsync(100);
            _allProducts = products.Select(p => new ProductDisplay(p)).ToList();
            // You should have this service
            ProductList = new ObservableCollection<ProductDisplay>(_allProducts);
        }

        [RelayCommand]
        private void AddToCart(ProductDisplay product)
        {
            if (product.SupplierPrice <= 0 || product.NewStocksQty <= 0)
                return;

            if (!CartList.Any(p => p.ProdId == product.ProdId))
            {
                CartList.Add(new ProductDisplay
                {
                    ProdId = product.ProdId,
                    ProdName = product.ProdName,
                    SupplierPrice = product.SupplierPrice,
                    NewStocksQty = product.NewStocksQty
                });

                CartCount = CartList.Count;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            // Reset timer
            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            _debounceTimer = new Timer(300); // debounce time in milliseconds
            _debounceTimer.Elapsed += (s, e) =>
            {
                _debounceTimer.Stop();
                _debounceTimer.Dispose();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PerformSearch(value);
                });
            };
            _debounceTimer.Start();
        }
        private void PerformSearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                ProductList = new ObservableCollection<ProductDisplay>(_allProducts);
            }
            else
            {
                ProductList = new ObservableCollection<ProductDisplay>(
                    _allProducts.Where(p =>
                        p.ProdName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    ));
            }
        }

        [RelayCommand]
        private async Task OpenCartAsync()
        {
            var popupVm = new IncomingStocksPopupViewModel(this);
            var popup = new IncomingStocksPopup(popupVm);
            popup.Show();
        }

        [RelayCommand]
        private async Task CompleteTransactionAsync()
        {
            var totalAmount = CartList.Sum(item => item.Subtotal);

            var incomingStocks = new IncomingStocks
            {
                DateStocksIn = DateTime.Now,
                SupplierName = this.SupplierName,
                TotalAmout = totalAmount
            };

            bool success = await IncomingStocksService.SaveIncomingStocksAsync(incomingStocks, CartList);

            if (success)
            {
                // Update StocksIn for each product
                foreach (var item in CartList)
                {
                    var product = await ProductService.GetProductByIdAsync(item.ProdId); // fetch from DB
                    if (product != null)
                    {
                        product.StocksIn += item.NewStocksQty;
                        await ProductService.UpdateProductAsync(product);
                    }
                }

                await ToastHelper.ShowMessage("Incoming stocks saved!");
                CartList.Clear(); // clear cart if needed
            }
        }

        [RelayCommand]
        private async Task CloseAsync()
        {
            bool confirmed = await App.Current.MainPage.DisplayAlert(
                "Confirm",
                "Are you sure you want to cancel transaction?",
                "Yes",
                "No");

            if (confirmed)
            {
                // Assuming you're using CommunityToolkit.Maui.Views.Popup
                // and you called `popup.Show()`, then:
                await App.Current.MainPage.Navigation.PopAsync(); // if it's a Page
            }
        }

    }
}


