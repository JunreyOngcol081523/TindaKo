using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class Buyer
    {
        [PrimaryKey, AutoIncrement]
        public int BuyerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}
