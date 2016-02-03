using PutraJayaNT.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Sales
{
    public class AlternativeSalesPrice
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ItemID { get; set; }

        public decimal SalesPrice { get; set; }

        public virtual Item Item { get; set; }

        public override string ToString()
        {
            return Name + "/" + string.Format("{0:N2}", SalesPrice);
        }
    }
}
