using SQLite;
using System.Collections.ObjectModel;

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
        public decimal Qty { get; set; }
        [Ignore] // This tells SQLite to ignore this property
        public decimal SoldSubtotal => Price * Qty;
    }
}
