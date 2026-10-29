using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Converters;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Views;

namespace TindaKo.ViewModel
{
    public partial class ProductViewModel : ObservableObject
    {
        [ObservableProperty] private string password;
        [ObservableProperty] private string productName;
        [ObservableProperty] private string productCategory;
        [ObservableProperty] private decimal productPrice;
        [ObservableProperty] private int stocksIn;
        [ObservableProperty] private int stocksOut = 0;
        [ObservableProperty] private int stocks;
        [ObservableProperty] private int maxResults;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private ObservableCollection<Product> products;
        [ObservableProperty] private ObservableCollection<string> categories;
        [ObservableProperty] private int newStocksIn;
        [ObservableProperty] private int newStocksOut;
        [ObservableProperty] private bool formVisible;
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        //constructor
        public ProductViewModel()
        {
            _ = LoadProducts();

            _ = LoadCategories();
            MaxResults = Preferences.Get(nameof(MaxResults), 20);
            FormVisible = false;
        }
        public async Task LoadCategories()
        {
            Categories = await CategoriesHelper.LoadCategoriesFromJsonAsync();
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
                    await ProductService.AddProductAsync(product);
                    await ToastHelper.ShowMessage("Product Added");
                    await LoadProducts();
                }
                else
                {
                    await ToastHelper.ShowError("Required fields are empty");
                }
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError(ex.Message);
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
            await ProductService.UpdateProductAsync(product);
            //var toast = Toast.Make("Product updated successfully!", ToastDuration.Short, 14);
            await ToastHelper.ShowMessage("Product Updated");

        }
        //method for loading products from sqlite

        public async Task LoadProducts()
        {
            try
            {
                IsBusy = true;
                await Task.Delay(500);
                List<Product> results;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    results = await ProductService.GetProductsAsync(MaxResults); // Loads all (optionally paginate)
                }
                else
                {
                    results = await ProductService.SearchProductsAsync(SearchText, MaxResults); // Filter at DB level
                }

                Products = new ObservableCollection<Product>(results);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        //method for updating product's stocks in and stocks out
        [RelayCommand]
        private async Task OpenProductStocksAdjustAsync(Product product)
        {
            if (product == null)
            {

                await ToastHelper.ShowError("No Product selected.");
                return;
            }
            else
            {
                var vm = new ProductAdjustViewModel(product);
                var popup = new ProductStockAdjustPopup(vm);
                await App.Navigator.Navigation.PushModalAsync(popup);
                await LoadProducts(); // refresh 
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = LoadProducts();
        }

        private void ClearTextFields()
        {
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
            if (fields.Contains(""))
            {
                return false;
            }
            if (ProductPrice <= 0)
            {
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

        [RelayCommand]
        private async Task AddNewProductAsync()
        {
            FormVisible = true;
        }
        [RelayCommand]
        private async Task CloseProductFormAsync()
        {
            FormVisible = false;
            ClearTextFields();
        }
    }
}
