using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class UserServices
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
            var userCount = await _db.Table<Users>().CountAsync();
            if (userCount == 0)
            {
                var defaultUser = new Users
                {
                    UserName = "admin",
                    UserFullName = "Administrator",
                    UserPassword = "admin123", // You can hash this later
                    UserRole = "May-ari"
                };

                await _db.InsertAsync(defaultUser);
            }
        }
        public static async Task<Users> GetUserAsync(string username, string password)
        {
            await Init();
            return await _db.Table<Users>()
                      .Where(u => u.UserName == username && u.UserPassword == password)
                      .FirstOrDefaultAsync();
        }
        public static async Task<List<Users>> GetUsersAsync()
        {
            await Init();
            return await _db.Table<Users>().ToListAsync();
        }

        public static async Task<int> UpdateUserAsync(Users user)
        {
            await Init();
            return await _db.UpdateAsync(user);
        }
        public static async Task<int> AddNewUserAsync(Users user)
        {
            await Init();
            return await _db.InsertAsync(user);
        }
    }
}
