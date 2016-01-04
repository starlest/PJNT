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
    }
}
