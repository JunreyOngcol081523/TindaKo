using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class TransactionServices
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
        }

        public static async Task SaveTransactionWithSalesAsync(Transaction transaction, List<Sell> soldItems)
        {
            try
            {
                await Init();
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

        public static async Task<List<Transaction>> GetFilteredTransactionsAsync(
            bool isFilter,
            DateTime fromDate,
            DateTime toDate,
            string transactionType)
        {
            await Init();
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
        public static async Task<List<Sell>> GetSellsAsync(int transactionId)
        {
            await Init();
            var allSells = await _db.Table<Sell>().ToListAsync();
            return allSells
                .Where(s => s.TransactionId == transactionId).ToList();
        }
        public static async Task<decimal> GetTodayTotalSalesAsync()
        {
            await Init();

            var today = DateTime.Today;
            var format = "MMM dd, yyyy h:mm tt";
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            var transactions = await _db.Table<Transaction>().ToListAsync();

            var todayTransactions = transactions
                .Where(t => DateTime.TryParseExact(t.DateTimeTransacted, format, culture, System.Globalization.DateTimeStyles.None, out var date)
                            && date.Date == today)
                .ToList();

            var total = todayTransactions.Sum(t => t.TotalAmount);
            return total;
        }
        public static async Task<decimal> GetMonthlyTotalSalesAsync()
        {
            await Init();

            var now = DateTime.Now;
            var format = "MMM dd, yyyy h:mm tt";
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            var transactions = await _db.Table<Transaction>().ToListAsync();

            var monthlyTransactions = transactions
                .Where(t =>
                    DateTime.TryParseExact(t.DateTimeTransacted, format, culture, System.Globalization.DateTimeStyles.None, out var date)
                    && date.Month == now.Month
                    && date.Year == now.Year)
                .ToList();

            var total = monthlyTransactions.Sum(t => t.TotalAmount);
            return total;
        }


    }
}
