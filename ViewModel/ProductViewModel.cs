using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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
        [ObservableProperty] private int maxResults;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private ObservableCollection<Product> products;
        [ObservableProperty] private ObservableCollection<string> categories;
        //constructor
        public ProductViewModel()
        {
            _ = LoadProducts();
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
                                "Medicines",
                                "Others"
                            };
            MaxResults = Preferences.Get(nameof(MaxResults), 20);
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
            finally
            {
                ClearTextFields();
            }
        }
        [RelayCommand]
        private async Task UpdateProductAsync(Product product)
        {
            if (product == null) return;
            await DatabaseService.UpdateProductAsync(product);
            var toast = Toast.Make("Product updated successfully!", ToastDuration.Short, 14);
            await toast.Show();
        }
        //method for loading products from sqlite

        public async Task LoadProducts()
        {
            List<Product> results;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await DatabaseService.GetProductsAsync(MaxResults); // Loads all (optionally paginate)
            }
            else
            {
                results = await DatabaseService.SearchProductsAsync(SearchText, MaxResults); // Filter at DB level
            }

            Products = new ObservableCollection<Product>(results);
        }



        partial void OnSearchTextChanged(string value)
        {
            _ = LoadProducts(); 
        }

        private void ClearTextFields() { 
            this.ProductName = string.Empty;
            this.ProductCategory = string.Empty;
            this.ProductPrice = 0; 
            this.StocksIn = 0;
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
        [RelayCommand]
        private async Task RefreshProductsAsync()
        {
            Preferences.Set(nameof(MaxResults), MaxResults);
            await LoadProducts();
        }
    }
}
