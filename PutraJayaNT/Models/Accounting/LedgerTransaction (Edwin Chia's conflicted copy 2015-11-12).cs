using MVVMFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PUJASM.ERP.Models.Accounting
{
    [Table("Ledger_Transactions")]
    public class LedgerTransaction : ObservableObject
    { 
        public int ID { get; set; }
        
        [Required]
        public virtual LedgerAccount Account { get; set; } 

        [Required]
        public DateTime DateStamp { get; set; }

        public string Documentation { get; set; }

        public string Description { get; set; }
        
        [Required]
        public string Seq { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
