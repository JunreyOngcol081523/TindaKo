namespace TindaKo.Helpers
{
    /// Pure cost/profit math. No database, no models, so it is easy to unit test.
    public static class CostCalculator
    {
        /// Weighted-average cost after receiving new stock.
        /// onHandQty must be what is on the shelf NOW (StocksIn - StocksOut),
        /// not the lifetime StocksIn total.
        public static decimal WeightedAverage(
            decimal onHandQty,
            decimal currentAvgCost,
            decimal qtyIn,
            decimal unitCost)
        {
            if (qtyIn <= 0) return currentAvgCost;               // nothing received
            if (onHandQty <= 0) return Math.Round(unitCost, 4);  // empty shelf -> new cost wins

            var totalQty = onHandQty + qtyIn;
            var totalValue = (onHandQty * currentAvgCost) + (qtyIn * unitCost);

            return Math.Round(totalValue / totalQty, 4);
        }

        /// Profit of one sale row, or null when the cost is unknown (UnitCost = 0,
        /// e.g. sales made before cost tracking existed). Reports should skip nulls
        /// instead of counting them as 100% margin.
        public static decimal? Profit(decimal sellPrice, decimal unitCost, decimal qty)
        {
            if (unitCost <= 0) return null;
            return (sellPrice - unitCost) * qty;
        }
    }
}