using MVVMFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Accounting
{
    [Table("Ledger_Account_Balances")]
    public class LedgerAccountBalance : ObservableObject
    {
        public LedgerAccountBalance()
        {
            BeginningBalance = 0;
            Balance1 = 0;
            Balance2 = 0;
            Balance3 = 0;
            Balance4 = 0;
            Balance5 = 0;
            Balance6 = 0;
            Balance7 = 0;
            Balance8 = 0;
            Balance9 = 0;
            Balance10 = 0;
            Balance11 = 0;
            Balance12 = 0;
        }

        [Key, ForeignKey("LedgerAccount")]
        public int ID { get; set; }

        [Required]
        public int PeriodYear { get; set; }

        [Required]
        public decimal BeginningBalance { get; set; }

        [Required]
        public decimal Balance1 { get; set; }

        [Required]
        public decimal Balance2 { get; set; }

        [Required]
        public decimal Balance3 { get; set; }

        [Required]
        public decimal Balance4 { get; set; }

        [Required]
        public decimal Balance5 { get; set; }

        [Required]
        public decimal Balance6 { get; set; }

        [Required]
        public decimal Balance7 { get; set; }

        [Required]
        public decimal Balance8 { get; set; }

        [Required]
        public decimal Balance9 { get; set; }

        [Required]
        public decimal Balance10 { get; set; }

        [Required]
        public decimal Balance11 { get; set; }

        [Required]
        public decimal Balance12 { get; set; }

        public virtual LedgerAccount LedgerAccount { get; set; }
    }
}
