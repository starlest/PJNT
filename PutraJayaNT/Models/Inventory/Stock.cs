namespace PutraJayaNT.Models.Inventory
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Stock
    {
        public Stock()
        {
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 1)]
        public string ItemID { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 2)]
        public int WarehouseID { get; set; }

        public int Pieces { get; set; }

        [ForeignKey("ItemID")]
        public virtual Item Item { get; set; }

        [ForeignKey("WarehouseID")]
        public virtual Warehouse Warehouse { get; set; }
    }
}
