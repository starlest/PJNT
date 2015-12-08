namespace PutraJayaNT.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SalesTransaction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SalesTransaction()
        {
            TransactionLines = new HashSet<SalesTransactionLine>();
            Paid = 0;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0), StringLength(128)]
        public string SalesTransactionID { get; set; }

        public virtual Customer Customer { get; set; }

        public decimal Total { get; set; }

        public decimal Paid { get; set; }

        public DateTime? InvoiceIssued { get; set; }

        [Index]
        public DateTime When { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesTransactionLine> TransactionLines { get; set; }

        public virtual ICollection<SalesReturnTransaction> SalesReturnTransactions { get; set; }
    }
}
