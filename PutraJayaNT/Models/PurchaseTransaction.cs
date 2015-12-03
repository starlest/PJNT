using MVVMFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    public class PurchaseTransaction : ObservableObject
    {
        public PurchaseTransaction()
        {
            PurchaseTransactionLines = new ObservableCollection<PurchaseTransactionLine>();
            Paid = 0;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0)]
        public string PurchaseID { get; set; }

        [Required]
        public Supplier Supplier { get; set; }

        [Required, Index]
        public DateTime Date { get; set; }

        [Required, Index]
        public DateTime DueDate { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public decimal Paid { get; set; }

        public virtual ObservableCollection<PurchaseTransactionLine> PurchaseTransactionLines { get; set; }

        public virtual ICollection<PurchaseReturnTransaction> PurchaseReturnTransactions { get; set; }
    }
}
