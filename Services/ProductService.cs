using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.Services
{
    public static class ProductService
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
        }
        public static async Task<Product> GetProductByIdAsync(int prodId)
        {
            if (_db == null)
                await Init();
            return await _db.Table<Product>().Where(p => p.ProdId == prodId).FirstOrDefaultAsync();
        }

        public static async Task UpdateProductAsync(Product product)
        {
            if (_db == null)
                await Init();
            await _db.UpdateAsync(product);
        }
        // CRUD Example
        public static async Task<List<Product>> GetProductsAsync(int maxResults)
        {
            await Init();
            return await _db.Table<Product>().Take(maxResults).ToListAsync();
        }
        public static async Task<List<Product>> SearchProductsAsync(string keyword, int maxResults)
        {
            await Init();
            return await _db.Table<Product>()
                      .Where(p =>
                          (!string.IsNullOrEmpty(p.ProdName) && p.ProdName.ToLower().Contains(keyword.ToLower())) ||
                          (!string.IsNullOrEmpty(p.Category) && p.Category.ToLower().Contains(keyword.ToLower())))
                      .Take(maxResults)
                      .ToListAsync();
        }

        public static async Task<int> AddProductAsync(Product product) {
            await Init();
            return await _db.InsertAsync(product);
        }



        public static async Task<int> DeleteProductAsync(Product product) {
            await Init();
            return await _db.DeleteAsync(product);
        }
        public static async Task<int> CountLowStockProductsAsync()
        {
            await Init(); // Ensure database is initialized

            var products = await _db.Table<Product>().ToListAsync();

            var lowStockCount = products.Count(p => (p.StocksIn - p.StocksOut) <= 5);

            return lowStockCount;
        }
        public static async Task<List<Product>> GetLowStockProductsAsync(int threshold = 5)
        {
            await Init(); // Ensure database is initialized

            // Get all products
            var products = await _db.Table<Product>().ToListAsync();

            // Filter by remaining stock
            var lowStockProducts = products
                .Where(p => (p.StocksIn - p.StocksOut) <= threshold)
                .OrderBy(p => p.CurrentStock) // Optional: order by lowest first
                .ToList();

            return lowStockProducts;
        }

        public static async Task<List<FastSellingProductDto>> GetFastMovingProductsAsync()
        {
            await Init();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var sells = await _db.Table<Sell>().ToListAsync();
            var transactions = await _db.Table<Transaction>().ToListAsync();

            var query = from s in sells
                        join t in transactions on s.TransactionId equals t.TransactionId
                        let date = DateTime.Parse(t.DateTimeTransacted)
                        where date.Month == currentMonth && date.Year == currentYear
                        group s by s.ProductName into g
                        orderby g.Sum(x => x.Qty) descending
                        select new FastSellingProductDto
                        {
                            ProductName = g.Key,
                            TotalQty = g.Sum(x => x.Qty)
                        };

            var allProducts = query.ToList();

            int halfCount = allProducts.Count / 2;

            return allProducts.Take(halfCount).ToList();
        }

    }
}
