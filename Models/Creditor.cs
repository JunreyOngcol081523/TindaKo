using SQLite;

namespace TindaKo.Models
{
    public partial class Creditor
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

    }
}
