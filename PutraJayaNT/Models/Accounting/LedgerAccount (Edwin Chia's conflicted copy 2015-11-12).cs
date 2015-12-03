using MVVMFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PUJASM.ERP.Models.Accounting
{
    [Table("Ledger_Accounts")]
    public class LedgerAccount : ObservableObject
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Notes { get; set; }

        [Required]
        public string Class { get; set; }

        public virtual LedgerGeneral LedgerGeneral { get; set; }
    }
}
