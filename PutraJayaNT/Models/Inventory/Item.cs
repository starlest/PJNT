using PutraJayaNT.Models.Sales;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Inventory
{
    [Table("Inventory")]
    public class Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Item()
        {
            Suppliers = new ObservableCollection<Supplier>();
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

        public override bool Equals(object obj)
        {
            var item = obj as Item;
            if (item == null) return false;
            else return this.ItemID.Equals(item.ItemID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual ObservableCollection<Stock> Stocks { get; set; }

        public virtual ICollection<SalesTransactionLine> SalesTransactionLines { get; set; }

        public virtual ICollection<SalesReturnTransactionLine> SalesReturnTransactionLines { get; set; }
    }
}
