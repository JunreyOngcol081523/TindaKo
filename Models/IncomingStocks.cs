using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class IncomingStocks
    {
        [PrimaryKey, AutoIncrement]
        public int IncomingStocksId { get; set; }
        public DateTime DateStocksIn { get; set; }
        public string? SupplierName { get; set; }
        public decimal TotalAmout { get; set; }
    }
    public class IncomingStocksProductsList
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int IncomingStocksId { get; set; }
        public int ProdId { get; set; }//foreign key to product table
        public string ProdName { get; set; }//data from product table
        public decimal SupplierPrice { get; set; }
        public decimal StocksInQty { get; set; }
        public decimal SubtotalAmount { get; set; }
    }
}
