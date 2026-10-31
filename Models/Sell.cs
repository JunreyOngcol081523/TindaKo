using SQLite;
using TindaKo.Helpers;

namespace TindaKo.Models
{
    public class Sell
    {
        [PrimaryKey, AutoIncrement]
        public int SellId { get; set; }
        public int TransactionId { get; set; } // Foreign key to Transaction table
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }

        /// Product.AverageCost at the moment of the sale (0 = unknown, e.g. sales made
        /// before cost tracking existed). Set by TransactionServices inside the save transaction.
        public decimal UnitCost { get; set; }

        public decimal Qty { get; set; }

        [Ignore] // This tells SQLite to ignore this property
        public decimal SoldSubtotal => Price * Qty;

        /// Profit of this row, or null when the cost is unknown (never treat null as 100% margin).
        [Ignore]
        public decimal? LineProfit => CostCalculator.Profit(Price, UnitCost, Qty);
    }
}