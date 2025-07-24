using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class CartItems:ObservableObject
    {
        public int ProductId { get; set; }
        public string ProdName { get; set; }
        public string ProdCategory { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }

        public decimal Subtotal{get; set; }
    }
}
