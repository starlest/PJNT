namespace PutraJayaNT.ViewModels.Suppliers.Purchase
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.Purchase;
    using MVVMFramework;
    using ViewModels.Purchase;

    internal class PurchaseNewEntryVM : ViewModelBase
    {
        private readonly PurchaseVM _parentVM;

        #region Backing Fields
        private WarehouseVM _newEntryWarehouse;
        private ItemVM _newEntryItem;
        private string _newEntryUnitName;
        private int? _newEntryUnits;
        private int? _newEntryPieces;
        private int? _newEntryPiecesPerUnit;
        private decimal _newEntryPrice;
        private decimal? _newEntryDiscountPercent;
        private decimal _newEntryDiscount;
        private bool _newEntrySubmitted;
        private ICommand _newEntryCommand;
        #endregion

        public PurchaseNewEntryVM(PurchaseVM parentVM)
        {
            _parentVM = parentVM;
        }

        public ObservableCollection<WarehouseVM> Warehouses => _parentVM.Warehouses;

        public ObservableCollection<ItemVM> SupplierItems => _parentVM.SupplierItems;
          
        #region New Entry Properties
        public WarehouseVM NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set { SetProperty(ref _newEntryWarehouse, value, () => NewEntryWarehouse); }
        }

        public ItemVM NewEntryItem
        {
            get { return _newEntryItem; }
            set
            {
                SetProperty(ref _newEntryItem, value, () => NewEntryItem);
                if (_newEntryItem == null) return;
                NewEntryUnitName = _newEntryItem.UnitName;
                NewEntryPrice = _newEntryItem.PurchasePrice;
                NewEntryPiecesPerUnit = _newEntryItem.PiecesPerUnit;
            }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, () => NewEntryUnitName); }
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

        public int? NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, () => NewEntryPiecesPerUnit); }
        }

        public decimal? NewEntryDiscountPercent
        {
            get { return _newEntryDiscountPercent; }
            set
            {
                if (value != null && (value < 0 || value > 100))
                {
                    MessageBox.Show("Please enter a value from the range of 0 - 100.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newEntryDiscountPercent, value, () => NewEntryDiscountPercent);
                if (_newEntryDiscountPercent == null) return;
                NewEntryDiscount = (decimal) _newEntryDiscountPercent / 100 * _newEntryPrice;
                NewEntryDiscountPercent = null;
            }
        }

        public decimal NewEntryDiscount
        {
            get { return _newEntryDiscount; }
            set
            {
                if (value < 0 || value > _newEntryPrice)
                {
                    MessageBox.Show($"Please enter a value from the range of 0 - {_newEntryPrice}.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _newEntryDiscount, value, () => NewEntryDiscount);
            }
        }

        public decimal NewEntryPrice
        {
            get { return _newEntryPrice; }
            set { SetProperty(ref _newEntryPrice, value, () => NewEntryPrice); }
        }

        public bool NewEntrySubmitted
        {
            get { return _newEntrySubmitted; }
            set { SetProperty(ref _newEntrySubmitted, value, () => NewEntrySubmitted); }
        }
        #endregion

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!AreAllEntryFieldsFilled() || !AreAllEntryFieldsValid()) return;
                    AddNewEntryToTransaction();
                    _parentVM.UpdateUIGrossTotal();
                    ResetEntryFields();
                    TriggerNewEntrySubmitted();
                }));
            }
        }

        #region Helper Methods
        private void AddNewEntryToTransaction()
        {
            var newEntryQuantity = (_newEntryUnits ?? 0) * _newEntryItem.PiecesPerUnit + (_newEntryPieces ?? 0);

            foreach (var line in _parentVM.DisplayedLines)
            {
                if (!_newEntryItem.ID.Equals(line.Item.ItemID) || !_newEntryWarehouse.ID.Equals(line.Warehouse.ID) ||
                    !_newEntryPrice.Equals(line.PurchasePrice) || !_newEntryDiscount.Equals(line.Discount)) continue;
                line.Quantity += newEntryQuantity;
                ResetEntryFields();
                return;
            }

            var newEntryLine = new PurchaseTransactionLine
            {
                Quantity = newEntryQuantity,
                PurchasePrice = _newEntryPrice / _newEntryItem.PiecesPerUnit,
                Discount = _newEntryDiscount / _newEntryItem.PiecesPerUnit,
                Item = _newEntryItem.Model,
                Warehouse = _newEntryWarehouse.Model,
                SoldOrReturned = 0,
                PurchaseTransaction = _parentVM.Model,
                Total = newEntryQuantity * (_newEntryPrice/_newEntryItem.PiecesPerUnit - _newEntryDiscount/_newEntryItem.PiecesPerUnit)
            };

            var lineVM = new PurchaseTransactionLineVM { Model = newEntryLine };
            _parentVM.DisplayedLines.Add(lineVM);
        }

        public void ResetEntryFields()
        {
            NewEntryItem = null;
            NewEntryPieces = null;
            NewEntryUnits = null;
            NewEntryPrice = 0;
            NewEntryDiscount = 0;
            NewEntryUnitName = null;
            NewEntryPiecesPerUnit = null;
        }

        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryItem != null && _newEntryWarehouse != null &&
                (_newEntryUnits != null || _newEntryPieces != null)) return true;
            MessageBox.Show("Please enter all the required fields", "Missing field(s)", MessageBoxButton.OK);
            return false;
        }

        private bool AreAllEntryFieldsValid()
        {
            if (_newEntryPrice >= 0 || _newEntryDiscount >= 0 && _newEntryDiscount <= _newEntryPrice) return true;
            MessageBox.Show("Please check that all fields are valid.", "Invalid Field(s)", MessageBoxButton.OK);
            return false;
        }

        private void TriggerNewEntrySubmitted()
        {
            NewEntrySubmitted = true;
            NewEntrySubmitted = false;
        }
        #endregion
    }
}
