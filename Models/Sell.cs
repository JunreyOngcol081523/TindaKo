using SQLite;
using System.Collections.ObjectModel;

namespace TindaKo.Models
{
    public class Sell
    {
        [PrimaryKey, AutoIncrement]
        public int SellId { get; set; }
        public int TransactionId { get; set; } 
        public int ProductID { get; set; }// Foreign key to Transaction table
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public decimal Qty { get; set; }
        [Ignore] // This tells SQLite to ignore this property
        public decimal SoldSubtotal => Price * Qty;


    }
}
