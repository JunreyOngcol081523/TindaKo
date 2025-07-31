using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Platform;
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
            if (string.IsNullOrEmpty(SearchText)) {
               var products = await ProductService.GetProductsAsync(100);
                _allProducts = products.Select(p => new ProductDisplay(p)).ToList();
            }
            else
            {
                var products = await ProductService.SearchProductsAsync(SearchText, 100);
                _allProducts = products.Select(p => new ProductDisplay(p)).ToList();
            }
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
                    _=LoadProductsAsync();
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
            if (CartCount <= 0)
            {
                await ToastHelper.ShowError("Your cart is empty.");
                return;
            }

            var mainPage = Application.Current?.MainPage;
            if (mainPage == null) return;

            bool confirm = await mainPage.DisplayAlert(
                "Completeness Check",
                "Click [Yes] if you are done with adding stocks.\nClick [No] if not.",
                "Yes", "No");

            if (!confirm) return;

            decimal totalAmount = CartList.Sum(item => item.Subtotal);

            var incomingStocks = new IncomingStocks
            {
                DateStocksIn = DateTime.Now,
                SupplierName = this.SupplierName,
                TotalAmout = totalAmount
            };

            bool success = await IncomingStocksService.SaveIncomingStocksAsync(incomingStocks, CartList);

            if (!success)
            {
                await ToastHelper.ShowError("Failed to save incoming stocks.");
                return;
            }

            // Update StocksIn for each product
            foreach (var item in CartList)
            {
                var product = await ProductService.GetProductByIdAsync(item.ProdId);
                if (product != null)
                {
                    product.StocksIn += item.NewStocksQty;
                    await ProductService.UpdateProductAsync(product);
                }
            }

            await ToastHelper.ShowMessage("Incoming stocks saved successfully.");
            CartList.Clear();
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


