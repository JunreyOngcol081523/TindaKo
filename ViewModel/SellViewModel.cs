using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public SellViewModel()
        {
            _ = LoadProducts();
            CartItems = new ObservableCollection<CartItems>();
            IsCashChecked = true;
        }
        public async Task LoadProducts()
        {
            var allProducts = await DatabaseService.GetProductsAsync(20) ?? new List<Product>();

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
            _=LoadProducts();
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
                    await Application.Current.MainPage.DisplayAlert("Invalid Quantity", "Please enter quantity greater than zero.", "OK");
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
                    await Application.Current.MainPage.DisplayAlert("Already Added", $"{product.ProdName} is already in the cart.", "OK");
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
                await Application.Current.MainPage.DisplayAlert("Error", "Something went wrong while adding to cart.", "OK");
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
            TransactionType = "Cash";
            SelectedBuyer = null;
            SelectedBuyerName = string.Empty;
        }
        partial void OnIsCreditCheckedChanged(bool value)
        {
            TransactionType = "Credit";
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
                await Application.Current.MainPage.DisplayAlert("Success", "Transaction completed!", "OK");
                CartItems.Clear();
                TotalAmount = 0;
                IsCashChecked = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Complete Transaction Method:]", ex.Message);
            }
        }

    }
}
