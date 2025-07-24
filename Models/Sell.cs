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
        public decimal SoldPrice { get; set; }
        public int SoldQty { get; set; }
        [Ignore] // This tells SQLite to ignore this property
        public decimal SoldSubtotal => SoldPrice * SoldQty;


    }
}
