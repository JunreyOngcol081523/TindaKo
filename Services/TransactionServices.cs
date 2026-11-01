using SQLite;
using System.Diagnostics;
using System.Globalization;
using TindaKo.Models;

namespace TindaKo.Services
{
    /// Thrown when a sale asks for more units than are in stock.
    /// The message is user-facing, so the ViewModel can show it as-is.
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message) : base(message) { }
    }

    public static class TransactionServices
    {
        // Single place that knows how DateTimeTransacted is stored.
        private const string DateFormat = "MMM dd, yyyy h:mm tt";

        private static bool TryParseDate(string? text, out DateTime date) =>
            DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

        // Kept for existing callers; the connection itself is owned by DatabaseService.
        public static Task Init() => DatabaseService.GetConnectionAsync();

        // ───────────────────────── Save ─────────────────────────

        /// Saves the transaction header, its sell rows, the stock deduction and (for utang)
        /// the creditor record as ONE database transaction: everything is saved or nothing is.
        /// THROWS on failure (InsufficientStockException for stock problems) so the caller
        /// can tell the user the truth and keep the cart.
        public static async Task SaveTransactionWithSalesAsync(
            Transaction transaction,
            List<Sell> soldItems,
            Creditor? creditor = null)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            if (soldItems is null || soldItems.Count == 0)
                throw new ArgumentException("Walang laman ang benta.", nameof(soldItems));

            var db = await DatabaseService.GetConnectionAsync();

            // Any exception thrown inside rolls EVERYTHING back and is re-thrown to the caller.
            await db.RunInTransactionAsync(conn =>
            {
                // 1. Check the stock against the database (not the screen) and deduct it.
                foreach (var item in soldItems)
                {
                    if (item.ProductID == 0)
                        throw new InvalidOperationException($"Hindi valid ang produkto: {item.ProductName}.");

                    var product = conn.Table<Product>().FirstOrDefault(p => p.ProdId == item.ProductID)
                        ?? throw new InvalidOperationException($"Hindi nahanap ang produkto: {item.ProductName}.");

                    var remaining = product.StocksIn - product.StocksOut;
                    if (item.Qty > remaining)
                        throw new InsufficientStockException(
                            $"{product.ProdName}: {remaining} na lang ang stock, pero {item.Qty} ang hinihingi.");

                    item.UnitCost = product.AverageCost; // cost snapshot for profit reports (0 = unknown)

                    product.StocksOut += item.Qty;
                    conn.Update(product);
                }

                // 2. Header first so we get its generated id.
                conn.Insert(transaction);

                // 3. Sell rows.
                foreach (var item in soldItems)
                {
                    item.TransactionId = transaction.TransactionId;
                    conn.Insert(item);
                }

                // 4. Utang record, in the same transaction.
                if (creditor != null)
                    conn.Insert(creditor);
            });
        }

        // ───────────────────────── Queries ─────────────────────────

        public static async Task<List<Transaction>> GetFilteredTransactionsAsync(
            bool isFilter,
            DateTime fromDate,
            DateTime toDate,
            string transactionType)
        {
            var db = await DatabaseService.GetConnectionAsync();
            var all = await db.Table<Transaction>().ToListAsync();

            return all
                .Where(t =>
                {
                    if (!isFilter) return true;

                    if (!TryParseDate(t.DateTimeTransacted, out var transDate))
                        return false;

                    bool inRange = transDate.Date >= fromDate.Date && transDate.Date <= toDate.Date;
                    bool matchesType = string.IsNullOrEmpty(transactionType) || t.TransactionType == transactionType;
                    return inRange && matchesType;
                })
                .OrderByDescending(t => t.TransactionId)
                .ToList();
        }

        public static async Task<List<Sell>> GetSellsAsync(int transactionId)
        {
            var db = await DatabaseService.GetConnectionAsync();
            // filtered by SQLite instead of loading every sell row into memory
            return await db.Table<Sell>().Where(s => s.TransactionId == transactionId).ToListAsync();
        }

        public static async Task<decimal> GetTodayTotalSalesAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var today = DateTime.Today;

            var transactions = await db.Table<Transaction>().ToListAsync();

            return transactions
                .Where(t => TryParseDate(t.DateTimeTransacted, out var date) && date.Date == today)
                .Sum(t => t.TotalAmount);
        }

        public static async Task<decimal> GetMonthlyTotalSalesAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var now = DateTime.Now;

            var transactions = await db.Table<Transaction>().ToListAsync();

            return transactions
                .Where(t => TryParseDate(t.DateTimeTransacted, out var date)
                            && date.Month == now.Month
                            && date.Year == now.Year)
                .Sum(t => t.TotalAmount);
        }

        /// All transactions of one year (defaults to the current year). Returns an empty list on error.
        public static async Task<List<Transaction>> GetAllTransactionsAsync(int? year = null)
        {
            try
            {
                var db = await DatabaseService.GetConnectionAsync();
                int targetYear = year ?? DateTime.Now.Year;

                var all = await db.Table<Transaction>().ToListAsync();

                // TryParseExact + InvariantCulture: the old DateTime.TryParse used the phone's
                // culture and could fail on a non-English device.
                return all
                    .Where(t => TryParseDate(t.DateTimeTransacted, out var date) && date.Year == targetYear)
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new List<Transaction>();
            }
        }

        public static async Task<decimal> GetTotalAmountAsync(string filterBy, DateTime dateFrom, DateTime dateTo)
        {
            var db = await DatabaseService.GetConnectionAsync();

            // Half-open range [start, endExclusive): the old code used "<= last day at 00:00",
            // which dropped every sale made on the last day of a month or year.
            DateTime start, endExclusive;

            switch (filterBy.ToLowerInvariant())
            {
                case "monthly":
                    start = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                    endExclusive = start.AddMonths(1);
                    break;

                case "yearly":
                    start = new DateTime(dateFrom.Year, 1, 1);
                    endExclusive = start.AddYears(1);
                    break;

                case "custom":
                    start = dateFrom.Date;
                    endExclusive = dateTo.Date.AddDays(1);
                    break;

                default:
                    throw new ArgumentException("Invalid filterBy value. Use 'monthly', 'yearly', or 'custom'.");
            }

            var transactions = await db.Table<Transaction>().ToListAsync();

            var matchingTransactions = transactions
                .Where(t => TryParseDate(t.DateTimeTransacted, out var date) && date >= start && date < endExclusive)
                .ToList();

            Debug.WriteLine($"[Transactions] FilterBy={filterBy}, input DateFrom={dateFrom:yyyy-MM-dd HH:mm:ss}, input DateTo={dateTo:yyyy-MM-dd HH:mm:ss}, range=[{start:yyyy-MM-dd HH:mm:ss}, {endExclusive:yyyy-MM-dd HH:mm:ss})");
            Debug.WriteLine($"[Transactions] Matching dates: {string.Join(", ", matchingTransactions.Select(t => t.DateTimeTransacted))}");

            return matchingTransactions.Sum(t => t.TotalAmount);
        }
    }
}