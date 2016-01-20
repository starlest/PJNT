using MVVMFramework;
using System.Collections.Generic;

namespace PutraJayaNT.Models.Inventory
{
    public class Category : ObservableObject
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        public override bool Equals(object obj)
        {
            var category = obj as Category;
            if (category == null) return false;
            else return this.ID.Equals(category.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
