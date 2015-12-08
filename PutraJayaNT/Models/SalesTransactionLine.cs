namespace PutraJayaNT.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SalesTransactionLine
    {
        public SalesTransactionLine()
        {

        }

        public SalesTransactionLine(SalesTransactionLine line)
        {
            SalesTransactionID = line.SalesTransactionID;
            ItemID = line.ItemID;
            Quantity = line.Quantity;
            SalesPrice = line.SalesPrice;
            Total = line.Total;
            Item = line.Item;
            SalesTransaction = line.SalesTransaction;
        }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string SalesTransactionID { get; set; }
         
        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        [Required]
        [Column(Order = 2)]
        public int Quantity { get; set; }

        [Required]
        [Column(Order = 3)]
        public decimal SalesPrice { get; set; }

        [Required]
        [Column(Order = 4)]
        public decimal Discount { get; set; }

        [Required]
        [Column(Order = 5)]
        public decimal Total { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("SalesTransactionID")]
        public virtual SalesTransaction SalesTransaction { get; set; }
    }
}
