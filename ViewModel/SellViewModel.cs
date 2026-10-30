using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using TindaKo.Converters;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;
using Timer = System.Timers.Timer;
namespace TindaKo.ViewModel
{
    public partial class SellViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Product> productList = new();

        [ObservableProperty]
        private ObservableCollection<CartItems> cartList = new();
        private List<Product> _allProducts;

        [ObservableProperty]
        private int cartCount;
        [ObservableProperty]
        private string searchText;
        private Timer _debounceTimer;
        [ObservableProperty] private decimal totalAmount;
        [ObservableProperty] private bool isCreditChecked;
        [ObservableProperty] private bool isCashChecked;
        [ObservableProperty] private string transactionType;
        public Buyer? SelectedBuyer { get; set; } // in SellViewModel
        [ObservableProperty] private string selectedBuyerName;
        [ObservableProperty] private bool isCartHasItems;
        [ObservableProperty] private bool isAllowedUtang;
        private int _dueDateDays;
        public SellViewModel()
        {
            CartList.CollectionChanged += CartList_CollectionChanged;
            IsCashChecked = true;
            IsCartHasItems = false;
            IsAllowedUtang = Preferences.Get("AllowUtang", false);
            _dueDateDays = Preferences.Get("DueDateDays", 0);
        }

        private void CartList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    CheckCartItems();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    CheckCartItems();
                }
            }
        }

        public async Task InitializeAsync()
        {
            await LoadProductsAsync();
        }



        private async Task LoadProductsAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(SearchText))
                {
                    _allProducts = await ProductService.GetProductsAsync(100);
                }
                else
                {
                    _allProducts = await ProductService.SearchProductsAsync(SearchText, 100);
                }
                ProductList = new ObservableCollection<Product>(_allProducts);
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"error loading products: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddToCart(Product product)
        {
            if (product.Price <= 0 || product.Quantity <= 0)
                return;
            var remainingStocks = product.StocksIn - product.StocksOut;
            if (product.Quantity > remainingStocks)
            {

                await ToastHelper.ShowMessage($"Only {remainingStocks} left in stock. Please reduce the quantity.");
                return;
            }

            if (!CartList.Any(p => p.ProductId == product.ProdId))
            {
                CartList.Add(new CartItems
                {
                    ProductId = product.ProdId,
                    ProdName = product.ProdName,
                    ProdCategory = product.Category,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    Subtotal = product.Subtotal
                });
                TotalAmount += product.Price * product.Quantity;
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
                    _ = LoadProductsAsync();
                });
            };
            _debounceTimer.Start();
        }
        partial void OnIsCashCheckedChanged(bool value)
        {
            if (value)
            {
                TransactionType = "Cash";
                IsCreditChecked = false;
                SelectedBuyer = null;
                SelectedBuyerName = string.Empty;
            }
        }
        partial void OnIsCreditCheckedChanged(bool value)
        {
            if (value)
            {
                TransactionType = "Credit";
                IsCashChecked = false;
            }
        }
        //choose buyer nga umutang
        [RelayCommand]
        public async Task OpenBuyerModalAsync()
        {
            var buyerPage = new BuyerPage();
            var buyerViewModel = new BuyerViewModel();

            buyerViewModel.OnBuyerSelected = (buyer) =>
            {
                SelectedBuyer = buyer;
                SelectedBuyerName = buyer.Name;
            };

            buyerPage.BindingContext = buyerViewModel;
            await App.Navigator.Navigation.PushModalAsync(buyerPage);
        }
        //complete transaction method
        [RelayCommand]
        private async Task CompleteTransactionAsync()
        {

            // Check if buyer is required
            if (IsCreditChecked && SelectedBuyer == null)
            {
                await ToastHelper.ShowError("Pakilagay ang pangalan ng umutang.");
            }
            else
            {
                bool confirm = await App.Navigator.DisplayAlert("Kompiramahin", $"Review your cart items:\n\n {BuildCartSummary()}", "Tama lahat", "Hindi");

                if (confirm)
                {
                    SaveTransaction();
                    bool again = await App.Navigator.DisplayAlert("Tagumpay", "Tapos na and transaction!\n Gusto mo bang gumawa ulit?", "Oo", "Hindi");

                    if (again)
                    {
                        ClearForm();
                    }
                    else
                    {
                        await App.Navigator.Navigation.PopAsync();
                    }
                }
                else
                {
                    return;
                }

            }
        }

        private void ClearForm()
        {
            CartList.Clear();
            TotalAmount = 0;
            IsCashChecked = true;
            SelectedBuyer = null;
            SelectedBuyerName = null;
        }

        private async void SaveTransaction()
        {

            try
            {
                var transaction = new Transaction
                {
                    BuyerID = SelectedBuyer?.BuyerId,
                    TransactionType = IsCashChecked ? "Cash" : "Credit",
                    DateTimeTransacted = DateTime.Now.ToString("MMM dd, yyyy h:mm tt"),
                    TotalAmount = TotalAmount,
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
                
                await TransactionServices.SaveTransactionWithSalesAsync(transaction, soldItems);
                if (IsCreditChecked)
                {
                    var creditor = new Creditor
                    {
                        CreditorName = SelectedBuyer.Name,
                        CreditAmount = TotalAmount,
                        CreditDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(_dueDateDays),
                        IsPaid = false
                    };
                    await CreditorServices.SaveCreditorAsync(creditor);
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"[Complete Sell transaction] {ex.Message}");
            }
            
        }
        [RelayCommand]
        private async Task CloseSellAsync()
        {
            bool answer = await App.Navigator.DisplayAlert(
                "Kumpirmasyon",
                "Gusto mo bang kanselahin ang kasalukuyang transaksyon ng pagbebenta?",
                "Oo", "Hindi");


            if (answer)
            {
                await App.Navigator.Navigation.PopAsync();
            }
        }
    
        private void CheckCartItems()
        {
            if (CartList.Count <= 0)
            {
                IsCartHasItems = false;
            }
            else
            {
                IsCartHasItems = true;
            }
        }
        private string BuildCartSummary()
        {
            var sb = new StringBuilder();
            decimal grandTotal = 0;

            sb.AppendLine("Product Name       |  Price  | Qty | Subtotal");

            foreach (var item in CartList)
            {
                grandTotal += item.Subtotal;

                sb.AppendLine(
                    $"{item.ProdName.PadRight(20)}| {item.Price,7} | {item.Quantity,3} | {item.Subtotal,8}");
            }

            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"Total:{"".PadLeft(35)}| {grandTotal,8}");
            sb.AppendLine();
            sb.AppendLine("Review the items above before you confirm.");

            return sb.ToString();
        }
        [RelayCommand]
        private async Task OpenCartAsync()
        {
            var popup = new SellPageCartPopup(this);
            popup.Show();
        }
        [RelayCommand]
        private async Task RemoveItemAsync(CartItems cartItems)
        {
            if (CartList.Contains(cartItems))
            {
                CartList.Remove(cartItems);
                CartCount = CartList.Count; // 🔁 Sync cart count
                TotalAmount -=cartItems.Subtotal;
            }
        }
    }
}
//public async Task AddToCart(Product product)
//{
//    try
//    {
//        if (product == null)
//        {
//            System.Diagnostics.Debug.WriteLine("AddToCart: product is null");
//            return;
//        }

