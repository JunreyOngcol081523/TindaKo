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
        //public static async Task<decimal> GetTotalAmountAsync(string filterBy, DateTime dateFrom, DateTime dateTo)
        //{
        //    await Init();
        //    DateTime startDate, endDate;

        //    switch (filterBy.ToLower())
        //    {
        //        case "monthly":
        //            startDate = new DateTime(dateFrom.Year, dateFrom.Month, 1); // 1st day of month
        //            endDate = startDate.AddMonths(1).AddDays(-1);              // last day of month
        //            break;

        //        case "yearly":
        //            startDate = new DateTime(dateFrom.Year, 1, 1);             // Jan 1
        //            endDate = new DateTime(dateFrom.Year, 12, 31);             // Dec 31
        //            break;

        //        case "custom":
        //            startDate = dateFrom.Date;
        //            endDate = dateTo.Date.AddDays(1).AddTicks(-1);
        //            break;


        //        default:
        //            throw new ArgumentException("Invalid filterBy value. Use 'monthly', 'yearly', or 'custom'.");
        //    }

        //    string sql = @"
        //SELECT IFNULL(SUM(TotalAmount), 0) 
        //FROM IncomingStocks 
        //WHERE DateStocksIn BETWEEN ? AND ?";

        //    var result = await _db.ExecuteScalarAsync<decimal>(sql, startDate, endDate);
        //    return result;
        //}
        public static async Task<decimal> GetTotalAmountAsync(string filterBy, DateTime dateFrom, DateTime dateTo)
        {
            await Init();
            DateTime startDate, endExclusive;

            switch (filterBy?.ToLower())
            {
                case "monthly":
                    startDate = new DateTime(dateFrom.Year, dateFrom.Month, 1); // 1st day of month
                    endExclusive = startDate.AddMonths(1);
                    break;

                case "yearly":
                    startDate = new DateTime(dateFrom.Year, 1, 1);             // Jan 1
                    endExclusive = startDate.AddYears(1);
                    break;

                case "custom":
                    startDate = dateFrom.Date;
                    endExclusive = dateTo.Date.AddDays(1);
                    break;

                default:
                    throw new ArgumentException("Invalid filterBy value. Use 'monthly', 'yearly', or 'custom'.");
            }

            var incomingStocks = await _db.Table<IncomingStocks>().ToListAsync();

            var matchingStocks = incomingStocks
                .Where(s => s.DateStocksIn >= startDate && s.DateStocksIn < endExclusive)
                .ToList();

            Debug.WriteLine($"[IncomingStocks] FilterBy={filterBy}, input DateFrom={dateFrom:yyyy-MM-dd HH:mm:ss}, input DateTo={dateTo:yyyy-MM-dd HH:mm:ss}, range=[{startDate:yyyy-MM-dd HH:mm:ss}, {endExclusive:yyyy-MM-dd HH:mm:ss})");
            Debug.WriteLine($"[IncomingStocks] Matching dates: {string.Join(", ", matchingStocks.Select(s => s.DateStocksIn.ToString("yyyy-MM-dd HH:mm:ss")))}");

            var result = matchingStocks.Sum(s => s.TotalAmount);

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

            var total = monthlyStocks.Sum(t => t.TotalAmount); // use TotalAmout (matches model)
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
        //load all incoming stocks logs by month and year
        public static async Task<List<IncomingStocks>> GetIncomingStocksLogsByMonthYearAsync(string month, int year)
        {
            try
            {
                await Init();
                var monthNumber = DateTime.ParseExact(month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                return (await _db.Table<IncomingStocks>().ToListAsync())
                    .Where(t => t.DateStocksIn.Month == monthNumber && t.DateStocksIn.Year == year)
                    .ToList();

            }
            catch (Exception ex)
            {

                await ToastHelper.ShowError(ex.Message);
                Debug.WriteLine($"Error loading incoming stocks logs: {ex.Message}");
                return new List<IncomingStocks>();
            }
            
        }

        //get incoming stocks products list by incoming stocks id
        public static async Task<List<IncomingStocksProductsList>> GetIncomingStocksProductsListAsync(int incomingStocksId)
        {
            await Init();
            return await _db.Table<IncomingStocksProductsList>()
                .Where(p => p.IncomingStocksId == incomingStocksId)
                .ToListAsync();
        }
    }

}
