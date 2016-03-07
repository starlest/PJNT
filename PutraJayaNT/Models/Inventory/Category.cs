using System.Collections.Generic;
using MVVMFramework;

namespace PutraJayaNT.Models.Inventory
{
    // TODO rename to Category
    public class Category : ObservableObject
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        public override bool Equals(object obj)
        {
            var category = obj as Category;
            if (category == null) return false;
            return ID.Equals(category.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}