//        if (product.Quantity <= 0)
//        {
//            await Toast.Make("Please enter quantity greater than zero.", ToastDuration.Short).Show();
//            return;
//        }
//        // 🔒 Check if Quantity exceeds remaining stocks
//        var remainingStocks = product.StocksIn - product.StocksOut;
//        if (product.Quantity > remainingStocks)
//        {
//            await Application.Current.MainPage.DisplayAlert(
//                "Insufficient Stock",
//                $"Only {remainingStocks} left in stock. Please reduce the quantity.",
//                "OK");
//            return;
//        }

//        var existing = CartItems.FirstOrDefault(c => c.ProdName == product.ProdName);
//        if (existing != null)
//        {
//            await Toast.Make($"{product.ProdName} is already in the cart.", ToastDuration.Short).Show();
//            return;
//        }

//        CartItems.Add(new CartItems
//        {
//            ProductId = product.ProdId,
//            ProdName = product.ProdName,
//            ProdCategory = product.Category,
//            Price = product.Price,
//            Quantity = product.Quantity,
//            Subtotal = product.Price * product.Quantity
//        });

//        TotalAmount += product.Price * product.Quantity;

//        // Reset quantity to avoid reusing
//        product.Quantity = 0;
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"❌ AddToCart Exception: {ex}");
//        await ToastHelper.ShowError("Something went wrong while adding to cart.");
//    }
//}