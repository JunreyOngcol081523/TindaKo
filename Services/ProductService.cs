using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;
using TindaKo.Services;

namespace TindaKo.Converters
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
    }
}
