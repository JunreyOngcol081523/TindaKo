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

    }
}
