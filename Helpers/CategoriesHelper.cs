using SQLite;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.Helpers
{
    
    public static class CategoriesHelper
    {
        public static async Task<ObservableCollection<string>> LoadCategoriesAsync()
        {
            return await CategoryService.GetCategoriesAsync().ContinueWith(t => new ObservableCollection<string>(t.Result));
        }


        internal static async Task SeedCategoriesAsync(SQLiteAsyncConnection conn)
        {
            try
            {
                if (await conn.Table<Category>().CountAsync() > 0) return;

                var names = new List<string>();

                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("categories.json");
                    var seed = await System.Text.Json.JsonSerializer.DeserializeAsync<List<string>>(stream);
                    if (seed != null) names.AddRange(seed);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Category seed JSON failed: {ex.Message}");
                }

                // categories already used by existing products
                var fromProducts = await conn.QueryScalarsAsync<string>(
                    "SELECT DISTINCT TRIM(Category) FROM Product " +
                    "WHERE Category IS NOT NULL AND TRIM(Category) <> ''");
                names.AddRange(fromProducts);

                await conn.RunInTransactionAsync(c =>
                {
                    foreach (var raw in names)
                    {
                        var name = CategoryNormalizer.ToDisplayCase(CategoryNormalizer.Normalize(raw));
                        c.Execute("INSERT OR IGNORE INTO Category (CategoryName) VALUES (?)", name);
                    }
                });
            }
            catch (Exception ex)
            {
                // seeding is non-critical: don't stop the app from opening the database
                System.Diagnostics.Debug.WriteLine($"Category seeding failed: {ex}");
            }
        }
    }

    public static class CategoryNormalizer
    {
        public const string Uncategorized = "Uncategorized";

        // trim, collapse repeated spaces, blank -> Uncategorized
        public static string Normalize(string? input)
        {
            var cleaned = Regex.Replace((input ?? string.Empty).Trim(), @"\s+", " ");
            return cleaned.Length == 0 ? Uncategorized : cleaned;
        }

        // only fix all-lower / all-upper input; keep deliberate casing like "RTD Drinks"
        public static string ToDisplayCase(string name) =>
            name == name.ToLowerInvariant() || name == name.ToUpperInvariant()
                ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant())
                : name;
    }
}
