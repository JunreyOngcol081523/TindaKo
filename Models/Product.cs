using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace TindaKo.Models
{
   

    public class Product
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
    }

}
