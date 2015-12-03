using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    public class PurchaseReturnTransaction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0), StringLength(128)]
        public string PurchaseReturnTransactionID { get; set; }

        public virtual PurchaseTransaction PurchaseTransaction { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public virtual List<PurchaseReturnTransactionLine> TransactionLines { get; set; }
    }
}
