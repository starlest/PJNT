using PutraJayaNT.Models.Inventory;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.StockCorrection
{
    public class MoveStockTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MoveStrockTransactionID { get; set; }

        public DateTime Date { get; set; }

        public virtual ObservableCollection<MoveStockTransactionLine> MoveStockTransactionLines { get; set; }

        public virtual User User { get; set; }

        public virtual Warehouse FromWarehouse { get; set; }

        public virtual Warehouse ToWarehouse { get; set; }
    }
}
