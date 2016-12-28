namespace ECERP.ViewModels.Reports
{
    using System.Linq;
    using Models.Inventory;
    using Models.Supplier;
    using MVVMFramework;
    using Utilities.ModelHelpers;

    public class InventoryReportLineVM : ViewModelBase
    {
        private Item _item;
        private int _quantity;

        public InventoryReportLineVM(Item item)
        {
            _item = item;
        }

        public Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value, "Item"); }
        }

        public decimal SalesPrice => _item.SalesPrice * _item.PiecesPerUnit;

        public decimal PurchasePrice => _item.PurchasePrice * _item.PiecesPerUnit;

        public string Unit => _item.PiecesPerSecondaryUnit == 0
            ? _item.UnitName + "/" + _item.PiecesPerUnit
            : _item.UnitName + "/" + _item.PiecesPerUnit / _item.PiecesPerSecondaryUnit;

        public string SecondaryUnit => _item.PiecesPerSecondaryUnit == 0
            ? null
            : _item.SecondaryUnitName + "/" + _item.PiecesPerSecondaryUnit;


        public string UnitName => _item.PiecesPerSecondaryUnit == 0
            ? _item.UnitName
            : _item.UnitName + "/" + _item.SecondaryUnitName;

        public string QuantityPerUnit => InventoryHelper.GetItemQuantityPerUnit(_item);

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                SetProperty(ref _quantity, value, "Quantity");
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("SecondaryUnits");
                OnPropertyChanged("InventoryValue");
            }
        }

        public int Units => _quantity / _item.PiecesPerUnit;

        public int? SecondaryUnits => _item.PiecesPerSecondaryUnit == 0
            ? (int?) null
            : _quantity % _item.PiecesPerUnit / _item.PiecesPerSecondaryUnit;

        public int Pieces => _item.PiecesPerSecondaryUnit == 0
            ? _quantity % Item.PiecesPerUnit
            : _quantity % _item.PiecesPerUnit % _item.PiecesPerSecondaryUnit;

        public decimal InventoryValue => _item.PurchasePrice * _quantity;

        public Supplier SelectedSupplier => _item.Suppliers.FirstOrDefault();
    }
}