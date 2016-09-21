namespace ECRP.Models.Accounting
{
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using MVVMFramework;

    [Table("Ledger_Accounts")]
    public class LedgerAccount : ObservableObject
    {
        public LedgerAccount()
        {
            Notes = "";
            // ReSharper disable once VirtualMemberCallInContructor
            LedgerTransactionLines = new ObservableCollection<LedgerTransactionLine>();
        }

        public int ID { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string Name { get; set; }

        public string Notes { get; set; }

        public virtual LedgerAccountClass LedgerAccountClass { get; set; }

        public virtual LedgerAccountGroup LedgerAccountGroup { get; set; }

        public virtual LedgerGeneral LedgerGeneral { get; set; }

        public virtual ObservableCollection<LedgerAccountBalance> LedgerAccountBalances { get; set; }

        public virtual ObservableCollection<LedgerTransactionLine> LedgerTransactionLines { get; set; }
    }
}
