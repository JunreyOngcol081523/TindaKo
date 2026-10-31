using SQLite;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class CreditorServices
    {
        // Kept for existing callers; the connection itself is owned by DatabaseService.
        public static Task Init() => DatabaseService.GetConnectionAsync();

        public static async Task<List<Creditor>> GetCreditorsAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            return await db.Table<Creditor>().ToListAsync();
        }

        public static async Task<int> UpdateCreditorAsync(Creditor creditor)
        {
            var db = await DatabaseService.GetConnectionAsync();
            return await db.UpdateAsync(creditor);
        }

        /// Standalone insert. For a sale on utang, pass the Creditor to
        /// TransactionServices.SaveTransactionWithSalesAsync instead so both are saved atomically.
        public static async Task SaveCreditorAsync(Creditor creditor)
        {
            var db = await DatabaseService.GetConnectionAsync();
            await db.InsertAsync(creditor);
        }

        public static async Task<int> GetOverdueUnpaidCreditsCountAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var today = DateTime.Today;

            // counted by SQLite, no need to load the rows
            return await db.Table<Creditor>().CountAsync(c => !c.IsPaid && c.DueDate < today);
        }
    }
}