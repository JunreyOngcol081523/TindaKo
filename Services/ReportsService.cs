using SQLite;
using System.Globalization;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class ReportsService
    {
        /// Builds every number the Reports page needs for [fromDate, toDate] (inclusive).
        public static async Task<ReportSnapshot> BuildAsync(DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var endExclusive = toDate.Date.AddDays(1);

            var db = await DatabaseService.GetConnectionAsync();
            var snap = new ReportSnapshot();

            // ───────────── Sales in the period ─────────────
            var txInRange = new List<(Transaction Tx, DateTime Date)>();
            foreach (var t in await db.Table<Transaction>().ToListAsync())
            {
                if (TransactionServices.TryParseDate(t.DateTimeTransacted, out var d)
                    && d >= start && d < endExclusive)
                {
                    txInRange.Add((t, d));
                }
            }

            snap.TransactionCount = txInRange.Count;
            snap.SalesTotal = txInRange.Sum(x => x.Tx.TotalAmount);
            snap.CashSales = txInRange.Where(x => x.Tx.TransactionType == "Cash").Sum(x => x.Tx.TotalAmount);
            snap.CreditSales = txInRange.Where(x => x.Tx.TransactionType == "Credit").Sum(x => x.Tx.TotalAmount);
            snap.Trend = BuildTrend(txInRange, start, endExclusive);

            // ───────────── Profit + product ranking ─────────────
            var ids = txInRange.Select(x => x.Tx.TransactionId).ToHashSet();

            var sells = ids.Count == 0
                ? new List<Sell>()
                : (await db.Table<Sell>().ToListAsync()).Where(s => ids.Contains(s.TransactionId)).ToList();

            var knownSells = sells.Where(s => s.UnitCost > 0).ToList();
            snap.KnownRevenue = knownSells.Sum(s => s.Price * s.Qty);
            snap.CostOfGoods = knownSells.Sum(s => s.UnitCost * s.Qty);
            snap.UnknownCostSales = sells.Where(s => s.UnitCost <= 0).Sum(s => s.Price * s.Qty);

            snap.Products = sells
                .GroupBy(s => s.ProductID)
                .Select(g =>
                {
                    var known = g.Where(s => s.UnitCost > 0).ToList();
                    return new ProductRankRow
                    {
                        ProductName = g.Last().ProductName,
                        QtySold = g.Sum(s => s.Qty),
                        Revenue = known.Sum(s => s.Price * s.Qty),
                        Cost = known.Sum(s => s.UnitCost * s.Qty),
                        UnknownCostQty = g.Where(s => s.UnitCost <= 0).Sum(s => s.Qty)
                    };
                })
                .ToList();

            // ───────────── Stock purchases in the period ─────────────
            var purchases = await db.Table<IncomingStocks>()
                .Where(x => x.DateStocksIn >= start && x.DateStocksIn < endExclusive)
                .ToListAsync();
            snap.StockPurchases = purchases.Sum(x => x.TotalAmount);

            // ───────────── Utang (current balances) ─────────────
            var today = DateTime.Today;
            var allCreditors = await db.Table<Creditor>().ToListAsync();
            var activePayments = (await db.Table<CreditPayment>().ToListAsync()).Where(p => !p.IsVoided).ToList();
            var paidById = activePayments
                .GroupBy(p => p.CreditorId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            decimal balanceOf(Creditor c) => Math.Max(c.CreditAmount - paidById.GetValueOrDefault(c.Id), 0m);

            snap.CollectedInPeriod = activePayments
                .Where(p => p.DatePaid >= start && p.DatePaid < endExclusive)
                .Sum(p => p.Amount);

            var unpaid = allCreditors.Where(c => !c.IsPaid).ToList();

            snap.UnpaidTotal = unpaid.Sum(c => balanceOf(c));
            snap.OverdueCount = unpaid.Count(c => c.DueDate < today);
            snap.TopDebtors = unpaid
                .GroupBy(c => string.IsNullOrWhiteSpace(c.CreditorName) ? "(no name)" : c.CreditorName)
                .Select(g =>
                {
                    var overdue = g.Count(c => c.DueDate < today);
                    return new DebtorRow
                    {
                        Name = g.Key,
                        Amount = g.Sum(c => balanceOf(c)),
                        Detail = $"{g.Count()} credit(s)" + (overdue > 0 ? $"  ·  {overdue} overdue" : string.Empty)
                    };
                })
                .OrderByDescending(d => d.Amount)
                .Take(5)
                .ToList();

            // ───────────── Inventory ─────────────
            var products = await db.Table<Product>().ToListAsync();
            snap.InventoryValue = products.Sum(p => Math.Max(p.StocksIn - p.StocksOut, 0m) * p.AverageCost);
            snap.NoCostProductCount = products.Count(p => p.StocksIn - p.StocksOut > 0 && p.AverageCost <= 0);

            return snap;
        }

        // Daily bars for ranges up to 31 days, monthly bars for anything longer.
        // Empty days/months are included as zero so the chart has no gaps.
        private static List<TrendPoint> BuildTrend(
            List<(Transaction Tx, DateTime Date)> tx, DateTime start, DateTime endExclusive)
        {
            var points = new List<TrendPoint>();

            if ((endExclusive - start).Days <= 31)
            {
                var byDay = tx
                    .GroupBy(x => x.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Tx.TotalAmount));

                for (var day = start; day < endExclusive; day = day.AddDays(1))
                {
                    points.Add(new TrendPoint
                    {
                        Label = day.Day.ToString(CultureInfo.InvariantCulture),
                        Amount = byDay.GetValueOrDefault(day)
                    });
                }
            }
            else
            {
                var byMonth = tx
                    .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Tx.TotalAmount));

                var multiYear = start.Year != endExclusive.AddDays(-1).Year;

                for (var month = new DateTime(start.Year, start.Month, 1); month < endExclusive; month = month.AddMonths(1))
                {
                    points.Add(new TrendPoint
                    {
                        Label = month.ToString(multiYear ? "MMM yy" : "MMM", CultureInfo.InvariantCulture),
                        Amount = byMonth.GetValueOrDefault(month)
                    });
                }
            }

            return points;
        }
    }
}
