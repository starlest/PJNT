namespace PutraJayaNT.ViewModels.Inventory
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.StockCorrection;
    using MVVMFramework;
    using Utilities;
    using System.Linq;

    internal class StockAdjustmentNewEntryVM : ViewModelBase
    {
        private readonly StockAdjustmentVM _parentVM;

        #region New Entry Backing Fields
        private WarehouseVM _newEntryWarehouse;
        private ItemVM _newEntryProduct;
        private string _newEntryUnitName;
        private int? _newEntryPiecesPerUnit;
        private int? _newEntryUnits;
        private int? _newEntryPieces;
        private int _remainingStock;
        private string _newEntryRemainingStock;
        private ICommand _newEntryCommand;
        #endregion

        public StockAdjustmentNewEntryVM(StockAdjustmentVM parentVM)
        {
            _parentVM = parentVM;
            Warehouses = new ObservableCollection<WarehouseVM>();
            Products = new ObservableCollection<ItemVM>();
            UpdateWarehouses();
        }

        #region Collections
        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<ItemVM> Products { get; }
        #endregion

        #region New Entry Properties
        public WarehouseVM NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set
            {
                SetProperty(ref _newEntryWarehouse, value, () => NewEntryWarehouse);
                if (_newEntryWarehouse == null) return;
                UpdateProducts();
            }
        }

        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, () => NewEntryProduct);
                if (_newEntryProduct == null) return;
                SetRemainingStock();
            }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, () => NewEntryUnitName); }
        }

        public int? NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, () => NewEntryPiecesPerUnit); }
        }

        public int? NewEntryUnits
        {
            get { return _newEntryUnits; }
            set { SetProperty(ref _newEntryUnits, value, () => NewEntryUnits); }
        }

        public int? NewEntryPieces
        {
            get { return _newEntryPieces; }
            set { SetProperty(ref _newEntryPieces, value, () => NewEntryPieces); }
        }

        public string NewEntryRemainingStock
        {
            get { return _newEntryRemainingStock; }
            set { SetProperty(ref _newEntryRemainingStock, value, () => NewEntryRemainingStock); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!AreAllEntryFieldsFilled() || !IsQuantityValid()) return;
                    AddNewEntryToTransaction();
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region New Entry Helper Methods

        public void UpdateWarehouses()
        {
            Warehouses.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var warehouses = context.Warehouses.OrderBy(warehouse => warehouse.Name);
                foreach (var warehouse in warehouses)
                    Warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void UpdateProducts()
        {
            Products.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var items = context.Inventory.OrderBy(item => item.Name);
                foreach (var item in items)
                    Products.Add(new ItemVM { Model = item });
            }
        }

        private void SetRemainingStock()
        {
            NewEntryUnitName = _newEntryProduct.UnitName;
            NewEntryPiecesPerUnit = _newEntryProduct.PiecesPerUnit;
            _remainingStock = UtilityMethods.GetRemainingStock(_newEntryProduct.Model, _newEntryWarehouse.Model);
            foreach (var line in _parentVM.DisplayedLines)
            {
                if (!line.Warehouse.ID.Equals(_newEntryWarehouse.ID) || 
                    !line.Item.ItemID.Equals(_newEntryProduct.ID) || line.Quantity > 0)
                    continue;
                _remainingStock -= -line.Quantity;
                break;
            }
            NewEntryRemainingStock = _remainingStock / _newEntryProduct.PiecesPerUnit + "/" + _remainingStock % _newEntryProduct.PiecesPerUnit;
        }

        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryWarehouse != null && _newEntryProduct != null &&
                (_newEntryUnits != null || _newEntryPieces != null)) return true;
            MessageBox.Show("Please enter all fields.", "Missing Field(s)", MessageBoxButton.OK);
            return false;
        }

        private bool IsQuantityValid()
        {

            var quantity = (_newEntryPieces ?? 0) + (_newEntryUnits ?? 0) * _newEntryProduct.PiecesPerUnit;
            if (quantity >= 0 || -quantity <= _remainingStock) return true;
            MessageBox.Show("There is not enough stock.", "Insufficient Stock", MessageBoxButton.OK);
            return false;
        }

        private void AddNewEntryToTransaction()
        {
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits ?? 0) * _newEntryProduct.PiecesPerUnit;

            foreach (var line in _parentVM.DisplayedLines)
            {
                if (!line.Item.ItemID.Equals(_newEntryProduct.ID) ||
                    !line.Warehouse.ID.Equals(_newEntryWarehouse.ID)) continue;
                line.Quantity += newEntryQuantity;
                ResetEntryFields();
                return;
            }

            var newEntry = new StockAdjustmentTransactionLineVM
            {
                Model = new StockAdjustmentTransactionLine
                {
                    StockAdjustmentTransaction = _parentVM.Model,
                    Item = _newEntryProduct.Model,
                    Warehouse = _newEntryWarehouse.Model,
                    Quantity = newEntryQuantity
                },
            };

            _parentVM.DisplayedLines.Add(newEntry);
        }


        public void ResetEntryFields()
        {
            NewEntryProduct = null;
            NewEntryUnitName = null;
            NewEntryPiecesPerUnit = null;
            NewEntryUnits = null;
            NewEntryPieces = null;
            NewEntryRemainingStock = null;
        }
        #endregion
    }
}
