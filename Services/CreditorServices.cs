using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class CreditorServices
    {
        private static SQLiteAsyncConnection _db;
        public static async Task Init()
        {
            _db = await DatabaseService.GetConnectionAsync();
        }
        public static async Task<List<Creditor>> GetCreditorsAsync()
        {
            await Init();
            return await _db.Table<Creditor>().ToListAsync();
        }
        public static async Task<int> UpdateCreditorAsync(Creditor creditor)
        {
            await Init();
            return await _db.UpdateAsync(creditor);
        }
        public static async Task SaveCreditorAsync(Creditor creditor)
        {
            await Init();
            await _db.InsertAsync(creditor);
        }
        public static async Task<int> GetOverdueUnpaidCreditsCountAsync()
        {
            await Init(); // Ensure database is initialized

            var today = DateTime.Today;

            var overdueUnpaidCredits = await _db.Table<Creditor>()
                .Where(c => !c.IsPaid && c.DueDate < today)
                .ToListAsync();

            return overdueUnpaidCredits.Count;
        }

    }
}
