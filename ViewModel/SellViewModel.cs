using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class SellViewModel : ObservableObject
    {
        // ───────────────────────── State ─────────────────────────

        [ObservableProperty] private ObservableCollection<Product> productList = new();
        [ObservableProperty] private ObservableCollection<CartItems> cartList = new();
        [ObservableProperty] private int cartCount;
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private decimal totalAmount;
        [ObservableProperty] private bool isCreditChecked;
        [ObservableProperty] private bool isCashChecked;
        [ObservableProperty] private string transactionType = "Cash";
        [ObservableProperty] private string? selectedBuyerName;
        [ObservableProperty] private bool isCartHasItems;
        [ObservableProperty] private bool isAllowedUtang;

        public Buyer? SelectedBuyer { get; set; }

        private CancellationTokenSource? _searchCts;
        private int _dueDateDays;

        // ───────────────────────── Ctor / Init ─────────────────────────

        public SellViewModel()
        {
            // Add / Remove / Reset / Replace -> always keep cart state in sync
            CartList.CollectionChanged += (_, _) => RecalculateCart();

            IsCashChecked = true;
            LoadSettings();
        }

        public async Task InitializeAsync()
        {
            LoadSettings(); // settings may have changed since this VM was created
            await LoadProductsAsync();
        }

        private void LoadSettings()
        {
            IsAllowedUtang = Preferences.Get("AllowUtang", false);
            _dueDateDays = Preferences.Get("DueDateDays", 0);
        }

        // Single source of truth for count / total / has-items flag
        private void RecalculateCart()
        {
            CartCount = CartList.Count;
            TotalAmount = CartList.Sum(c => c.Price * c.Quantity);
            IsCartHasItems = CartList.Count > 0;
        }

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

                ProductList = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"error loading products: {ex.Message}");
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
        private async Task AddToCart(Product? product)
        {
            if (product is null) return;

            if (product.Price <= 0 || product.Quantity <= 0)
            {
                await ToastHelper.ShowMessage("Lagyan ng presyo at dami.");
                return;
            }

            if (CartList.Any(c => c.ProductId == product.ProdId))
            {
                await ToastHelper.ShowMessage($"{product.ProdName} ay nasa cart na.");
                return;
            }

            var remainingStocks = product.StocksIn - product.StocksOut;
            if (product.Quantity > remainingStocks)
            {
                await ToastHelper.ShowMessage($"Only {remainingStocks} left in stock. Please reduce the quantity.");
                return;
            }

            CartList.Add(new CartItems
            {
                ProductId = product.ProdId,
                ProdName = product.ProdName,
                ProdCategory = product.Category,
                Price = product.Price,
                Quantity = product.Quantity,
                Subtotal = product.Price * product.Quantity
            });
            // CartCount / TotalAmount / IsCartHasItems -> handled by RecalculateCart
        }

        [RelayCommand]
        private void RemoveItem(CartItems? item)
        {
            if (item is not null)
                CartList.Remove(item);
        }

        [RelayCommand]
        private void OpenCart()
        {
            var popup = new SellPageCartPopup(this);
            popup.Show();
        }

        // ───────────────────────── Payment type ─────────────────────────
        // Cash / Credit are mutually exclusive and one must always be checked.

        partial void OnIsCashCheckedChanged(bool value)
        {
            if (value)
            {
                TransactionType = "Cash";
                IsCreditChecked = false;
                SelectedBuyer = null;
                SelectedBuyerName = string.Empty;
            }
            else if (!IsCreditChecked)
            {
                IsCashChecked = true; // never allow "neither"
            }
        }

        partial void OnIsCreditCheckedChanged(bool value)
        {
            if (value)
            {
                TransactionType = "Credit";
                IsCashChecked = false;
            }
            else if (!IsCashChecked)
            {
                IsCashChecked = true; // never allow "neither"
            }
        }

        // choose buyer nga umutang
        [RelayCommand]
        public async Task OpenBuyerModalAsync()
        {
            var buyerPage = new BuyerPage();
            var buyerViewModel = new BuyerViewModel
            {
                OnBuyerSelected = buyer =>
                {
                    SelectedBuyer = buyer;
                    SelectedBuyerName = buyer.Name;
                }
            };

            buyerPage.BindingContext = buyerViewModel;
            await App.Navigator.Navigation.PushModalAsync(buyerPage);
        }

        // ───────────────────────── Complete transaction ─────────────────────────

        [RelayCommand]
        private async Task CompleteTransactionAsync()
        {
            LoadSettings(); // validate against the CURRENT utang setting, not the one from when the page opened

            // 1. Validation
            if (CartList.Count == 0)
            {
                await ToastHelper.ShowError("Walang laman ang cart.");
                return;
            }

            if (IsCreditChecked && !IsAllowedUtang)
            {
                await ToastHelper.ShowError("Hindi pinapayagan ang utang.");
                return;
            }

            if (IsCreditChecked && SelectedBuyer is null)
            {
                await ToastHelper.ShowError("Pakilagay ang pangalan ng umutang.");
                return;
            }

            // 2. Confirm
            bool confirm = await App.Navigator.DisplayAlert(
                "Kompiramahin",
                $"Review your cart items:\n\n {BuildCartSummary()}",
                "Tama lahat", "Hindi");

            if (!confirm) return;

            // 3. Save. The service saves sale + stock + utang as ONE database transaction,
            //    so on failure NOTHING was saved and the cart is kept for a retry.
            var (saved, error) = await SaveTransactionAsync();

            if (!saved)
            {
                await App.Navigator.DisplayAlert(
                    "Error", error ?? "Hindi na-save ang transaksyon. Subukan ulit.", "OK");

                await LoadProductsAsync(); // stock may have changed -> show the real numbers
                return;
            }

            // 4. Next step
            bool again = await App.Navigator.DisplayAlert(
                "Tagumpay", "Tapos na ang transaction!\nGusto mo bang gumawa ulit?", "Oo", "Hindi");

            // The sale is finished either way -> never leave sold items sitting in the cart.
            ClearForm();

            if (again)
                await LoadProductsAsync(); // refresh stocks + reset row quantities
            else
                await App.Navigator.Navigation.PopAsync();
        }

        private async Task<(bool Saved, string? Error)> SaveTransactionAsync()
        {
            if (CartList.Count == 0) return (false, null); // last line of defense

            // Snapshot everything BEFORE any await so UI changes can't affect the save.
            var now = DateTime.Now;
            var buyer = SelectedBuyer;
            var isCredit = IsCreditChecked;
            var total = CartList.Sum(c => c.Price * c.Quantity);
            var dueDateDays = _dueDateDays;

            if (isCredit && buyer is null) return (false, null);

            var transaction = new Transaction
            {
                BuyerID = isCredit ? buyer!.BuyerId : null,
                TransactionType = isCredit ? "Credit" : "Cash",
                DateTimeTransacted = now.ToString("MMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture),
                TotalAmount = total,
                Username = Preferences.Get("username", string.Empty)
            };

            var soldItems = CartList.Select(ci => new Sell
            {
                ProductID = ci.ProductId,
                ProductName = ci.ProdName,
                Category = ci.ProdCategory,
                Price = ci.Price,
                Qty = ci.Quantity
            }).ToList();

            Creditor? creditor = null;
            if (isCredit)
            {
                creditor = new Creditor
                {
                    CreditorName = buyer!.Name,
                    CreditAmount = total,
                    CreditDate = now,
                    DueDate = now.AddDays(dueDateDays),
                    IsPaid = false
                };
            }

            try
            {
                await TransactionServices.SaveTransactionWithSalesAsync(transaction, soldItems, creditor);
                return (true, null);
            }
            catch (InsufficientStockException ex)
            {
                return (false, ex.Message); // precise, user-facing message
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Complete Sell transaction] {ex}");
                return (false, null);
            }
        }

        private void ClearForm()
        {
            CartList.Clear(); // RecalculateCart resets count, total and has-items flag
            IsCreditChecked = false;
            IsCashChecked = true;
            TransactionType = "Cash";
            SelectedBuyer = null;
            SelectedBuyerName = string.Empty;
        }

        // ───────────────────────── Cancel ─────────────────────────

        [RelayCommand]
        private async Task CloseSellAsync()
        {
            // Nothing to lose -> leave without asking
            if (CartList.Count == 0)
            {
                await App.Navigator.Navigation.PopAsync();
                return;
            }

            bool answer = await App.Navigator.DisplayAlert(
                "Kumpirmasyon",
                "Gusto mo bang kanselahin ang kasalukuyang transaksyon ng pagbebenta?",
                "Oo", "Hindi");

            if (answer)
            {
                ClearForm();
                await App.Navigator.Navigation.PopAsync();
            }
        }

        // ───────────────────────── Helpers ─────────────────────────

        private string BuildCartSummary()
        {
            var sb = new StringBuilder();
            decimal grandTotal = 0;

            sb.AppendLine("Product Name       |  Price  | Qty | Subtotal");

            foreach (var item in CartList)
            {
                var subtotal = item.Price * item.Quantity;
                grandTotal += subtotal;

                var name = item.ProdName ?? string.Empty;
                if (name.Length > 20) name = name[..20];

                sb.AppendLine($"{name.PadRight(20)}| {item.Price,7} | {item.Quantity,3} | {subtotal,8}");
            }

            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Total:{"".PadLeft(35)}| {grandTotal,8}");
            sb.AppendLine();
            sb.AppendLine("Review the items above before you confirm.");

            return sb.ToString();
        }
    }
}