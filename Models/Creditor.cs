using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System.Collections.ObjectModel;

namespace TindaKo.Models
{
    /// One utang (debt). The amount still owed is CreditAmount minus its non-voided payments.
    public partial class Creditor : ObservableObject
    {
        public Creditor() { }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// Kept for display and for old rows. New rows also store BuyerId.
        public string CreditorName { get; set; } = string.Empty;

        /// Link to Buyer (0 = unknown, e.g. rows created before this field existed).
        public int BuyerId { get; set; }

        /// Link to the sale that created this debt (0 = unknown).
        public int TransactionId { get; set; }

        public DateTime CreditDate { get; set; }

        /// The ORIGINAL amount of the debt.
        public decimal CreditAmount { get; set; }

        public DateTime DueDate { get; set; }

        /// True once the balance reaches zero. Maintained by CreditorServices, never set by the UI.
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                    NotesChanged?.Invoke(this);
                }
            }
        }

        // Event callback so the ViewModel can debounce the notes autosave
        public event Action<Creditor>? NotesChanged;

        // ───────────── Not stored in the database ─────────────

        private decimal _amountPaid;

        /// Sum of non-voided payments. Filled by CreditorServices.GetCreditorsAsync.
        [Ignore]
        public decimal AmountPaid
        {
            get => _amountPaid;
            set
            {
                if (SetProperty(ref _amountPaid, value))
                {
                    OnPropertyChanged(nameof(Balance));
                    OnPropertyChanged(nameof(ProgressFraction));
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        [Ignore]
        public decimal Balance => IsPaid ? 0m : Math.Max(Math.Round(CreditAmount - AmountPaid, 2), 0m);

        [Ignore]
        public double ProgressFraction =>
            IsPaid ? 1.0
            : CreditAmount > 0 ? Math.Clamp((double)(AmountPaid / CreditAmount), 0.0, 1.0)
            : 0.0;

        /// "Paid", "Partial" or "Unpaid" (the page colors the badge from this).
        [Ignore]
        public string Status => IsPaid ? "Paid" : AmountPaid > 0 ? "Partial" : "Unpaid";

        [Ignore]
        public ObservableCollection<CreditPayment> Payments { get; set; } = new();

        [Ignore]
        public bool HasPayments => Payments.Count > 0;

        [Ignore]
        public string PaymentsHeader => $"Payments ({Payments.Count(p => !p.IsVoided)})";

        // UI state (survives list filtering; the ViewModel copies it across reloads)
        private bool _isNotesExpanded;
        [Ignore]
        public bool IsNotesExpanded
        {
            get => _isNotesExpanded;
            set => SetProperty(ref _isNotesExpanded, value);
        }

        private bool _isHistoryExpanded;
        [Ignore]
        public bool IsHistoryExpanded
        {
            get => _isHistoryExpanded;
            set => SetProperty(ref _isHistoryExpanded, value);
        }

        private bool _isPayPanelOpen;
        [Ignore]
        public bool IsPayPanelOpen
        {
            get => _isPayPanelOpen;
            set => SetProperty(ref _isPayPanelOpen, value);
        }

        private string _payAmountText = string.Empty;
        [Ignore]
        public string PayAmountText
        {
            get => _payAmountText;
            set => SetProperty(ref _payAmountText, value);
        }

        private string _payNote = string.Empty;
        [Ignore]
        public string PayNote
        {
            get => _payNote;
            set => SetProperty(ref _payNote, value);
        }
    }

    /// One payment received against a debt. Payments are never deleted, only voided,
    /// so there is always a record of what happened.
    public class CreditPayment
    {
        [PrimaryKey, AutoIncrement]
        public int PaymentId { get; set; }

        [Indexed]
        public int CreditorId { get; set; }

        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool IsVoided { get; set; }

        [Ignore]
        public bool CanVoid => !IsVoided;

        [Ignore]
        public bool HasNote => !string.IsNullOrWhiteSpace(Note);
    }
}