namespace PutraJayaNT.ViewModels.Inventory
{
    using MVVMFramework;
    using Models.Inventory;
    using Models.StockCorrection;
    using PutraJayaNT.Reports.Windows;
    using Utilities;
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Utilities.ModelHelpers;

    public class StockMovementVM : ViewModelBase<StockMovementTransaction>
    {
        private bool _isNotEditMode;
        private bool _isFromWarehouseNotSelected;
        private bool _isToWarehouseNotSelected;

        #region New Entry Backing Fields
        private ItemVM _newEntryProduct;
        private WarehouseVM _transactionFromWarehouse;
        private WarehouseVM _transactionToWarehouse;
        private string _newEntryRemainingStock;
        private int? _newEntryUnits;
        private int? _newEntryPieces;
        private ICommand _newEntrySubmitCommand;
        #endregion

        #region Transaction Backing Fields
        private string _transactionID;
        private DateTime _transactionDate;
        private ICommand _newTransactionCommand;
        private ICommand _saveTransactionCommand;
        #endregion

        private StockMovementTransactionLineVM _selectedLine;
        private ICommand _deleteLineCommand;
        private ICommand _printCommand;

        public StockMovementVM()
        {
            Model = new StockMovementTransaction();
            Model.StockMovementTransactionLines = new ObservableCollection<StockMovementTransactionLine>();

            ListedProducts = new ObservableCollection<ItemVM>();
            Warehouses = new ObservableCollection<WarehouseVM>();
            StockMovementTransactionLines = new ObservableCollection<StockMovementTransactionLineVM>();

            _transactionDate = UtilityMethods.GetCurrentDate().Date;

            _isNotEditMode = true;
            _isFromWarehouseNotSelected = true;
            _isToWarehouseNotSelected = true;

            UpdateWarehouses();
            SetTransactionID();
        }

        #region Collections
        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<ItemVM> ListedProducts { get; }

        public ObservableCollection<StockMovementTransactionLineVM> StockMovementTransactionLines { get; }
        #endregion

        public bool IsNotEditMode
        {
            get { return _isNotEditMode; }
            set { SetProperty(ref _isNotEditMode, value, () => IsNotEditMode); }
        }

        public bool IsFromWarehouseNotSelected
        {
            get { return _isFromWarehouseNotSelected; }
            set { SetProperty(ref _isFromWarehouseNotSelected, value, () => IsFromWarehouseNotSelected); }
        }

        public bool IsToWarehouseNotSelected
        {
            get { return _isToWarehouseNotSelected; }
            set { SetProperty(ref _isToWarehouseNotSelected, value, () => IsToWarehouseNotSelected); }
        }

