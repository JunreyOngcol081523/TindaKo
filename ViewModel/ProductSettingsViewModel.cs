using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ClosedXML.Excel;
using TindaKo.Models;
using TindaKo.Services;
using System.Diagnostics;
namespace TindaKo
{
    public partial class ProductSettingsViewModel : ObservableObject
    {
        [RelayCommand]
        private async Task DownloadTemplateAsync()
        {
            try
            {
                // 1. Load the template from the app package
                using var inputStream = await FileSystem.OpenAppPackageFileAsync("Resources/Templates/ProductTemplate.xlsx");

                // 2. Define output file name and path
                var fileName = "ProductTemplate.xlsx";
                var outputPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                // 3. Copy stream to local storage
                using var outputStream = File.Create(outputPath);
                await inputStream.CopyToAsync(outputStream);

                await Application.Current.MainPage.DisplayAlert("Download Complete", $"Template saved to:\n{outputPath}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }



        [RelayCommand]
        public async Task ImportTemplateAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("Resources/Templates/ProductTemplate.xlsx");
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var products = new List<Product>();

                foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header row
                {
                    var product = new Product
                    {
                        ProdName = row.Cell(1).GetString(),
                        Category = row.Cell(2).GetString(),
                        Price = Convert.ToDecimal(row.Cell(3).Value),
                        StocksIn = Convert.ToDecimal(row.Cell(4).Value),
                        StocksOut = 0
                    };

                    products.Add(product);
                }

                foreach (var product in products)
                {
                    await DatabaseService.AddProductAsync(product);
                }

                await Application.Current.MainPage.DisplayAlert("Success", "Products imported successfully.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to import: {ex.Message}", "OK");
            }
        }
    }
}