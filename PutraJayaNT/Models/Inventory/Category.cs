using System.Collections.Generic;
using MVVMFramework;

namespace PutraJayaNT.Models.Inventory
{
    // TODO rename to Category
    #pragma warning disable CS0659
    public class Category : ObservableObject
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        public override bool Equals(object obj)
        {
            var category = obj as Category;
            return category != null && ID.Equals(category.ID);
        }
    }
}