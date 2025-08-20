using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
using TindaKo.Services.TindaKo.Services;
using Microsoft.Maui.Storage;
namespace TindaKo.ViewModel
{
    public partial class FastSellingProductViewModel : ObservableObject
    {
        private ObservableCollection<FastSellingProductDto> _fastSellingProducts;
        public ObservableCollection<FastSellingProductDto> FastSellingProducts
        {
            get => _fastSellingProducts;
            set => SetProperty(ref _fastSellingProducts, value);
        }
        //constructor
        public FastSellingProductViewModel()
        {
            FastSellingProducts = new ObservableCollection<FastSellingProductDto>();
        }
        public async Task LoadFastSellingProductsAsync()
        {
            var result = await ProductService.GetFastMovingProductsAsync();
            FastSellingProducts.Clear();
            foreach (var item in result)
                FastSellingProducts.Add(item);
        }


        [RelayCommand]
        private async Task ShareFastSellingProductsAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Fast Selling Products");
            sb.AppendLine("Product Name, Total Sold, Total Amount");
            if(FastSellingProducts.Count == 0)
            {
                await ToastHelper.ShowMessage("No fast selling products to share");
                return;
            }
            foreach (var item in FastSellingProducts)
            {
                sb.AppendLine($"{item.ProductName}, {item.TotalQty}");
            }

            var fileName = $"TindaKoFastSellingProducts_{DateTime.Now:yyyyMM}.txt";
            var filePath = await FileService.SaveTextToFileAsync(fileName, sb.ToString());

            // Show a toast that it was saved
            await ToastHelper.ShowMessage("Fast selling products saved successfully");

            // Share the file
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share Fast Selling Products",
                File = new ShareFile(filePath)
            });
        }

    }
}
