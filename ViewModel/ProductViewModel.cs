using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class ProductViewModel:ObservableObject
    {
        [ObservableProperty] private string productName;
        [ObservableProperty] private string productCategory;
        [ObservableProperty] private decimal productPrice;
        [ObservableProperty] private int stocksIn;
        [ObservableProperty] private int stocksOut=0;
        [ObservableProperty] private int stocks;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private ObservableCollection<Product> products;
        [ObservableProperty] private ObservableCollection<string> categories;
        //constructor
        public ProductViewModel()
        {
            LoadProducts();
            categories = new ObservableCollection<string>
                            {
                                "Food and Snacks",
                                "Beverages",
                                "Cooking Essentials",
                                "Personal Care",
                                "Laundry and Cleaning",
                                "Household Items",
                                "School Supplies",
                                "Mobile and Load",
                                "Cigarettes and Alcohol",
                                "Others"
                            };

        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            try
            {
                if (ValidatedRequireFields())
                {
                    Product product = new Product
                    {
                        ProdName = ProductName,
                        Category = ProductCategory,
                        Price = ProductPrice,
                        StocksIn = StocksIn,
                        StocksOut = StocksOut
                    };
                    await DatabaseService.AddProductAsync(product);
                    await Application.Current.MainPage.DisplayAlert("Info", "Product Added", "ok");
                    await LoadProducts();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Warning","Required fields are empty", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error Adding Product", ex.Message, "OK");
            }
        }
        [RelayCommand]
        private async Task EditProductAsync()
        {

        }
        //method for loading products from sqlite

        public async Task LoadProducts()
        {
            Products = new ObservableCollection<Product>();
            var _products = await DatabaseService.GetProductsAsync();
            foreach (var product in _products)
            {
                Products.Add(product);
            }
        }

        private void ClearTextFields() { 
            
        }
        private bool ValidatedRequireFields()
        {
            List<string> fields = new List<string>();
            fields.Add(ProductName);
            fields.Add(ProductCategory);
            if(fields.Contains(""))
            {
                return false;
            }
            if (ProductPrice <= 0) { 
                return false;
            }
            return true;
        }
    }
}
