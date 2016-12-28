namespace ECERP.Models.Accounting
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Ledger_Account_Classes")]
    public class LedgerAccountClass
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }
}
