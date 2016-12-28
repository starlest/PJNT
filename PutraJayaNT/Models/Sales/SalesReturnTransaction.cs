namespace ECERP.Models.Sales
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class SalesReturnTransaction
    {
        public SalesReturnTransaction()
        {
            SalesReturnTransactionLines = new List<SalesReturnTransactionLine>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0), StringLength(128)]
        public string SalesReturnTransactionID { get; set; }

        public virtual SalesTransaction SalesTransaction { get; set; }

        public virtual User User { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal NetTotal { get; set; }

        public virtual List<SalesReturnTransactionLine> SalesReturnTransactionLines { get; set; }
    }
}
