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
        public string DateTimeISO { get; set; } // ISO 8601 timestamp: 2025-07-22T16:30:00
        public decimal TotalAmount { get; set; }
        public string Username { get; set; }
        public string TransactionType { get; set; } // "cash", "credit"
        public string? BuyerName { get; set; } // Nullable, only used for credit
    }
}
