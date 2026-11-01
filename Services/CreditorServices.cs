using SQLite;
using System.Collections.ObjectModel;
using TindaKo.Models;

namespace TindaKo.Services
{
    public static class CreditorServices
    {
        // Kept for existing callers; the connection itself is owned by DatabaseService.
        public static Task Init() => DatabaseService.GetConnectionAsync();

        // ───────────────────────── Read ─────────────────────────

        /// All debts, each with its payments attached and AmountPaid / Balance filled in.
        public static async Task<List<Creditor>> GetCreditorsAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();

            var creditors = await db.Table<Creditor>().ToListAsync();
            var payments = await db.Table<CreditPayment>().ToListAsync();

            var byCreditor = payments
                .GroupBy(p => p.CreditorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(p => p.DatePaid).ThenByDescending(p => p.PaymentId).ToList());

            foreach (var c in creditors)
            {
                if (byCreditor.TryGetValue(c.Id, out var list))
                {
                    c.Payments = new ObservableCollection<CreditPayment>(list);
                    c.AmountPaid = list.Where(p => !p.IsVoided).Sum(p => p.Amount);
                }
            }

            return creditors;
        }

        public static async Task<int> GetOverdueUnpaidCreditsCountAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var today = DateTime.Today;

            // IsPaid is only true when the balance is zero, so partly paid debts still count
            return await db.Table<Creditor>().CountAsync(c => !c.IsPaid && c.DueDate < today);
        }

        // ───────────────────────── Payments ─────────────────────────

        /// Records a payment. The balance check, the payment row and the "fully paid" flag are
        /// ONE database transaction. THROWS (with a user-facing message) if it can't be saved.
        public static async Task AddPaymentAsync(int creditorId, decimal amount, string? note)
        {
            amount = Math.Round(amount, 2);
            if (amount <= 0)
                throw new ArgumentException("Dapat mas malaki sa zero ang bayad.", nameof(amount));

            var db = await DatabaseService.GetConnectionAsync();

            await db.RunInTransactionAsync(conn =>
            {
                var creditor = conn.Find<Creditor>(creditorId)
                    ?? throw new InvalidOperationException("Hindi nahanap ang utang.");

                var paid = conn.Table<CreditPayment>()
                    .Where(p => p.CreditorId == creditorId && !p.IsVoided)
                    .ToList()
                    .Sum(p => p.Amount);

                var balance = Math.Round(creditor.CreditAmount - paid, 2);

                if (amount > balance)
                    throw new InvalidOperationException($"Sobra sa balanse ang bayad. Balanse: ₱{balance:N2}.");

                var now = DateTime.Now;

                conn.Insert(new CreditPayment
                {
                    CreditorId = creditorId,
                    Amount = amount,
                    DatePaid = now,
                    Note = note?.Trim() ?? string.Empty
                });

                if (amount >= balance)
                {
                    creditor.IsPaid = true;
                    creditor.PaidDate = now;
                    conn.Update(creditor);
                }
            });
        }

        /// Cancels a payment (it stays in the history as voided). If the debt was fully paid,
        /// it becomes unpaid again. One database transaction.
        public static async Task VoidPaymentAsync(int paymentId)
        {
            var db = await DatabaseService.GetConnectionAsync();

            await db.RunInTransactionAsync(conn =>
            {
                var payment = conn.Find<CreditPayment>(paymentId)
                    ?? throw new InvalidOperationException("Hindi nahanap ang bayad.");

                if (payment.IsVoided) return;

                payment.IsVoided = true;
                conn.Update(payment);

                var creditor = conn.Find<Creditor>(payment.CreditorId);
                if (creditor is null) return;

                var paid = conn.Table<CreditPayment>()
                    .Where(p => p.CreditorId == creditor.Id && !p.IsVoided)
                    .ToList()
                    .Sum(p => p.Amount);

                var balance = Math.Round(creditor.CreditAmount - paid, 2);

                if (balance > 0 && creditor.IsPaid)
                {
                    creditor.IsPaid = false;
                    creditor.PaidDate = null;
                    conn.Update(creditor);
                }
            });
        }

        // ───────────────────────── Notes ─────────────────────────

        /// Updates ONLY the Notes column. The old autosave rewrote the whole row from a possibly
        /// stale object, which could overwrite IsPaid or other fields changed in the meantime.
        public static async Task UpdateNotesAsync(int creditorId, string? notes)
        {
            var db = await DatabaseService.GetConnectionAsync();
            var table = new TableMapping(typeof(Creditor)).TableName;

            await db.ExecuteAsync($"UPDATE \"{table}\" SET Notes = ? WHERE Id = ?", notes ?? string.Empty, creditorId);
        }

        // ───────────────────────── Other writes ─────────────────────────

        /// Standalone insert. For a sale on utang, pass the Creditor to
        /// TransactionServices.SaveTransactionWithSalesAsync instead so both are saved atomically.
        public static async Task SaveCreditorAsync(Creditor creditor)
        {
            var db = await DatabaseService.GetConnectionAsync();
            await db.InsertAsync(creditor);
        }

        /// Whole-row update. Avoid for notes or payments (use the methods above).
        public static async Task<int> UpdateCreditorAsync(Creditor creditor)
        {
            var db = await DatabaseService.GetConnectionAsync();
            return await db.UpdateAsync(creditor);
        }

        // ───────────────────────── Migration ─────────────────────────

        /// One-time, idempotent: debts that were already marked paid get one payment record for
        /// their full amount, so Balance = amount - payments stays correct for old data.
        /// Call once at app start. Returns how many records were created.
        public static async Task<int> MigrateLegacyPaidAsync()
        {
            var db = await DatabaseService.GetConnectionAsync();
            var created = 0;

            await db.RunInTransactionAsync(conn =>
            {
                var hasPayment = conn.Table<CreditPayment>().ToList()
                    .Select(p => p.CreditorId)
                    .ToHashSet();

                var legacy = conn.Table<Creditor>().ToList()
                    .Where(c => c.IsPaid && !hasPayment.Contains(c.Id))
                    .ToList();

                foreach (var c in legacy)
                {
                    conn.Insert(new CreditPayment
                    {
                        CreditorId = c.Id,
                        Amount = c.CreditAmount,
                        DatePaid = c.PaidDate ?? c.CreditDate,
                        Note = "Migrated: marked as paid before payment tracking"
                    });
                    created++;
                }
            });

            return created;
        }
    }
}
