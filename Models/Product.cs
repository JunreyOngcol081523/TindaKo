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

        public int StocksIn { get; set; } = 0;

        public int StocksOut { get; set; } = 0;
        [Ignore] // This tells SQLite to ignore this property
        public int CurrentStock => StocksIn - StocksOut;


        private decimal quantity;

        [Ignore]
        public decimal Quantity
        {
            get => quantity;
            set => SetProperty(ref quantity, value);
        }
    }

}
