namespace PutraJayaNT.ViewModels.Reports
{
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using System.Linq;

    public class InventoryReportLineVM : ViewModelBase
    {
        Item _item;
        int _quantity;

        public InventoryReportLineVM(Item item)
        {
            _item = item;
        }

        public Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value, "Item"); }
        }

        public decimal SalesPrice
        {
            get { return _item.SalesPrice * _item.PiecesPerUnit; }
        }

        public decimal PurchasePrice
        {
            get { return _item.PurchasePrice * _item.PiecesPerUnit; }
        }

        public string Unit
        {
            get { return _item.UnitName + "/" + _item.PiecesPerUnit; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                SetProperty(ref _quantity, value, "Quantity");
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("InventoryValue");
            }
        }

        public int Units
        {
            get { return _quantity / _item.PiecesPerUnit; }
        }

        public int Pieces
        {
            get { return _quantity % _item.PiecesPerUnit; }
        }

        public decimal InventoryValue
        {
            get { return Item.PurchasePrice * _quantity; }
        }

        public Supplier SelectedSupplier
        {
            get { return Item.Suppliers.FirstOrDefault(); }
        }
    }
}
