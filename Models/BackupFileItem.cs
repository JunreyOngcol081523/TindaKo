using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Models
{
    public class BackupFileItem
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsMainDatabase { get; set; }
    }

}
