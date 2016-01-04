using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.StockCorrection
{
    public class DecreaseStockTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string DecreaseStrockTransactionID { get; set; }

        public DateTime Date { get; set; }

        public virtual ObservableCollection<DecreaseStockTransactionLine> DecreaseStockTransactionLines { get; set; }

        public virtual User User { get; set; }
    }
}
