using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
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
    public partial class IncomingStocksViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProductDisplay> productList = new();

        [ObservableProperty]
        private ObservableCollection<ProductDisplay> cartList = new();

        [ObservableProperty]
        private int cartCount;

        public IncomingStocksViewModel()
        {
            _=LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var products = await DatabaseService.GetProductsAsync(100); // You should have this service
            ProductList = new ObservableCollection<ProductDisplay>(
                products.Select(p => new ProductDisplay
                {
                    ProdId = p.ProdId,
                    ProdName = p.ProdName
                }));
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
    }

}
