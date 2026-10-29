using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class TransactionDisplay
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string DateTimeTransacted { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Username { get; set; } = string.Empty;
        public int? BuyerID { get; set; }

        public string BuyerName { get; set; } = string.Empty;
    }

}
