using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Inventory
{
    [Table("Inventory")]
    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Item()
        {
            Active = true;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column(Order = 0)]
        public string ItemID { get; set; }

        public virtual Category Category { get; set; }

        public string Name { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal SalesPrice { get; set; }

        public string UnitName { get; set; }

        public int PiecesPerUnit { get; set; }

        public decimal SalesExpense { get; set; }

        public virtual ObservableCollection<Supplier> Suppliers { get; set; }

        public bool Active { get; set; }

        public virtual ObservableCollection<AlternativeSalesPrice> AlternativeSalesPrices { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as Item;
            return item != null && ItemID.Equals(item.ItemID);
        }
    }
}
