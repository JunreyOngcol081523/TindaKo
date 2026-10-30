using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class OutgoingProductItem
    {
        public int TransactionId { get; set; }
        public string DateTransacted { get; set; } = string.Empty;
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal TotalQty { get; set; }      // qty on this line
        public decimal TotalAmount { get; set; }   // price × qty on this line
    }
}
