using TindaKo.Models;

namespace TindaKo.Services
{
    public static class SellServices
    {
        public static async Task<List<OutgoingProductItem>> GetOutgoingProductsAsync(DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1).AddTicks(-1); // include the whole "To" day

            var db = await DatabaseService.GetConnectionAsync();

            // Sell has no date, so find the transactions in range first
            var trans = (await db.Table<Transaction>().ToListAsync())
                .Where(t => DateTime.TryParseExact(
                    t.DateTimeTransacted,
                    "MMM dd, yyyy h:mm tt",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var transactedAt)
                    && transactedAt >= start
                    && transactedAt <= end)
                .ToList();

            var dateById = trans.ToDictionary(t => t.TransactionId, t => t.DateTimeTransacted);
            if (dateById.Count == 0) return new List<OutgoingProductItem>();

            var sells = await db.Table<Sell>().ToListAsync();

            return sells.Where(s => dateById.ContainsKey(s.TransactionId))
                        .OrderByDescending(s => s.TransactionId)
                        .ThenBy(s => s.ProductName)
                        .Select(s => new OutgoingProductItem
                        {
                            TransactionId = s.TransactionId,
                            DateTransacted = dateById[s.TransactionId],
                            ProductID = s.ProductID,
                            ProductName = s.ProductName,
                            Category = s.Category,
                            Price = s.Price,
                            TotalQty = s.Qty,
                            TotalAmount = s.SoldSubtotal
                        })
                        .ToList();
        }
    }
}