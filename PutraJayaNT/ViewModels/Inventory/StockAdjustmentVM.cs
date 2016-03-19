namespace PutraJayaNT.ViewModels.Inventory
{
    using Item;
    using Utilities.ModelHelpers;
    using MVVMFramework;
    using Models.StockCorrection;
    using Utilities;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    internal class StockAdjustmentVM : ViewModelBase<StockAdjustmentTransaction>
    {
        #region Transaction Backing Fields
        private string _transactionID;
        private string _transactionDescription;
        private ICommand _newTransactionCommand;
        private ICommand _saveTransactionCommand;
        #endregion

        #region New Entry Backing Fields
        WarehouseVM _newEntryWarehouse;
        ItemVM _newEntryProduct;
        string _newEntryUnitName;
        int? _newEntryPiecesPerUnit;
        int? _newEntryUnits;
        int? _newEntryPieces;
        int _remainingStock;
        string _newEntryRemainingStock;
        ICommand _newEntryCommand;
        #endregion

        StockAdjustmentTransactionLineVM _selectedLine;
        ICommand _deleteLineCommand;

        bool _isNotEditMode;

        public StockAdjustmentVM()
        {
            Model = new StockAdjustmentTransaction
            {
                AdjustStockTransactionLines = new ObservableCollection<StockAdjustmentTransactionLine>()
            };

            Warehouses = new ObservableCollection<WarehouseVM>();
            Products = new ObservableCollection<ItemVM>();
            Lines = new ObservableCollection<StockAdjustmentTransactionLineVM>();

            _isNotEditMode = true;

            UpdateWarehouses();
            SetTransactionID();
        }

        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<ItemVM> Products { get; }

        public ObservableCollection<StockAdjustmentTransactionLineVM> Lines { get; }

        public bool IsNotEditMode
        {
            get { return _isNotEditMode; }
            set { SetProperty(ref _isNotEditMode, value, () => IsNotEditMode); }
        }

        #region Transaction Properties 
        public string TransactionID
        {
            get { return _transactionID; }
            set
            {
                if (!CheckIDExists(value))
                {
                    MessageBox.Show("Transaction does not exists.", "Invalid ID", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionID, value, () => TransactionID);
                SetEditMode();
            }
        }

        public string TransactionDescription
        {
            get { return _transactionDescription; }
            set { SetProperty(ref _transactionDescription, value, () => TransactionDescription); }
        }

        public ICommand NewTransactionCommand => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(() =>
                {
                    if (Lines.Count == 0 || !IsDescriptionFilled() || !IsSavingConfirmationYes() || !UtilityMethods.GetVerification()) return;
                    AssignTransactionPropertiesToModel();
                    StockAdjustmentHelper.AddStockAdjustmentTransactionToDatabase(Model);
                    MessageBox.Show("Transaction sucessfully is saved.", "Success", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }

        private bool IsDescriptionFilled()
        {
            if (_transactionDescription != null && !_transactionDescription.Equals("") && !_transactionDescription.Equals(" ")) return true;
            MessageBox.Show("Please enter a description.", "Missing Field", MessageBoxButton.OK);
            return false;
        }

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

        #region Line Properties
        public StockAdjustmentTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected() && !IsLineDeletionConfirmationYes()) return;
                    Lines.Remove(_selectedLine);
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateWarehouses()
        {
            Warehouses.Clear();
            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.OrderBy(warehouse => warehouse.Name);
                foreach (var warehouse in warehouses)
                    Warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void UpdateProducts()
        {
            Products.Clear();
            using (var context = new ERPContext())
            {
                var items = context.Inventory.OrderBy(item => item.Name);
                foreach (var item in items)
                    Products.Add(new ItemVM { Model = item });
            }
        }

        private static bool CheckIDExists(string id)
        {
            using (var context = new ERPContext())
            {
                var transaction = context.StockAdjustmentTransactions.SingleOrDefault(e => e.StockAdjustmentTransactionID.Equals(id));
                return transaction != null;
            }
        }

        private void SetEditMode()
        {
            IsNotEditMode = false;
            ResetEntryFields();
            Lines.Clear();

            using (var context = new ERPContext())
            {
                TransactionDescription =
                    context.StockAdjustmentTransactions.Single(
                        transaction => transaction.StockAdjustmentTransactionID.Equals(_transactionID))
                        .Description;

                var lines = context.StockAdjustmentTransactionLines
                    .Include("Item")
                    .Include("Warehouse")
                    .Where(line => line.StockAdjustmentTransactionID.Equals(_transactionID))
                    .OrderBy(line => line.Warehouse.Name)
                    .ThenBy(line => line.Item.Name);
                foreach (var line in lines)
                    Lines.Add(new StockAdjustmentTransactionLineVM { Model = line });
            }
        }

        private void SetTransactionID()
        {
            var month = UtilityMethods.GetCurrentDate().Month;
            var year = UtilityMethods.GetCurrentDate().Year;
            _transactionID = "SA" + (long)((year - 2000) * 100 + month) * 1000000;

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = from StockAdjustmentTransaction in context.StockAdjustmentTransactions
                    where string.Compare(StockAdjustmentTransaction.StockAdjustmentTransactionID, _transactionID, StringComparison.Ordinal) >= 0 && StockAdjustmentTransaction.StockAdjustmentTransactionID.Substring(0, 2).Equals("SA")
                    orderby StockAdjustmentTransaction.StockAdjustmentTransactionID descending
                    select StockAdjustmentTransaction.StockAdjustmentTransactionID;
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _transactionID = "SA" + (Convert.ToInt64(lastTransactionID.Substring(2)) + 1);

            Model.StockAdjustmentTransactionID = _transactionID;
            OnPropertyChanged("TransactionID");
        }

        private void ResetEntryFields()
        {
            NewEntryProduct = null;
            NewEntryUnitName = null;
            NewEntryPiecesPerUnit = null;
            NewEntryUnits = null;
            NewEntryPieces = null;
            NewEntryRemainingStock = null;
        }

        private void ResetTransaction()
        {
            IsNotEditMode = true;
            TransactionDescription = null;
            NewEntryWarehouse = null;
            ResetEntryFields();
            Lines.Clear();
            Model = new StockAdjustmentTransaction
            {
                AdjustStockTransactionLines = new ObservableCollection<StockAdjustmentTransactionLine>()
            };
            SetTransactionID();
            UpdateWarehouses();
        }
        #endregion

        #region Transaction Helper Methods
        private static bool IsSavingConfirmationYes()
        {
            return MessageBox.Show("Confirm transaction?", "Confirmation", MessageBoxButton.YesNo) ==
                   MessageBoxResult.Yes;
        }

        private void AssignTransactionPropertiesToModel()
        {
            Model.Description = _transactionDescription;
            Model.Date = UtilityMethods.GetCurrentDate().Date;
            foreach (var line in Lines)
                Model.AdjustStockTransactionLines.Add(line.Model);
        }
        #endregion

        #region New Entry Helper Methods
        private void SetRemainingStock()
        {
            NewEntryUnitName = _newEntryProduct.UnitName;
            NewEntryPiecesPerUnit = _newEntryProduct.PiecesPerUnit;
            _remainingStock = UtilityMethods.GetRemainingStock(_newEntryProduct.Model, _newEntryWarehouse.Model);
            foreach (var line in Lines)
            {
                if (!line.Warehouse.ID.Equals(_newEntryWarehouse.ID) || !line.Item.ItemID.Equals(_newEntryProduct.ID) || line.Quantity > 0)
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

            foreach (var line in Lines)
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
                    StockAdjustmentTransaction = Model,
                    Item = _newEntryProduct.Model,
                    Warehouse = _newEntryWarehouse.Model,
                    Quantity = newEntryQuantity
                },
            };

            Lines.Add(newEntry);
        }
        #endregion

        #region Line Helper Methods
        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private static bool IsLineDeletionConfirmationYes()
        {
            return
                MessageBox.Show("Confirm deletion?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes;
        }
        #endregion
    }
}
