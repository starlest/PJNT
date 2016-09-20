namespace ECRP.Models.StockCorrection
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class StockAdjustmentTransaction
    {
        public StockAdjustmentTransaction()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            AdjustStockTransactionLines = new ObservableCollection<StockAdjustmentTransactionLine>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string StockAdjustmentTransactionID { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public virtual ObservableCollection<StockAdjustmentTransactionLine> AdjustStockTransactionLines { get; set; }

        public virtual User User { get; set; }
    }
}
