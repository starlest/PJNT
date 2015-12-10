namespace PutraJayaNT.Models.Sales
{
    using Inventory;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SalesTransactionLine
    {
        public SalesTransactionLine()
        {

        }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string SalesTransactionID { get; set; }
         
        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WarehouseID { get; set; }

        [Required]
        [Column(Order = 3)]
        public int Quantity { get; set; }

        [Required]
        [Column(Order = 4)]
        public decimal SalesPrice { get; set; }

        [Required]
        [Column(Order = 5)]
        public decimal Discount { get; set; }

        [Required]
        [Column(Order = 6)]
        public decimal Total { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }

        [ForeignKey("SalesTransactionID")]
        public virtual SalesTransaction SalesTransaction { get; set; }
    }
}
