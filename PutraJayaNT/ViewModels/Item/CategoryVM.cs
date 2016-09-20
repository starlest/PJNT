namespace ECRP.ViewModels.Item
{
    using Models.Inventory;
    using MVVMFramework;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class CategoryVM : ViewModelBase<Category>
    {
        public int ID => Model.ID;

        public string Name => Model.Name;

        public override bool Equals(object obj)
        {
            var category = obj as CategoryVM;
            return category != null && ID.Equals(category.ID);
        }
    }
}
