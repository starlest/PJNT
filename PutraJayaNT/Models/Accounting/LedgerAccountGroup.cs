namespace ECERP.Models.Accounting
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Ledger_Account_Groups")]
    public class LedgerAccountGroup
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }
}