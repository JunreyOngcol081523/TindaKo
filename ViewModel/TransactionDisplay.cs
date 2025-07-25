using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.ViewModel
{
    public class TransactionDisplay
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string DateTimeTransacted { get; set; }
        public decimal TotalAmount { get; set; }
        public string Username { get; set; }
        public int? BuyerID { get; set; }

        public string BuyerName { get; set; } // 🟢 Add this
    }

}
