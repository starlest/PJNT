using MVVMFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    public class PurchaseTransactionLine : ObservableObject
    {
        [Key, ForeignKey("PurchaseTransaction")]
        [Column(Order = 0)]
        public string PurchaseID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string ItemID { get; set; }

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Total { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        public virtual PurchaseTransaction PurchaseTransaction { get; set; }
    }
}
