using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class BuyerServices
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
        }
        public static async Task<int> AddBuyerAsync(Buyer buyer) { 
            await Init();
            return await _db.InsertAsync(buyer);
        }

        public static async Task<List<Buyer>> GetBuyersAsync() {
            await Init();
            return await _db.Table<Buyer>().ToListAsync();
        }
        
        public static async Task<Buyer> GetBuyerAsync(int? buyerId)
        {
            await Init();
            return await _db.Table<Buyer>().Where(b => b.BuyerId == buyerId).FirstOrDefaultAsync();
        }
    }
}
