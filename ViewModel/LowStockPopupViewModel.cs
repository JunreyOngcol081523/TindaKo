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
using TindaKo.Services.TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class LowStockPopupViewModel:ObservableObject
    {
        public ObservableCollection<Product> LowStockProducts { get; set; } = new();

        public async Task LoadLowStockProductsAsync()
        {
            var result = await ProductService.GetLowStockProductsAsync();
            LowStockProducts.Clear();
            foreach (var item in result)
                LowStockProducts.Add(item);
        }

        [RelayCommand]
        private async Task ShareLowStockProductsAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("⚠ Low Stock Products");
            sb.AppendLine("Product Name, Remaining Stock");

            foreach (var item in LowStockProducts)
                sb.AppendLine($"{item.ProdName}, {item.CurrentStock}");

            var fileName = $"LowStockProducts_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var filePath = await FileService.SaveTextToFileAsync(fileName, sb.ToString());

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share Low Stock Products",
                File = new ShareFile(filePath)
            });
        }
    }
}
