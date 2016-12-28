namespace ECERP.Models.Accounting
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Ledger_Transaction_Lines")]
    public class LedgerTransactionLine
    {
        [Key, ForeignKey("LedgerTransaction")]
        [Column(Order = 0)]
        public int LedgerTransactionID { get; set; }

        [Key, ForeignKey("LedgerAccount")]
        [Column(Order = 1)]
        public int LedgerAccountID { get; set; }

        [Required]
        public string Seq { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public virtual LedgerTransaction LedgerTransaction { get; set; }

        [Required]
        public virtual LedgerAccount LedgerAccount { get; set; }
    }
}
