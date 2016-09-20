namespace ECRP.Models.Purchase
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using MVVMFramework;
    using Supplier;

    public class PurchaseTransaction : ObservableObject
    {
        public PurchaseTransaction()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            PurchaseTransactionLines = new ObservableCollection<PurchaseTransactionLine>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0), StringLength(128)]
        public string PurchaseID { get; set; }

        public string DOID { get; set; }

        [Required]
        public Supplier Supplier { get; set; }

        [Required, Index]
        public DateTime Date { get; set; }

        [Required, Index]
        public DateTime DueDate { get; set; }

        [Required]
        public decimal GrossTotal { get; set; }

        [Required]
        public decimal Discount { get; set; }

        [Required]
        public decimal Tax { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public decimal Paid { get; set; }

        public string Note { get; set; }

        public virtual User User { get; set; }

        public virtual ObservableCollection<PurchaseTransactionLine> PurchaseTransactionLines { get; set; }

        public virtual ICollection<PurchaseReturnTransaction> PurchaseReturnTransactions { get; set; }

        [NotMapped]
        public decimal Remaining { get; set; }
    }
}
