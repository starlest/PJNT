namespace ECRP.Models.StockCorrection
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Inventory;

    public class StockMovementTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string StockMovementTransactionID { get; set; }

        public DateTime Date { get; set; }

        public virtual ObservableCollection<StockMovementTransactionLine> StockMovementTransactionLines { get; set; }

        public virtual User User { get; set; }

        public virtual Warehouse FromWarehouse { get; set; }

        public virtual Warehouse ToWarehouse { get; set; }
    }
}
