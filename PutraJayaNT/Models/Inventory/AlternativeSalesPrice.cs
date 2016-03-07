using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Inventory
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
            return Name + "/" + $"{SalesPrice:N2}";
        }

        public override bool Equals(object obj)
        {
            var altSalesPrice = obj as AlternativeSalesPrice;
            if (altSalesPrice == null) return false;
            return ItemID.Equals(altSalesPrice.ItemID) && Name.Equals(altSalesPrice.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
