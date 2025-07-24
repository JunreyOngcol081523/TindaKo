using SQLite;
using System.Collections.ObjectModel;

namespace TindaKo.Models
{
    public class Sell
    {
        [PrimaryKey, AutoIncrement]
        public int SellId { get; set; }
        public int TransactionId { get; set; } // Foreign key to Transaction table
        public int SoldProdId { get; set; } // foreign key to Product
        public string SoldName { get; set; }
        public string SoldCategory { get; set; }
        public decimal SoldPrice { get; set; }
        public decimal SoldQty { get; set; }
        [Ignore] // This tells SQLite to ignore this property
        public decimal SoldSubtotal => SoldPrice * SoldQty;


    }
}
