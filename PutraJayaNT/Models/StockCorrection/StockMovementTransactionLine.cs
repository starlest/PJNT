using PutraJayaNT.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.StockCorrection
{
    public class StockMovementTransactionLine
    {
        [Key]
        [Column(Order = 0)]
        public string StockMovementTransactionID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("StockMovementTransactionID")]
        public virtual StockMovementTransaction StockMovementTransaction { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }
    }
}
