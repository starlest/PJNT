using MVVMFramework;
using PutraJayaNT.Models.Inventory;

namespace PutraJayaNT.ViewModels
{
    class WarehouseVM : ViewModelBase<Warehouse>
    {
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
    }
}
