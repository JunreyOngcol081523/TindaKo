using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Helpers;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class IncomingStocksService
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
        }

        public static async Task<bool> SaveIncomingStocksAsync(IncomingStocks incomingStocks,
            ObservableCollection<ProductDisplay> cartList)
        {
            try
            {
                if (_db == null)
                    await Init();

                // Insert the parent record (IncomingStocks)
                await _db.InsertAsync(incomingStocks);

                // Map ProductDisplay -> IncomingStocksProductsList
                var itemsToInsert = cartList.Select(p => new IncomingStocksProductsList
                {
                    IncomingStocksId = incomingStocks.IncomingStocksId,
                    ProdId = p.ProdId,
                    ProdName = p.ProdName,
                    SupplierPrice = p.SupplierPrice,
                    StocksInQty = p.NewStocksQty,
                    SubtotalAmount = p.Subtotal
                });

                // Insert all mapped items
                await _db.InsertAllAsync(itemsToInsert);

                return true;
            }
            catch (Exception ex)
            {
                // You can log the error or show a toast/alert if needed
                Debug.WriteLine($"Error saving incoming stocks: {ex.Message}");
                await ToastHelper.ShowError($"Error saving incoming stocks: {ex.Message}");
                return false;
            }
        }
        public static async Task<decimal> GetTotalAmountAsync(string filterBy, DateTime dateFrom, DateTime dateTo)
        {
            await Init();
            DateTime startDate, endDate;

            switch (filterBy.ToLower())
            {
                case "monthly":
                    startDate = new DateTime(dateFrom.Year, dateFrom.Month, 1); // 1st day of month
                    endDate = startDate.AddMonths(1).AddDays(-1);              // last day of month
                    break;

                case "yearly":
                    startDate = new DateTime(dateFrom.Year, 1, 1);             // Jan 1
                    endDate = new DateTime(dateFrom.Year, 12, 31);             // Dec 31
                    break;

                case "custom":
                    startDate = dateFrom.Date;
                    endDate = dateTo.Date.AddDays(1).AddTicks(-1);
                    break;


                default:
                    throw new ArgumentException("Invalid filterBy value. Use 'monthly', 'yearly', or 'custom'.");
            }

            string sql = @"
        SELECT IFNULL(SUM(TotalAmout), 0) 
        FROM IncomingStocks 
        WHERE DateStocksIn BETWEEN ? AND ?";

            var result = await _db.ExecuteScalarAsync<decimal>(sql, startDate, endDate);
            return result;
        }

        //get total incoming stocks for a monthly
        public static async Task<decimal> GetTotalIncomingStocksAsync()
        {
            await Init();

            var now = DateTime.Now;

            var incomingStocks = await _db.Table<IncomingStocks>().ToListAsync();

            var monthlyStocks = incomingStocks
                .Where(t => t.DateStocksIn.Month == now.Month && t.DateStocksIn.Year == now.Year)
                .ToList();

            var total = monthlyStocks.Sum(t => t.TotalAmout); // use TotalAmout (matches model)
            return total;
        }

    }
    public static class IncomingStocksLogsService
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
        }
        public static async Task<List<IncomingStocks>> GetIncomingStocksLogsAsync()
        {
            await Init();
            return await _db.Table<IncomingStocks>().ToListAsync();
        }
    }
}
