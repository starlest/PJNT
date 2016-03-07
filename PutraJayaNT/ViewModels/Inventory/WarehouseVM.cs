using MVVMFramework;
using PutraJayaNT.Models.Inventory;

namespace PutraJayaNT.ViewModels.Inventory
{
    public class WarehouseVM : ViewModelBase<Warehouse>
    {
        bool _isSelected;

        public int ID
        {
            get { return Model.ID; }
        }

        public string Name
        {
            get { return Model.Name; }
        }

        public override bool Equals(object obj)
        {
            var warehouse = obj as WarehouseVM;

            if (warehouse == null) return false;
            else return this.ID.Equals(warehouse.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, "IsSelected"); }
        }
    }
}
