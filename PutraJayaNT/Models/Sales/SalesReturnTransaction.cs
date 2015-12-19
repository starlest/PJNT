using PutraJayaNT.Models.Sales;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Sales
{
    public class SalesReturnTransaction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0), StringLength(128)]
        public string SalesReturnTransactionID { get; set; }

        public virtual SalesTransaction SalesTransaction { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public virtual List<SalesReturnTransactionLine> TransactionLines { get; set; }
    }
}
