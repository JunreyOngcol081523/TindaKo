using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using TindaKo.Models;
namespace TindaKo.Services
{


    public class DatabaseService
    {
        public static SQLiteAsyncConnection _db;

        public static async Task InitAsync()
        {
            if (_db != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tindako.db");
            _db = new SQLiteAsyncConnection(dbPath);

            // Create tables
            await _db.CreateTableAsync<Product>();
            await _db.CreateTableAsync<Users>();

            //-------Users------------//
            // Check if Users table is empty
            var userCount = await _db.Table<Users>().CountAsync();
            if (userCount == 0)
            {
                var defaultUser = new Users
                {
                    UserName = "admin",
                    UserPassword = "admin123", // You can hash this later
                    UserRole = "May-ari"
                };

                await _db.InsertAsync(defaultUser);
            }
        }

        // CRUD Example
        public static Task<List<Product>> GetProductsAsync() =>
            _db.Table<Product>().ToListAsync();

        public static Task<int> AddProductAsync(Product product) =>
            _db.InsertAsync(product);

        public static Task<int> UpdateProductAsync(Product product) =>
            _db.UpdateAsync(product);

        public static Task<int> DeleteProductAsync(Product product) =>
            _db.DeleteAsync(product);
        public static Task<Users> GetUserAsync(string username, string password)
        {
            return _db.Table<Users>()
                      .Where(u => u.UserName == username && u.UserPassword == password)
                      .FirstOrDefaultAsync();
        }

    }

}
