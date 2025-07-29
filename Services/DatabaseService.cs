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
            //dropping for testing only
            //await _db.DropTableAsync<Product>();
            //await _db.DropTableAsync<Transaction>();
            //await _db.DropTableAsync<Users>();
            //await _db.DropTableAsync<Sell>();
            //await _db.DropTableAsync<Buyer>();
            //await _db.DropTableAsync<Creditor>();

            // Create tables
            await _db.CreateTableAsync<Product>();
            await _db.CreateTableAsync<Users>();
            await _db.CreateTableAsync<Sell>();
            await _db.CreateTableAsync<Transaction>();
            await _db.CreateTableAsync<Buyer>();
            await _db.CreateTableAsync<Creditor>();

            //-------Users------------//
            // Check if Users table is empty
            var userCount = await _db.Table<Users>().CountAsync();
            if (userCount == 0)
            {
                var defaultUser = new Users
                {
                    UserName = "admin",
                    UserFullName= "Administrator",
                    UserPassword = "admin123", // You can hash this later
                    UserRole = "May-ari"
                };

                await _db.InsertAsync(defaultUser);
            }
        }

        // CRUD Example
        public static Task<List<Product>> GetProductsAsync(int maxResults)
        {
            _ = DatabaseService.InitAsync();
            return _db.Table<Product>().Take(maxResults).ToListAsync();
        }
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


        //--------------buyer--------------------//
        public static Task<int> AddBuyerAsync(Buyer buyer) =>
        _db.InsertAsync(buyer);
        public static Task<List<Buyer>> GetBuyersAsync() =>
        _db.Table<Buyer>().ToListAsync();
        public static Task<Buyer> GetBuyerAsync(int? buyerId)
        {
            return _db.Table<Buyer>().Where(b => b.BuyerId == buyerId).FirstOrDefaultAsync();
        }

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
        public static async Task SaveCreditorAsync(Creditor creditor)
        {
            await _db.InsertAsync(creditor);
        }
        //------------load transaction----------------//
        public static async Task<List<Transaction>> GetFilteredTransactionsAsync(
            bool isFilter,
            DateTime fromDate,
            DateTime toDate,
            string transactionType)
        {
            var allTransactions = await _db.Table<Transaction>().ToListAsync();

            return allTransactions
                .Where(t =>
                {
                    if (isFilter)
                    {
                        bool parsed = DateTime.TryParseExact(
                            t.DateTimeTransacted,
                            "MMM dd, yyyy h:mm tt",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime transDate);

                        if (!parsed)
                            return false;

                        bool isInDateRange = transDate.Date >= fromDate.Date && transDate.Date <= toDate.Date;
                        bool matchesType = string.IsNullOrEmpty(transactionType) || t.TransactionType == transactionType;
                        return isInDateRange && matchesType;
                    }

                    return true;
                })
                .OrderByDescending(t => t.TransactionId)
                .ToList();
        }


        //---------------------Sell-----------------------//
        public static async Task<List<Sell>> GetSellsAsync(int transactionId)
        {
            var allSells = await _db.Table<Sell>().ToListAsync();
            return allSells
                .Where(s=>s.TransactionId == transactionId).ToList();
        }
        //-------------------Users-----------------------//
        public static Task<Users> GetUserAsync(string username, string password)
        {
            return _db.Table<Users>()
                      .Where(u => u.UserName == username && u.UserPassword == password)
                      .FirstOrDefaultAsync();
        }
        public static Task<List<Users>> GetUsersAsync() =>
            _db.Table<Users>().ToListAsync();
        public static async Task<int> UpdateUserAsync(Users user) {

            return  await _db.UpdateAsync(user);
        }
        public static Task<int> AddNewUserAsync(Users user) {
            return _db.InsertAsync(user);
        }
         //--------------------creditor--------------------------//
         public static async Task<List<Creditor>> GetCreditorsAsync()
        {
            DatabaseService.InitAsync();
            return await _db.Table<Creditor>().ToListAsync();
        }
        public static async Task<int> UpdateCreditorAsync(Creditor creditor) =>
           await _db.UpdateAsync(creditor);
        
        //---------------------incoming stocks-----------------------//
    }
}
