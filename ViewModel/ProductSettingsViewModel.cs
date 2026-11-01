using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliJ.Lang.Annotations;
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
        private const string DefaultCategory = "Uncategorized"; // used when the Category cell is blank
        private const int MaxErrorsShown = 5;

        private sealed record ParsedRow(int RowNumber, Product Product);
        private sealed record ParseResult(List<ParsedRow> Rows, List<string> Errors);

        [ObservableProperty] private bool isBusy;

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
                var toInsert = new List<ParsedRow>();
                foreach (var row in parsed.Rows)
                {
                    if (existingNames.Contains(row.Product.ProdName.Trim()))
                        errors.Add($"Row {row.RowNumber}: '{row.Product.ProdName}' already exists");
                    else
                        toInsert.Add(row);
                }

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
                if (errors.Count > 0)
                {
                    summary += "\n\n" + string.Join("\n", errors.Take(MaxErrorsShown));
                    if (errors.Count > MaxErrorsShown)
                        summary += $"\n...and {errors.Count - MaxErrorsShown} more";
                }

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
                    Category = category.Length == 0 ? DefaultCategory : category,
                    Price = price,
                    StocksIn = stocksIn,
                    StocksOut = stocksOut
                }));
            }

            return new ParseResult(rows, errors);
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