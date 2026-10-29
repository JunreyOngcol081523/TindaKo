using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TindaKo.Models
{
   

    public class Product : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int ProdId { get; set; }

        public string ProdName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal StocksIn { get; set; } = 0;

        public decimal StocksOut { get; set; } = 0;
        [Ignore] // This tells SQLite to ignore this property
        public decimal CurrentStock => StocksIn - StocksOut;


        private decimal quantity;
        public decimal Quantity
        {
            get => quantity;
            set
            {
                if (SetProperty(ref quantity, value))
                {
                    OnPropertyChanged(nameof(Subtotal)); // Notify UI to re-read Subtotal
                }
            }
        }

        [Ignore]
        public decimal Subtotal => Price * Quantity;

    }

    public class ProductDisplay : ObservableObject
    {
        public ProductDisplay(Product product) { 
            ProdId = product.ProdId;
            ProdName = product.ProdName;
        }
        public ProductDisplay() { }
        public int ProdId { get; set; }
  
        public string ProdName { get; set; } = string.Empty;
        
        private decimal supplierPrice;
 
        public decimal SupplierPrice
        {
            get => supplierPrice;
            set
            {
                SetProperty(ref supplierPrice, value);
                RecalculateSubtotal();
            }
        }

        private decimal newStocksQty;

        public decimal NewStocksQty
        {
            get => newStocksQty;
            set
            {
                SetProperty(ref newStocksQty, value);
                RecalculateSubtotal();
            }
        }

        private decimal subtotal;

        public decimal Subtotal
        {
            get => subtotal;
            private set => SetProperty(ref subtotal, value);
        }

        private void RecalculateSubtotal()
        {
            Subtotal = SupplierPrice * NewStocksQty;
        }
    }


}
