using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using TindaKo.Helpers;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.ViewModel
{
    public partial class ProductSettingsViewModel : ObservableObject
    {
        private const string TemplateAssetPath = "Resources/Templates/ProductTemplate.xlsx";
        private const string TemplateFileName = "ProductTemplate.xlsx";
        private const int MaxErrorsShown = 5;
        private const int MaxCategoryLength = 30;   // same limit as the Add entry in the page

        private sealed record ParsedRow(int RowNumber, Product Product);
        private sealed record ParseResult(List<ParsedRow> Rows, List<string> Errors);

        [ObservableProperty] private bool isBusy;

        // ------------------------------------------------------------------
        // CATEGORIES
        // ------------------------------------------------------------------
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCategoriesEmpty))]
        private ObservableCollection<CategoryInfo> categoryItems = new();

        [ObservableProperty] private string newCategoryName = string.Empty;

        public bool IsCategoriesEmpty => CategoryItems.Count == 0;

        public async Task LoadCategoriesAsync()
        {
            var list = await CategoryService.GetCategoriesWithCountAsync();
            CategoryItems = new ObservableCollection<CategoryInfo>(list);
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var error = await CategoryService.AddCategoryAsync(NewCategoryName);
                if (error != null)
                {
                    await ToastHelper.ShowError(error);
                    return;                              // keep what the user typed
                }

                NewCategoryName = string.Empty;
                await LoadCategoriesAsync();
                await ToastHelper.ShowMessage("Category added");
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
        private async Task EditCategoryAsync(CategoryInfo? item)
        {
            if (item == null || IsBusy) return;

            var input = await App.Navigator.DisplayPromptAsync(
                "I-edit ang category",
                item.ProductCount > 0
                    ? $"Mababago rin ang {item.ProductCountText} na gumagamit nito."
                    : "Bagong pangalan:",
                accept: "I-save", cancel: "Kanselahin",
                initialValue: item.CategoryName, maxLength: MaxCategoryLength);

            if (input == null) return;                   // cancelled

            IsBusy = true;
            try
            {
                var error = await CategoryService.EditCategoryAsync(item.CategoryId, input);
                if (error != null)
                {
                    await ToastHelper.ShowError(error);
                    return;
                }

                await LoadCategoriesAsync();
                await ToastHelper.ShowMessage("Category updated");
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

        // ------------------------------------------------------------------
        // DOWNLOAD TEMPLATE
        // ------------------------------------------------------------------
        [RelayCommand]
        private async Task DownloadTemplateAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                using var templateStream = await FileSystem.OpenAppPackageFileAsync(TemplateAssetPath);

                // FileSaver opens the system "save as" dialog on every platform, so the user can
                // actually find the file. (AppDataDirectory on Android is private and unreachable.)
                var result = await FileSaver.Default.SaveAsync(TemplateFileName, templateStream, CancellationToken.None);

                if (result.IsSuccessful)
                    await ToastHelper.ShowMessage($"Saved to: {result.FilePath}", "Download Complete");
                else
                    Debug.WriteLine($"Template save not completed: {result.Exception?.Message}"); // usually: user cancelled
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

        // ------------------------------------------------------------------
        // IMPORT TEMPLATE
        // ------------------------------------------------------------------
        [RelayCommand]
        private async Task ImportTemplateAsync()
        {
            if (IsBusy) return;
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

                var picked = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Excel Template",
                    FileTypes = customFileTypes
                });

                if (picked is null) return; // user cancelled: not an error

                if (!Path.GetExtension(picked.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    await ToastHelper.ShowError("Please select a .xlsx file.");
                    return;
                }

                // Parse off the UI thread (big sheets freeze the app otherwise).
                ParseResult parsed;
                using (var stream = await picked.OpenReadAsync())
                {
                    parsed = await Task.Run(() => ParseWorkbook(stream));
                }

                var errors = parsed.Errors;

                // Skip products that already exist (re-importing the same file must not double your stock).
                var existingNames = await GetExistingNamesAsync();

                // Existing categories, matched case-insensitively so "snacks" reuses "Snacks".
                var knownCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in await CategoryService.GetCategoriesAsync())
                    knownCategories[c] = c;

                var newCategories = new List<string>();   // categories this import will create
                var toInsert = new List<ParsedRow>();

                foreach (var row in parsed.Rows)
                {
                    if (existingNames.Contains(row.Product.ProdName.Trim()))
                    {
                        errors.Add($"Row {row.RowNumber}: '{row.Product.ProdName}' already exists");
                        continue;
                    }

                    row.Product.Category = ResolveCategory(row.Product.Category, knownCategories, newCategories);
                    toInsert.Add(row);
                }

                // create the new categories first, so no product points to a category that doesn't exist
                foreach (var name in newCategories)
                    await CategoryService.AddCategoryAsync(name);

                int added = 0;
                foreach (var row in toInsert)
                {
                    try
                    {
                        await ProductService.AddProductAsync(row.Product);
                        added++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {row.RowNumber}: {ex.Message}");
                    }
                }

                if (added == 0 && errors.Count == 0)
                {
                    await ToastHelper.ShowError("No products found in the file.");
                    return;
                }

                var summary = $"Added: {added}\nSkipped: {errors.Count}";
                if (newCategories.Count > 0)
                    summary += $"\nNew categories: {string.Join(", ", newCategories)}";

                if (errors.Count > 0)
                {
                    summary += "\n\n" + string.Join("\n", errors.Take(MaxErrorsShown));
                    if (errors.Count > MaxErrorsShown)
                        summary += $"\n...and {errors.Count - MaxErrorsShown} more";
                }

                await LoadCategoriesAsync();   // refresh the list on this page
                await ToastHelper.ShowMessage(summary, "Import Complete");
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

        // ------------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------------
        private static ParseResult ParseWorkbook(Stream stream)
        {
            var rows = new List<ParsedRow>();
            var errors = new List<string>();
            var namesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);

            foreach (var row in sheet.RowsUsed().Skip(1)) // skip header
            {
                int n = row.RowNumber();

                string name = row.Cell(1).GetString().Trim();
                string category = row.Cell(2).GetString().Trim();

                if (name.Length == 0) { errors.Add($"Row {n}: product name is empty"); continue; }
                if (!namesInFile.Add(name)) { errors.Add($"Row {n}: '{name}' is listed twice in the file"); continue; }

                if (category.Length > MaxCategoryLength)
                { errors.Add($"Row {n}: '{name}' has a category longer than {MaxCategoryLength} characters"); continue; }

                if (!TryReadDecimal(row.Cell(3), out var price) || price <= 0)
                { errors.Add($"Row {n}: '{name}' has an invalid price"); continue; }

                if (!TryReadDecimal(row.Cell(4), out var stocksIn) || stocksIn < 0)
                { errors.Add($"Row {n}: '{name}' has invalid stocks in"); continue; }

                if (!TryReadDecimal(row.Cell(5), out var stocksOut) || stocksOut < 0)
                { errors.Add($"Row {n}: '{name}' has invalid stocks out"); continue; }

                if (stocksOut > stocksIn)
                { errors.Add($"Row {n}: '{name}' stocks out is more than stocks in"); continue; }

                rows.Add(new ParsedRow(n, new Product
                {
                    ProdName = name,
                    Category = category,          // raw text; resolved against the Category table after parsing
                    Price = price,
                    StocksIn = stocksIn,
                    StocksOut = stocksOut
                }));
            }

            return new ParseResult(rows, errors);
        }

        // Maps the sheet's category text to an existing category (stored spelling),
        // or registers it as new. Blank becomes "Uncategorized".
        private static string ResolveCategory(
            string? raw,
            Dictionary<string, string> known,
            List<string> created)
        {
            var name = CategoryNormalizer.Normalize(raw);

            if (known.TryGetValue(name, out var existing))
                return existing;

            name = CategoryNormalizer.ToDisplayCase(name);
            known[name] = name;      // later rows with the same text reuse this spelling
            created.Add(name);
            return name;
        }

        // Reads real numeric cells first; falls back to text like "₱1,200.50".
        // Blank cell = 0 (the caller decides whether 0 is acceptable).
        private static bool TryReadDecimal(IXLCell cell, out decimal value)
        {
            value = 0;
            if (cell.IsEmpty()) return true;

            if (cell.TryGetValue(out decimal number))
            {
                value = number;
                return true;
            }

            var text = cell.GetString().Replace("₱", "").Replace(",", "").Trim();
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        // ASSUMPTION: ProductService has a method returning all products.
        // Rename GetProductsAsync to whatever yours is called.
        private static async Task<HashSet<string>> GetExistingNamesAsync()
        {
            var existing = await ProductService.GetProductsAsync();
            return new HashSet<string>(
                existing.Select(p => p.ProdName?.Trim() ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}