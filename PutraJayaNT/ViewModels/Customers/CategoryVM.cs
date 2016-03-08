using MVVMFramework;
using PutraJayaNT.Models.Inventory;

namespace PutraJayaNT.ViewModels
{
    public class CategoryVM : ViewModelBase<Category>
    {
        public int ID => Model.ID;

        public string Name => Model.Name;

        public override bool Equals(object obj)
        {
            var category = obj as CategoryVM;
            if (category == null) return false;
            else return this.ID.Equals(category.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
