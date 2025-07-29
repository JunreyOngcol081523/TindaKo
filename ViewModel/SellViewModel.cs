using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class SellViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Product> products;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private ObservableCollection<CartItems> cartItems;
        [ObservableProperty] private decimal totalAmount;
        [ObservableProperty] private bool isCreditChecked;
        [ObservableProperty] private bool isCashChecked;
        [ObservableProperty] private string transactionType;
        public Buyer SelectedBuyer { get; set; } // in SellViewModel
        [ObservableProperty] private string selectedBuyerName;
        [ObservableProperty] private bool isCartHasItems;
        public SellViewModel()
        {
            _ = LoadProducts();
            CartItems = new ObservableCollection<CartItems>();
            IsCashChecked = true;
            CartItems.CollectionChanged += CartItems_CollectionChanged;
            IsCartHasItems = false;
        }



        public async Task LoadProducts()
        {
            var allProducts = await DatabaseService.GetProductsAsync(10) ?? new List<Product>();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(allProducts);
            }
            else
            {
                var filtered = allProducts
                    .Where(p =>
                        (p.StocksIn - p.StocksOut) > 0 && // ✅ Only products with remaining stock
                        (
                            (!string.IsNullOrEmpty(p.ProdName) && p.ProdName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(p.Category) && p.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        )
                    )
                    .Take(5)
                    .ToList();

                Products = new ObservableCollection<Product>(filtered);
            }
        }
        partial void OnSearchTextChanged(string value)
        {
            _ = LoadProducts();
        }
        [RelayCommand]
        public async Task AddToCart(Product product)
        {
            try
            {
                if (product == null)
                {
                    System.Diagnostics.Debug.WriteLine("AddToCart: product is null");
                    return;
                }

                if (product.Quantity <= 0)
                {
                    await Toast.Make("Please enter quantity greater than zero.", ToastDuration.Short).Show();
                    return;
                }
                // 🔒 Check if Quantity exceeds remaining stocks
                var remainingStocks = product.StocksIn - product.StocksOut;
                if (product.Quantity > remainingStocks)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Insufficient Stock",
                        $"Only {remainingStocks} left in stock. Please reduce the quantity.",
                        "OK");
                    return;
                }

                var existing = CartItems.FirstOrDefault(c => c.ProdName == product.ProdName);
                if (existing != null)
                {
                    await Toast.Make($"{product.ProdName} is already in the cart.", ToastDuration.Short).Show();
                    return;
                }

                CartItems.Add(new CartItems
                {
                    ProductId = product.ProdId,
                    ProdName = product.ProdName,
                    ProdCategory = product.Category,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    Subtotal = product.Price * product.Quantity
                });

                TotalAmount += product.Price * product.Quantity;

                // Reset quantity to avoid reusing
                product.Quantity = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ AddToCart Exception: {ex}");
                await ToastHelper.ShowError("Something went wrong while adding to cart.");
            }
        }
        //remove items from cart
        [RelayCommand]
        public async Task RemoveFromCart(CartItems item)
        {
            if (item == null) return;

            // Update total
            TotalAmount -= item.Subtotal;

            // Remove from list
            CartItems.Remove(item);
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
            await Application.Current.MainPage.Navigation.PushModalAsync(buyerPage);
        }
        //complete transaction method
        [RelayCommand]
        private async Task CompleteTransactionAsync()
        {

            // Check if buyer is required
            if (IsCreditChecked && SelectedBuyer == null)
            {
                await Toast.Make("Pakilagay ang pangalan ng umutang.", ToastDuration.Short).Show();
            }
            else
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Kompiramahin", $"Review your cart items:\n\n {BuildCartSummary()}", "Tama lahat", "Hindi");

                if (confirm)
                {
                    SaveTransaction();
                    bool again = await Application.Current.MainPage.DisplayAlert("Tagumpay", "Tapos na and transaction!\n Gusto mo bang gumawa ulit?", "Oo", "Hindi");

                    if (again)
                    {
                        ClearForm();
                    }
                    else
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
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
            CartItems.Clear();
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

                var soldItems = CartItems.Select(ci => new Sell
                {
                    ProductID = ci.ProductId,
                    ProductName = ci.ProdName,
                    Category = ci.ProdCategory,
                    Price = ci.Price,
                    Qty = ci.Quantity
                }).ToList();
                
                await DatabaseService.SaveTransactionWithSalesAsync(transaction, soldItems);
                if (IsCreditChecked)
                {
                    var creditor = new Creditor
                    {
                        CreditorName = SelectedBuyer.Name,
                        CreditAmount = TotalAmount,
                        CreditDate = DateTime.Now,
                        DueDate = DateTime.Now.AddMonths(1),
                        IsPaid = false
                    };
                    await DatabaseService.SaveCreditorAsync(creditor);
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
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Kumpirmasyon",
                "Gusto mo bang kanselahin ang kasalukuyang transaksyon ng pagbebenta?",
                "Oo", "Hindi");


            if (answer)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }
        private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
        private void CheckCartItems()
        {
            if (CartItems.Count <= 0)
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

            foreach (var item in CartItems)
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

    }
}
