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
        public string DateTimeTransacted { get; set; } = DateTime.Now.ToString("MMM dd, yyyy h:mm tt");
        public decimal TotalAmount { get; set; }
        public string Username { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // "cash", "credit" 
        public int? BuyerID { get; set; } // Nullable, only used for credit
    }
}
