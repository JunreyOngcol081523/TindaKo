using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Helpers
{
    public static class Session
    {
        public static string Username { get; private set; } = string.Empty;

        public static bool IsActive => !string.IsNullOrWhiteSpace(Username);

        public static void Start(string username) => Username = username?.Trim() ?? string.Empty;

        public static void Clear() => Username = string.Empty;
    }   
}
