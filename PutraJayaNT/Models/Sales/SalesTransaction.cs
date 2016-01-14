namespace PutraJayaNT.Models.Sales
{
    using PutraJayaNT.Models.Salesman;
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

        public virtual User User { get; set; }

        public virtual Salesman CollectionSalesman { get; set; }

        public decimal GrossTotal { get; set; }
        
        public decimal Discount { get; set; }

        public decimal SalesExpense { get; set; }

        public decimal Total { get; set; }

        public decimal Paid { get; set; }

        public DateTime? InvoiceIssued { get; set; }

        public DateTime DueDate { get; set; }

        [Index]
        public DateTime When { get; set; }

        public string Notes { get; set; }

        [Required]
        public bool DOPrinted { get; set; }

        [Required]
        public bool InvoicePrinted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesTransactionLine> TransactionLines { get; set; }

        public virtual ICollection<SalesReturnTransaction> SalesReturnTransactions { get; set; }
    }
}