        #region New Entry Properties
        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, () => NewEntryProduct);
                if (_newEntryProduct != null) SetRemainingStock();
            }
        }

        public string NewEntryRemainingStock
        {
            get { return _newEntryRemainingStock; }
            set { SetProperty(ref _newEntryRemainingStock, value, () => NewEntryRemainingStock); }
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

        public ICommand NewEntrySubmitCommand
        {
            get
            {
                return _newEntrySubmitCommand ?? (_newEntrySubmitCommand = new RelayCommand(() =>
                {
                    if (!AreAllEntryFieldsFilled() || !IsQuantityValid() || !IsThereEnoughStock()) return;
                    AddEntryToTransaction();
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region Line Properties
        public StockMovementTransactionLineVM SelectedLine
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
                    if (!IsThereLineSelected() || !IsDeletionConfirmationYes()) return;
                    StockMovementTransactionLines.Remove(_selectedLine);
                }));
            }
        }
        #endregion

        #region New Transaction Properties
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

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set
            {
                if (value > UtilityMethods.GetCurrentDate().Date)
                {
                    MessageBox.Show("Cannot set to a future date.", "Invalid Date", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _transactionDate, value, () => TransactionDate);
            }
        }

        public WarehouseVM TransactionFromWarehouse
        {
            get { return _transactionFromWarehouse; }
            set
            {
                SetProperty(ref _transactionFromWarehouse, value, () => TransactionFromWarehouse);
                if (_transactionFromWarehouse == null) return;
                IsFromWarehouseNotSelected = false;
                UpdateListedProducts();
            }
        }

        public WarehouseVM TransactionToWarehouse
        {
            get { return _transactionToWarehouse; }
            set
            {
                if (IsNotEditMode && _transactionFromWarehouse != null && _transactionFromWarehouse.ID.Equals(value.ID))
                {
                    MessageBox.Show("Cannot move between the same warehouses.", "Invalid Command", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionToWarehouse, value, () => TransactionToWarehouse);
                if (_transactionToWarehouse == null) return;
                IsToWarehouseNotSelected = false;
            }
        }

        public ICommand NewTransactionCommand => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(() =>
                {
                    if (StockMovementTransactionLines.Count == 0 || !IsToWarehouseSelected() || !IsTransactionConfirmationYes() || !AreThereEnoughStock() || !UtilityMethods.GetVerification()) return;
                    SetTransactionID();
                    AssignSelectedPropertiesToModel();
                    StockMovementTransactionHelper.AddStockMovementTransactionToDatabase(Model);
                    MessageBox.Show("Stock movement Transaction is sucessfully added", "Success", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }
        #endregion

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (StockMovementTransactionLines.Count == 0) return;
                    ShowPrintWindow();
                }));
            }
        }

        #region Helper Methods
        private void UpdateListedProducts()
        {
            ListedProducts.Clear();
            using (var context = new ERPContext())
            {
                var stocksFromDatabase = context.Stocks.Where(
                    stock => stock.WarehouseID.Equals(_transactionFromWarehouse.ID))
                    .Include("Item")
                    .Include("Warehouse")
                    .OrderBy(stock => stock.Item.Name);

                foreach (var stock in stocksFromDatabase)
                    ListedProducts.Add(new ItemVM { Model = stock.Item });
            }
        }

        private void UpdateWarehouses()
        {
            Warehouses.Clear();

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();
                foreach (var warehouse in warehouses)
                    Warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private static bool CheckIDExists(string id)
        {
            using (var context = new ERPContext())
            {
                var transaction = context.StockMovementTransactions.SingleOrDefault(e => e.StockMovementTransactionID.Equals(id));
                return transaction != null;
            }
        }

        private void SetTransactionID()
        {
            var month = UtilityMethods.GetCurrentDate().Month;
            var year = UtilityMethods.GetCurrentDate().Year;
            _transactionID = "MS" + (long)((year - 2000) * 100 + month) * 1000000;

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = from StockMovementTransaction in context.StockMovementTransactions
                          where string.Compare(StockMovementTransaction.StockMovementTransactionID, _transactionID, StringComparison.Ordinal) >= 0
                          orderby StockMovementTransaction.StockMovementTransactionID descending
                          select StockMovementTransaction.StockMovementTransactionID;
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _transactionID = "MS" + (Convert.ToInt64(lastTransactionID.Substring(2)) + 1);
            Model.StockMovementTransactionID = _transactionID;
            OnPropertyChanged("TransactionID");
        }

        private void ResetEntryFields()
        {
            NewEntryProduct = null;
            NewEntryRemainingStock = null;
            NewEntryUnits = null;
            NewEntryPieces = null;
        }

        private void ResetTransaction()
        {
            ResetEntryFields();
            StockMovementTransactionLines.Clear();
            ListedProducts.Clear();
            Model = new StockMovementTransaction
            {
                StockMovementTransactionLines = new ObservableCollection<StockMovementTransactionLine>()
            };
            SetTransactionID();
            IsNotEditMode = true;
            TransactionFromWarehouse = null;
            TransactionToWarehouse = null;
            TransactionDate = UtilityMethods.GetCurrentDate().Date;
            IsFromWarehouseNotSelected = true;
            IsToWarehouseNotSelected = true;
            UpdateWarehouses();
        }

        private void SetEditMode()
        {
            IsNotEditMode = false;
            ResetEntryFields();

            using (var context = new ERPContext())
            {
                var stockMovementTransactionFromDatabase = context.StockMovementTransactions
                    .Include("StockMovementTransactionLines")
                    .Include("StockMovementTransactionLines.Item")
                    .Include("FromWarehouse")
                    .Include("ToWarehouse")
                    .Single(transaction => transaction.StockMovementTransactionID.Equals(_transactionID));

                TransactionFromWarehouse = new WarehouseVM { Model = stockMovementTransactionFromDatabase.FromWarehouse };
                TransactionToWarehouse = new WarehouseVM { Model = stockMovementTransactionFromDatabase.ToWarehouse };
                TransactionDate = stockMovementTransactionFromDatabase.Date;

                StockMovementTransactionLines.Clear();
                var lines = stockMovementTransactionFromDatabase.StockMovementTransactionLines.ToList();
                foreach (var line in lines)
                    StockMovementTransactionLines.Add(new StockMovementTransactionLineVM { Model = line });
            }
        }

        private int GetRemainingStock(Item item, Warehouse warehouse)
        {
            using (var context = new ERPContext())
            {
                var stock = context.Stocks.SingleOrDefault(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));
                var availableQuantity = stock?.Pieces ?? 0;
                availableQuantity = StockMovementTransactionLines.Where(line => line.Item.ItemID.Equals(item.ItemID)).Aggregate(availableQuantity, (current, line) => current - line.Quantity);
                return availableQuantity;
            }
        }

        private static int GetRemainingStockInDatabase(Item item, Warehouse warehouse)
        {
            using (var context = new ERPContext())
            {
                var stock = context.Stocks.SingleOrDefault(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));
                return stock?.Pieces ?? 0;
            }
        }

        private void SetRemainingStock()
        {
            var remainingStock = GetRemainingStock(_newEntryProduct.Model, _transactionFromWarehouse.Model);
            NewEntryRemainingStock = remainingStock / _newEntryProduct.PiecesPerUnit + "/" + remainingStock % _newEntryProduct.PiecesPerUnit + " " + _newEntryProduct.UnitName;
        }

        private void ShowPrintWindow()
        {
            var stockMovementReportWindow = new StockMovementReportWindow(this)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            stockMovementReportWindow.Show();
        }
        #endregion

        #region New Entry Helper Methods
        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryProduct != null && _transactionFromWarehouse != null && _transactionToWarehouse != null &&
                (_newEntryUnits != null || _newEntryPieces != null))
                return true;
            MessageBox.Show("Please enter the missing field(s).", "Missing Field(s)", MessageBoxButton.OK);
            return false;
        }

        private bool IsQuantityValid()
        {
            var units = _newEntryUnits ?? 0;
            var pieces = _newEntryPieces ?? 0;
            if (units >= 0 && pieces >= 0) return true;
            MessageBox.Show("Please enter a value higher than 0.", "Invalid Quantity", MessageBoxButton.OK);
            return false;
        }

        private bool IsThereEnoughStock()
        {
            var units = _newEntryUnits ?? 0;
            var pieces = _newEntryPieces ?? 0;
            var quantity = units * _newEntryProduct.PiecesPerUnit + pieces;
            if (GetRemainingStock(_newEntryProduct.Model, _transactionFromWarehouse.Model) > 0 &&
                quantity <= GetRemainingStock(_newEntryProduct.Model, _transactionFromWarehouse.Model))
                return true;
            MessageBox.Show("Not enough stock to be moved.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private void AddEntryToTransaction()
        {
            var newEntryUnits = _newEntryUnits ?? 0;
            var newEntryPieces = _newEntryPieces ?? 0;
            var newEntryQuantity = newEntryUnits * _newEntryProduct.PiecesPerUnit + newEntryPieces;

            var isThereAnExistingLine = false;
            foreach (var line in StockMovementTransactionLines.Where(line => _newEntryProduct.ID.Equals(line.Item.ItemID)))
            {
                line.Quantity += newEntryQuantity;
                isThereAnExistingLine = true;
                break;
            }

            if (!isThereAnExistingLine)
            {
                var newEntry = new StockMovementTransactionLine
                {
                    StockMovementTransaction = Model,
                    Item = _newEntryProduct.Model,
                    Quantity = newEntryQuantity
                };
                StockMovementTransactionLines.Add(new StockMovementTransactionLineVM { Model = newEntry });
            }
        }
        #endregion

        #region Delete Line Helper Methods
        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private static bool IsDeletionConfirmationYes()
        {
            return MessageBox.Show("Confirm deletion?", "Confirmation", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
        #endregion

        #region Save Transaction Helper Methods
        private static bool IsTransactionConfirmationYes()
        {
            return MessageBox.Show("Confirm movement?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private bool IsToWarehouseSelected()
        {
            if (_transactionToWarehouse != null) return true;
            MessageBox.Show("Please select a destination warehouse.", "Missing Selection", MessageBoxButton.OK);
            return false;
        }

        private bool AreThereEnoughStock()
        {
            foreach (var line in StockMovementTransactionLines)
            {
                var availableQuantity = GetRemainingStockInDatabase(line.Item, _transactionFromWarehouse.Model);
                if (availableQuantity != 0 && line.Quantity <= availableQuantity) continue;
                MessageBox.Show("Not enough stock to be moved.", "Invalid Command", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void AssignSelectedPropertiesToModel()
        {
            Model.Date = _transactionDate;
            Model.FromWarehouse = _transactionFromWarehouse.Model;
            Model.ToWarehouse = _transactionToWarehouse.Model;
            Model.StockMovementTransactionLines = new ObservableCollection<StockMovementTransactionLine>();
            foreach (var line in StockMovementTransactionLines)
                Model.StockMovementTransactionLines.Add(line.Model);
        }
        #endregion
    }
}
