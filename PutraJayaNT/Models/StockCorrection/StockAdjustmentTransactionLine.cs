using PutraJayaNT.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.StockCorrection
{
    public class StockAdjustmentTransactionLine
    {
        [Key]
        [Column(Order = 0)]
        public string StockAdjustmentTransactionID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WarehouseID { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("StockAdjustmentTransactionID")]
        public virtual StockAdjustmentTransaction StockAdjustmentTransaction { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }
    }
}
