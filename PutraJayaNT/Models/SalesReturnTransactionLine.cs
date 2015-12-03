using MVVMFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    public class SalesReturnTransactionLine : ObservableObject
    {
        int _quantity;
        decimal _costOfGoodsSold;

        [Key]
        [Column("SalesReturnTransactionID", Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string SalesReturnTransactionID { get; set; }

        [Key]
        [Column("ItemID", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Required]
        public int Quantity
        {
            get { return _quantity; }
            set { SetProperty(ref _quantity, value, "Quantity"); }
        }

        [Required]
        public decimal SalesPrice { get; set; }

        [Required]
        public decimal CostOfGoodsSold
        {
            get { return _costOfGoodsSold; }
            set { SetProperty(ref _costOfGoodsSold, value, "CostOfGoodsSold"); }
        }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("SalesReturnTransactionID")]
        public virtual SalesReturnTransaction SalesReturnTransaction { get; set; }
    }
}
