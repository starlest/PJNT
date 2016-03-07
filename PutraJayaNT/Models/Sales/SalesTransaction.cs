namespace PutraJayaNT.Models.Sales
{
    using Salesman;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class SalesTransaction
    {
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
        public DateTime Date { get; set; }

        public string Notes { get; set; }

        [Required]
        public bool DOPrinted { get; set; }

        [Required]
        public bool InvoicePrinted { get; set; }

        public virtual ICollection<SalesTransactionLine> TransactionLines { get; set; }

        public virtual ICollection<SalesReturnTransaction> SalesReturnTransactions { get; set; }
    }
}
