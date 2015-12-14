using PutraJayaNT.Models.Inventory;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    public class Supplier
    {
        public Supplier()
        {
            Active = true;
        }

        public int ID { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public int GSTID { get; set; }

        public decimal PurchaseReturnCredits { get; set; }

        public bool Active { get; set; }

        public virtual ObservableCollection<Item> Items { get; set; }

        public override string ToString() { return Name; }

        public override bool Equals(object obj)
        {
            var supplier = obj as Supplier;
            if (supplier == null) return false;
            else return this.ID.Equals(supplier.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
