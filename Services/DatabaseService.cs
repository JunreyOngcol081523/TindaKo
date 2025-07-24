using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using TindaKo.Models;
using System.Diagnostics;
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
            await _db.CreateTableAsync<Sell>();
            await _db.CreateTableAsync<Transaction>();
            await _db.CreateTableAsync<Buyer>();
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
        public static Task<List<Product>> GetProductsAsync(int maxResults) =>
            _db.Table<Product>().Take(maxResults).ToListAsync();
        public static Task<List<Product>> SearchProductsAsync(string keyword, int maxResults)
        {
            return _db.Table<Product>()
                      .Where(p =>
                          (!string.IsNullOrEmpty(p.ProdName) && p.ProdName.ToLower().Contains(keyword.ToLower())) ||
                          (!string.IsNullOrEmpty(p.Category) && p.Category.ToLower().Contains(keyword.ToLower())))
                      .Take(maxResults)
                      .ToListAsync();
        }

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

        //--------------buyer--------------------//
        public static Task<int> AddBuyerAsync(Buyer buyer) =>
        _db.InsertAsync(buyer);
        public static Task<List<Buyer>> GetBuyersAsync() =>
        _db.Table<Buyer>().ToListAsync();

        //------------------transaction----------------//
        public static async Task SaveTransactionWithSalesAsync(Transaction transaction, List<Sell> soldItems)
        {
            try
            {
                // Insert Transaction first to get the ID
                await _db.InsertAsync(transaction);

                foreach (var sell in soldItems)
                {
                    sell.TransactionId = transaction.TransactionId; // Set FK
                    await _db.InsertAsync(sell);

                    // Update StocksOut
                    if (sell.ProductID == 0)
                    {
                        Debug.WriteLine("Warning: sell.ProductID is 0. Cannot fetch product.");
                        continue;
                    }
                    var product = await _db.Table<Product>().FirstOrDefaultAsync(p => p.ProdId == sell.ProductID);
                    Debug.WriteLine($"Product prodID: {product.ProdId}\n sell prodID: {sell.ProductID}");
                    if (product != null)
                    {
                        product.StocksOut += sell.Qty;
                        await _db.UpdateAsync(product);
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No product found with ProdId = {sell.ProductID}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }



        //---------------sell----------------------------//
        public static Task<int> AddSellAsync(Sell sell) => _db.InsertAsync(sell);
    }

}
