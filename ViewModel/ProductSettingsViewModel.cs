using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using TindaKo.Converters;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;
namespace TindaKo
{
    public partial class ProductSettingsViewModel : ObservableObject
    {
        private CancellationTokenSource _categorySaveDebounceToken;

        public ProductSettingsViewModel() {
            _ = LoadCategoriesIntoEditorAsync();
        }
        [ObservableProperty]
        private string categoriesText;

        [ObservableProperty]
        private ObservableCollection<string> categoryList;


        [ObservableProperty] private bool isBusy;
        [RelayCommand]
        private async Task DownloadTemplateAsync()
        {
            try
            {
                // Load the embedded template
                using var inputStream = await FileSystem.OpenAppPackageFileAsync("Resources/Templates/ProductTemplate.xlsx");

                // Define your custom path
                string baseFolder;
#if WINDOWS || MACCATALYST
        baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // or MyDownloads
#else
                baseFolder = FileSystem.AppDataDirectory; // For Android/iOS fallback
#endif
                string folderPath = Path.Combine(baseFolder, "TindaKo", "Templates");

                // Ensure the folder exists
                Directory.CreateDirectory(folderPath);

                // Full file path
                string fileName = "ProductTemplate.xlsx";
                string outputPath = Path.Combine(folderPath, fileName);

                // Save the file
                using var outputStream = File.Create(outputPath);
                await inputStream.CopyToAsync(outputStream);

                // Notify success
                
                await ToastHelper.ShowMessage($"Saved to: {outputPath}", "Download Complete");
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError(ex.Message);
            }
        }


        [RelayCommand]
        private async Task ImportTemplateAsync()
        {
            IsBusy = true;
            try
            {
                var customFileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xlsx" } },
                    { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                    { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
                    { DevicePlatform.MacCatalyst, new[] { "com.microsoft.excel.xlsx" } },
                });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Excel Template",
                    FileTypes = customFileTypes
                });

                if (result == null)
                {
                    await ToastHelper.ShowError("No file selected.");
                    return;
                }

                if (!Path.GetExtension(result.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    await ToastHelper.ShowError("Please select a .xlsx file.");
                    return;
                }

                using var stream = await result.OpenReadAsync();

                // ✅ READ EXCEL FILE AND INSERT INTO DB
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var products = new List<Product>();

                foreach (var row in worksheet.RowsUsed().Skip(1)) // skip header
                {
                    try
                    {
                        var product = new Product
                        {
                            ProdName = row.Cell(1).GetString(),
                            Category = row.Cell(2).GetString(),
                            Price = decimal.TryParse(row.Cell(3).GetFormattedString(), out var price) ? price : 0,
                            StocksIn = decimal.TryParse(row.Cell(4).GetFormattedString(), out var stocksIn) ? stocksIn : 0,
                            StocksOut = decimal.TryParse(row.Cell(5).GetFormattedString(), out var stocksOut) ? stocksOut : 0
                        };

                        products.Add(product);
                    }
                    catch (Exception ex)
                    {
                        // Log or ignore invalid rows if needed
                        System.Diagnostics.Debug.WriteLine($"Row error: {ex.Message}");
                    }
                }

                // Save to database
                foreach (var product in products)
                {
                    await ProductService.AddProductAsync(product);
                }

                await ToastHelper.ShowMessage("Products imported successfully.");
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        public async Task SaveCategoriesToJsonAsync()
        {

            try
            {
                    var categories = CategoriesText?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();

                if (categories is null || categories.Count == 0)
                    return;

                var json = JsonSerializer.Serialize(categories);

                var filePath = Path.Combine(FileSystem.AppDataDirectory, "categories.json");
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                await ToastHelper.ShowError(ex.Message);
            }
        }
        public async Task LoadCategoriesIntoEditorAsync()
        {
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "categories.json");

            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                var categories = JsonSerializer.Deserialize<List<string>>(json);
                CategoriesText = string.Join(", ", categories ?? []);
            }
        }
        partial void OnCategoriesTextChanged(string value)
        {
            _categorySaveDebounceToken?.Cancel(); // Cancel any previous pending save
            _categorySaveDebounceToken = new CancellationTokenSource();

            var token = _categorySaveDebounceToken.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1500, token); // Wait 1.5 seconds

                    if (!token.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(SaveCategoriesToJsonAsync);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected if typing continued before timeout
                }
            });
        }

    }
}