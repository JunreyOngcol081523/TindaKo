using SQLite;
using TindaKo.Helpers;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class StockReceivingService
    {
        /// Saves a delivery as ONE database transaction: the header, the product lines,
        /// the stock increase and the new average cost. Either everything is saved or nothing.
        /// THROWS on failure, so the caller keeps the cart and the user can retry safely.
        public static async Task ReceiveStockAsync(IncomingStocks header, IReadOnlyList<ProductDisplay> items)
        {
            ArgumentNullException.ThrowIfNull(header);

            if (items is null || items.Count == 0)
                throw new ArgumentException("Walang laman ang cart.", nameof(items));

            foreach (var i in items)
            {
                if (i.SupplierPrice <= 0 || i.NewStocksQty <= 0)
                    throw new ArgumentException($"Invalid price or quantity for {i.ProdName}.", nameof(items));
            }

            var db = await DatabaseService.GetConnectionAsync();

            await db.RunInTransactionAsync(conn =>
            {
                // one source of truth for the total
                header.TotalAmount = items.Sum(i => i.SupplierPrice * i.NewStocksQty);
                conn.Insert(header); // sets header.IncomingStocksId

                foreach (var item in items)
                {
                    var product = conn.Table<Product>().FirstOrDefault(p => p.ProdId == item.ProdId)
                        ?? throw new InvalidOperationException($"Product not found: {item.ProdName}.");

                    var onHand = product.StocksIn - product.StocksOut;

                    // Cost still unknown but stock exists (e.g. entered by hand): assume the old
                    // units cost the same as this delivery instead of averaging them in at 0.
                    var currentAvg = (product.AverageCost > 0 || onHand <= 0)
                        ? product.AverageCost
                        : item.SupplierPrice;

                    product.AverageCost = CostCalculator.WeightedAverage(
                        onHand, currentAvg, item.NewStocksQty, item.SupplierPrice);
                    product.StocksIn += item.NewStocksQty;
                    conn.Update(product);

                    conn.Insert(new IncomingStocksProductsList
                    {
                        IncomingStocksId = header.IncomingStocksId,
                        ProdId = item.ProdId,
                        ProdName = item.ProdName,
                        SupplierPrice = item.SupplierPrice,
                        StocksInQty = item.NewStocksQty,
                        SubtotalAmount = item.SupplierPrice * item.NewStocksQty
                    });
                }
            });
        }

        /// One-time (idempotent) fill of Product.AverageCost for products that have stock-in
        /// history but no average yet. Call once at app start, after login or in your main menu init.
        /// Assumes the oldest units sold first: it averages the NEWEST deliveries that cover the
        /// quantity currently on hand. Returns how many products were updated.
        public static async Task<int> BackfillAverageCostsAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var updated = 0;

            await db.RunInTransactionAsync(conn =>
            {
                // Filtered in memory on purpose: rows that existed before the column was added
                // can hold NULL, and "AverageCost <= 0" in SQL would silently skip them.
                var products = conn.Table<Product>().ToList()
                    .Where(p => p.AverageCost <= 0)
                    .ToList();

                foreach (var product in products)
                {
                    var lines = conn.Table<IncomingStocksProductsList>()
                        .Where(l => l.ProdId == product.ProdId)
                        .OrderByDescending(l => l.Id) // newest first; Id, not date
                        .ToList();

                    if (lines.Count == 0) continue; // no history -> cost stays unknown (0)

                    var onHand = product.StocksIn - product.StocksOut;
                    decimal avg;

                    if (onHand <= 0)
                    {
                        avg = lines[0].SupplierPrice; // shelf empty: latest known cost
                    }
                    else
                    {
                        var remaining = onHand;
                        decimal value = 0, covered = 0;

                        foreach (var line in lines)
                        {
                            var take = Math.Min(remaining, line.StocksInQty);
                            value += take * line.SupplierPrice;
                            covered += take;
                            remaining -= take;
                            if (remaining <= 0) break;
                        }

                        avg = covered > 0 ? Math.Round(value / covered, 4) : lines[0].SupplierPrice;
                    }

                    product.AverageCost = avg;
                    conn.Update(product);
                    updated++;
                }
            });

            return updated;
        }
    }
}