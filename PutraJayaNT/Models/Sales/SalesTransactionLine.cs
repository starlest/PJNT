namespace ECERP.Models.Sales
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Inventory;
    using Salesman;

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

        public bool Equals2(object obj)
        {
            var line = obj as SalesTransactionLine;
            if (line == null) return false;
            else return line.Item.ItemID.Equals(this.Item.ItemID) && line.Warehouse.ID.Equals(this.Warehouse.ID)
                && Math.Round(line.SalesPrice, 2).Equals(Math.Round(this.SalesPrice, 2))
                && Math.Round(line.Discount, 2).Equals(Math.Round(this.Discount, 2));
        }
    }
}
