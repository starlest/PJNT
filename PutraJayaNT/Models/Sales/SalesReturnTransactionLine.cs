using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Sales
{
    public class SalesReturnTransactionLine : ObservableObject
    {
        int _quantity;
        decimal _costOfGoodsSold;

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string SalesReturnTransactionID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WarehouseID { get; set; }
        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public decimal SalesPrice { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public decimal Discount { get; set; }

        [Required]
        public int Quantity
        {
            get { return _quantity; }
            set { SetProperty(ref _quantity, value, "Quantity"); }
        }

        [Required]
        public decimal CostOfGoodsSold
        {
            get { return _costOfGoodsSold; }
            set { SetProperty(ref _costOfGoodsSold, value, "CostOfGoodsSold"); }
        }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }

        [ForeignKey("SalesReturnTransactionID")]
        public virtual SalesReturnTransaction SalesReturnTransaction { get; set; }
    }
}
