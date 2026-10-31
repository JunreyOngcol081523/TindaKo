using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class IncomingStocksViewModel : ObservableObject
    {
        // ───────────────────────── State ─────────────────────────

        [ObservableProperty] private ObservableCollection<ProductDisplay> productList = new();
        [ObservableProperty] private ObservableCollection<ProductDisplay> cartList = new();
        [ObservableProperty] private int cartCount;
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string supplierName = string.Empty;

        private CancellationTokenSource? _searchCts;

        // ───────────────────────── Ctor / Init ─────────────────────────

        public IncomingStocksViewModel()
        {
            // Add / Remove / Reset -> CartCount can never go out of sync
            CartList.CollectionChanged += (_, _) => CartCount = CartList.Count;

            _ = InitializeAsync();
        }

        public async Task InitializeAsync() => await LoadProductsAsync();

        // ───────────────────────── Products & Search ─────────────────────────

        private async Task LoadProductsAsync(CancellationToken token = default)
        {
            try
            {
                var products = string.IsNullOrWhiteSpace(SearchText)
                    ? await ProductService.GetProductsAsync(100)
                    : await ProductService.SearchProductsAsync(SearchText, 100);

                // a newer search superseded this one -> drop the stale result
                if (token.IsCancellationRequested) return;

                ProductList = new ObservableCollection<ProductDisplay>(
                    products.Select(p => new ProductDisplay(p)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IncomingStocks] error loading products: {ex}");
            }
        }

        partial void OnSearchTextChanged(string value) => _ = DebouncedSearchAsync();

        private async Task DebouncedSearchAsync()
        {
            var previous = _searchCts;
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            previous?.Cancel();
            previous?.Dispose();

            try
            {
                await Task.Delay(300, token);
                await LoadProductsAsync(token);
            }
            catch (OperationCanceledException) { }
        }

        // ───────────────────────── Cart ─────────────────────────

        [RelayCommand]
        private async Task AddToCart(ProductDisplay? product)
        {
            if (product is null) return;

            if (product.SupplierPrice <= 0 || product.NewStocksQty <= 0)
            {
                await ToastHelper.ShowMessage("Enter a supplier price and quantity greater than zero.");
                return;
            }

            if (CartList.Any(p => p.ProdId == product.ProdId))
            {
                await ToastHelper.ShowMessage($"{product.ProdName} is already in the cart.");
                return;
            }

            CartList.Add(new ProductDisplay
            {
                ProdId = product.ProdId,
                ProdName = product.ProdName,
                SupplierPrice = product.SupplierPrice,
                NewStocksQty = product.NewStocksQty
            });
            // CartCount is updated by the CollectionChanged handler
        }

        [RelayCommand]
        private void OpenCart()
        {
            var popupVm = new IncomingStocksPopupViewModel(this);
            var popup = new IncomingStocksPopup(popupVm);
            popup.Show();
        }

        // ───────────────────────── Complete transaction ─────────────────────────

        [RelayCommand]
        private async Task CompleteTransactionAsync()
        {
            // 1. Validation (use the real collection, not a separate counter)
            if (CartList.Count == 0)
            {
                await ToastHelper.ShowError("Your cart is empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SupplierName))
            {
                await ToastHelper.ShowError("Please enter the supplier name.");
                return;
            }

            // 2. Confirm
            bool confirm = await App.Navigator.DisplayAlert(
                "Completeness Check",
                "Click [Yes] if you are done with adding stocks.\nClick [No] if not.",
                "Yes", "No");

            if (!confirm) return;

            // 3. Snapshot BEFORE any await so UI changes can't affect the save
            var items = CartList.ToList();
            var supplier = SupplierName.Trim();

            // 4. Save. ONE database transaction: delivery record + product lines +
            //    stock increase + new average cost. If it throws, NOTHING was saved,
            //    so the cart is kept and the user can retry safely.
            try
            {
                var incomingStocks = new IncomingStocks
                {
                    DateStocksIn = DateTime.Now,
                    SupplierName = supplier
                    // TotalAmount is computed inside the service
                };

                await StockReceivingService.ReceiveStockAsync(incomingStocks, items);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IncomingStocks] save failed: {ex}");
                await ToastHelper.ShowError("Failed to save incoming stocks. Nothing was changed.");
                return;
            }

            // 5. Saved. Clear the cart so the same delivery can't be logged twice.
            ClearForm();
            await LoadProductsAsync(); // refresh stock numbers + reset the input rows

            await ToastHelper.ShowMessage("Incoming stocks saved successfully.");
        }

        private void ClearForm()
        {
            CartList.Clear(); // CartCount resets via CollectionChanged
            SupplierName = string.Empty;
        }

        // ───────────────────────── Navigation ─────────────────────────

        [RelayCommand]
        private async Task CloseAsync()
        {
            // Nothing to lose -> leave without asking
            if (CartList.Count == 0)
            {
                await App.Navigator.Navigation.PopAsync();
                return;
            }

            bool confirmed = await App.Navigator.DisplayAlert(
                "Confirm",
                "Are you sure you want to cancel transaction?",
                "Yes", "No");

            if (confirmed)
            {
                ClearForm();
                await App.Navigator.Navigation.PopAsync();
            }
        }

        [RelayCommand]
        private async Task OpenLogsAsync()
        {
            await App.Navigator.Navigation.PushAsync(new IncomingStocksLogsPage());
        }
    }
}