using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int TransactionId { get; set; }
        public string DateTimeTransacted { get; set; } 
        public decimal TotalAmount { get; set; }
        public string Username { get; set; }
        public string TransactionType { get; set; } // "cash", "credit"
        public int? BuyerID { get; set; } // Nullable, only used for credit
    }
}
