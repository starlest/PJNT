namespace PutraJayaNT.Models.Sales
{
    using Inventory;
    using PutraJayaNT.Models.Salesman;
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

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public decimal SalesPrice { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public decimal Discount { get; set; }

        [Required]
        [Column(Order = 5)]
        public int Quantity { get; set; }

        [Required]
        [Column(Order = 6)]
        public decimal Total { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }

        [ForeignKey("SalesTransactionID")]
        public virtual SalesTransaction SalesTransaction { get; set; }

        [ForeignKey("Salesman")]
        [Column("Salesman_ID")]
        public int Salesman_ID { get; set; }


        public virtual Salesman Salesman { get; set; }
    }
}
