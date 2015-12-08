namespace PutraJayaNT.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Inventory")]
    public partial class Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Item()
        {
            TransactionLines = new HashSet<SalesTransactionLine>();
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

        public int Pieces { get; set; }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesTransactionLine> TransactionLines { get; set; }
    }
}
