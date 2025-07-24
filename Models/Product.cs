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

        public string ProdName { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public decimal StocksIn { get; set; } = 0;

        public decimal StocksOut { get; set; } = 0;
        [Ignore] // This tells SQLite to ignore this property
        public decimal CurrentStock => StocksIn - StocksOut;


        private decimal quantity;

        [Ignore]
        public decimal Quantity
        {
            get => quantity;
            set => SetProperty(ref quantity, value);
        }
    }

}
