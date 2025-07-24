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

namespace TindaKo.ViewModel
{
    public partial class SellViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<Product> products;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private ObservableCollection<CartItems> cartItems;
        public SellViewModel()
        {
            _ = LoadProducts();
            CartItems = new ObservableCollection<CartItems>();
        }
        public async Task LoadProducts()
        {
            var allProducts = await DatabaseService.GetProductsAsync() ?? new List<Product>();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(allProducts);
            }
            else
            {
                var filtered = allProducts
                    .Where(p => (!string.IsNullOrEmpty(p.ProdName) && p.ProdName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(p.Category) && p.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
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
            if (product is null || product.Quantity <= 0)
                return;

            CartItems.Add(new CartItems
            {
                ProdName = product.ProdName,
                Price = product.Price,
                Quantity = product.Quantity,
                Subtotal = product.Price * product.Quantity
            });

            product.Quantity = 0; ;
        }
    }
}
