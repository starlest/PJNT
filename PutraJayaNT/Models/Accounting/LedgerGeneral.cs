using MVVMFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PutraJayaNT.Models.Accounting
{
    [Table("Ledger_General")]
    public class LedgerGeneral : ObservableObject
    {
        [Key, ForeignKey("LedgerAccount")]
        public int ID { get; set; }

        [Required]
        public int PeriodYear { get; set; }

        [Required]
        public int Period { get; set; }

        [Required]
        public decimal Debit { get; set; }

        [Required]
        public decimal Credit { get; set; }

        public virtual LedgerAccount LedgerAccount { get; set; }
    }
}
