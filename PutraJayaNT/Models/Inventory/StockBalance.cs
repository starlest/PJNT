using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Inventory
{
    public class StockBalance
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WarehouseID { get; set; }

        [Key]
        [Column(Order = 2)]
        public int Year { get; set; }

        [Required]
        public int BeginningBalance { get; set; }

        [Required]
        public int Balance1 { get; set; }

        [Required]
        public int Balance2 { get; set; }

        [Required]
        public int Balance3 { get; set; }

        [Required]
        public int Balance4 { get; set; }

        [Required]
        public int Balance5 { get; set; }

        [Required]
        public int Balance6 { get; set; }

        [Required]
        public int Balance7 { get; set; }

        [Required]
        public int Balance8 { get; set; }

        [Required]
        public int Balance9 { get; set; }

        [Required]
        public int Balance10 { get; set; }

        [Required]
        public int Balance11 { get; set; }

        [Required]
        public int Balance12 { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }
    }
}
