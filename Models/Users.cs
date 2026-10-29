using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class Users : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
    

