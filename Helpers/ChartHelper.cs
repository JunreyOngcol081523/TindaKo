using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Services;

namespace TindaKo.Helpers
{
    public static class ChartHelper
    {


        public static async Task<List<ChartEntry>> MonthlySalesEntry(int? year = null)
        {
            try
            {
                var allTransactions = await TransactionServices.GetAllTransactionsAsync(year);
                if (allTransactions == null || !allTransactions.Any())
                    return new List<ChartEntry>();
                List<ChartEntry> entries = allTransactions
                    .GroupBy(t =>
                    {
                        var dt = DateTime.Parse(t.DateTimeTransacted);
                        return dt.ToString("MMM yyyy", CultureInfo.InvariantCulture); // e.g., "Aug 2025"
                    })
                    .OrderBy(g => DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.InvariantCulture))
                    .Select(g => new ChartEntry((float)g.Sum(t => t.TotalAmount))
                    {
                        Label = g.Key,
                        ValueLabel = "₱" + g.Sum(t => t.TotalAmount).ToString("N0"),
                        Color = SKColor.Parse("#009688")
                    })
                    .ToList();

                return entries;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new List<ChartEntry>(); // Return an empty chart list (chart will just be blank)
            }
        }
        public static async Task<List<ChartEntry>> GetExpensesSalesChartDataAsync(string filterBy, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var expenses = await IncomingStocksService.GetTotalAmountAsync(filterBy, dateFrom, dateTo);
                var sales = await TransactionServices.GetTotalAmountAsync(filterBy, dateFrom, dateTo);
                 
                var entries = new List<ChartEntry>
                {
                    new ChartEntry((float)expenses)
                    {
                        Label = "Expenses",
                        ValueLabel = "₱" + expenses.ToString("N0"),
                        Color = SKColor.Parse("#C0392B") // Red for expenses
                    },
                    new ChartEntry((float)sales)
                    {
                        Label = "Sales",
                        ValueLabel = "₱" + sales.ToString("N0"),
                        Color = SKColor.Parse("#009688") // Green for sales
                    }
                };
                return entries;
            }
            catch (Exception ex)
            {
                return new List<ChartEntry>(); // Return an empty chart list (chart will just be blank)
            }
        }
    }
}
