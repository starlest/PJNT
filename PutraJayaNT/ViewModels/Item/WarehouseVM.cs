namespace ECERP.ViewModels.Item
{
    using Models.Inventory;
    using MVVMFramework;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class WarehouseVM : ViewModelBase<Warehouse>
    {
        private bool _isSelected;

        public int ID => Model.ID;

        public string Name => Model.Name;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, () => IsSelected); }
        }

        public override bool Equals(object obj)
        {
            var warehouse = obj as WarehouseVM;
            return warehouse != null && ID.Equals(warehouse.ID);
        }
    }
}
