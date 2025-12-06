using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace TindaKo.Models
{
    public partial class Creditor : ObservableObject
    {
        public Creditor() { }
       
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CreditorName { get; set; }
        public DateTime CreditDate { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        private string _notes;
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

        // Event callback so ViewModel can debounce
        public event Action<Creditor> NotesChanged;
        // UI state properties (not stored in database)
        [Ignore]
        public bool IsNotesExpanded { get; set; }

        [Ignore]
        public bool IsEditingNotes { get; set; }

        [Ignore]
        public string TempNotes { get; set; }
    }
}
