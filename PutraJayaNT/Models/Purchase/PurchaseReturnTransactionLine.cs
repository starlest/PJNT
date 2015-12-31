using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Purchase
{
    public class PurchaseReturnTransactionLine : ObservableObject
    {
        int _quantity;

        [Key]
        [Column("PurchaseReturnTransactionID", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string PurchaseReturnTransactionID { get; set; }

        [Key]
        [Column("ItemID", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Key]
        [Column("WarehouseID", Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WarehouseID { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal PurchasePrice { get; set; }

        [Key]
        [Column(Order = 4)]
        public decimal Discount { get; set; }

        [Required]
        public decimal NetDiscount { get; set; }

        [Required]
        public int Quantity
        {
            get { return _quantity; }
            set { SetProperty(ref _quantity, value, "Quantity"); }
        }

        [Required]
        public decimal Total { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }

        [ForeignKey("PurchaseReturnTransactionID")]
        public virtual PurchaseReturnTransaction PurchaseReturnTransaction { get; set; }
    }
}
