using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class Users
    {
        [PrimaryKey]
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserRole { get; set; }
    }
}
    

