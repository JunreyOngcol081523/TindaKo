using TindaKo.Helpers;
using TindaKo.Models;

namespace TindaKo.Services
{
    internal class CategoryService
    {
        public static Task Init() => DatabaseService.GetConnectionAsync();

        // names only, sorted (for the Picker on the product form)
        public static async Task<List<string>> GetCategoriesAsync()
        {
            var conn = await DatabaseService.GetConnectionAsync();
            var categories = await conn.Table<Category>().ToListAsync();
            return categories
                .Select(c => c.CategoryName)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // names + product count (for the Category list in Product Settings)
        public static async Task<List<CategoryInfo>> GetCategoriesWithCountAsync()
        {
            var conn = await DatabaseService.GetConnectionAsync();
            return await conn.QueryAsync<CategoryInfo>(
                @"SELECT c.CategoryId, c.CategoryName, COUNT(p.Category) AS ProductCount
                  FROM Category c
                  LEFT JOIN Product p ON p.Category = c.CategoryName COLLATE NOCASE
                  GROUP BY c.CategoryId, c.CategoryName
                  ORDER BY c.CategoryName COLLATE NOCASE");
        }

        // returns null on success, otherwise the message to show the user
        public static async Task<string?> AddCategoryAsync(string? categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return "Maglagay ng pangalan ng category.";

            var conn = await DatabaseService.GetConnectionAsync();
            var name = CategoryNormalizer.ToDisplayCase(CategoryNormalizer.Normalize(categoryName));

            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Category WHERE CategoryName = ? COLLATE NOCASE", name);
            if (exists > 0)
                return $"Mayroon nang \"{name}\".";

            await conn.InsertAsync(new Category { CategoryName = name });
            return null;
        }

        // renames the category AND all products using it, in one transaction
        public static async Task<string?> EditCategoryAsync(int categoryId, string? newCategoryName)
        {
            if (string.IsNullOrWhiteSpace(newCategoryName))
                return "Maglagay ng pangalan ng category.";

            var conn = await DatabaseService.GetConnectionAsync();
            var newName = CategoryNormalizer.ToDisplayCase(CategoryNormalizer.Normalize(newCategoryName));
            string? error = null;

            await conn.RunInTransactionAsync(c =>
            {
                var oldName = c.ExecuteScalar<string>(
                    "SELECT CategoryName FROM Category WHERE CategoryId = ?", categoryId);

                if (oldName == null) { error = "Hindi nahanap ang category."; return; }

                if (string.Equals(oldName, CategoryNormalizer.Uncategorized, StringComparison.OrdinalIgnoreCase))
                { error = "Hindi puwedeng baguhin ang Uncategorized."; return; }

                if (oldName == newName) return;   // nothing changed

                // ignore this same row, so "chips" -> "Chips" is allowed
                var duplicate = c.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Category WHERE CategoryName = ? COLLATE NOCASE AND CategoryId <> ?",
                    newName, categoryId);
                if (duplicate > 0) { error = $"Mayroon nang \"{newName}\"."; return; }

                c.Execute("UPDATE Category SET CategoryName = ? WHERE CategoryId = ?", newName, categoryId);
                c.Execute("UPDATE Product SET Category = ? WHERE Category = ? COLLATE NOCASE", newName, oldName);
            });

            return error;
        }
    }
}