namespace PutraJayaNT.ViewModels.Inventory
{
    using MVVMFramework;
    using Models;
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

    public class MoveStockVM : ViewModelBase<MoveStockTransaction>
    {
        ObservableCollection<ItemVM> _products;
        ObservableCollection<WarehouseVM> _warehouses;
        ObservableCollection<MoveStockTransactionLineVM> _lines;

        bool _isNotEditMode;
        bool _isFromWarehouseNotSelected;
        bool _isToWarehouseNotSelected;

        ItemVM _newEntryProduct;
        WarehouseVM _newTransactionFromWarehouse;
        WarehouseVM _newTransactionToWarehouse;
        string _newEntryRemainingStock;
        int? _newEntryUnits;
        int? _newEntryPieces;
        ICommand _newEntrySubmitCommand;

        string _newTransactionID;
        DateTime _newTransactionDate;
        ICommand _newTransactionCommand;
        ICommand _saveTransactionCommand;

        MoveStockTransactionLineVM _selectedLine;
        ICommand _deleteLineCommand;

        ICommand _printCommand;

        public MoveStockVM()
        {
            Model = new MoveStockTransaction();
            Model.MoveStockTransactionLines = new ObservableCollection<MoveStockTransactionLine>();

            _products = new ObservableCollection<ItemVM>();
            _warehouses = new ObservableCollection<WarehouseVM>();
            _lines = new ObservableCollection<MoveStockTransactionLineVM>();

            _newTransactionDate = UtilityMethods.GetCurrentDate().Date;

            _isNotEditMode = true;
            _isFromWarehouseNotSelected = true;
            _isToWarehouseNotSelected = true;

            UpdateWarehouses();
            SetTransactionID();
        }

        #region Collections
        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
        }

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }

        public ObservableCollection<MoveStockTransactionLineVM> Lines
        {
            get { return _lines; }
        }
        #endregion

        public bool IsNotEditMode
        {
            get { return _isNotEditMode; }
            set { SetProperty(ref _isNotEditMode, value, "IsNotEditMode"); }
        }

        public bool IsFromWarehouseNotSelected
        {
            get { return _isFromWarehouseNotSelected; }
            set { SetProperty(ref _isFromWarehouseNotSelected, value, "IsFromWarehouseNotSelected"); }
        }

        public bool IsToWarehouseNotSelected
        {
            get { return _isToWarehouseNotSelected; }
            set { SetProperty(ref _isToWarehouseNotSelected, value, "IsToWarehouseNotSelected"); }
        }

        #region New Entry Properties
        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, "NewEntryProduct");

                if (_newEntryProduct != null) SetRemainingStock();
            }
        }

        public string NewEntryRemainingStock
        {
            get { return _newEntryRemainingStock; }
            set { SetProperty(ref _newEntryRemainingStock, value, "NewEntryRemainingStock"); }
        }

        public int? NewEntryUnits
        {
            get { return _newEntryUnits; }
            set { SetProperty(ref _newEntryUnits, value, "NewEntryUnits"); }
        }

        public int? NewEntryPieces
        {
            get { return _newEntryPieces; }
            set { SetProperty(ref _newEntryPieces, value, "NewEntryPieces"); }
        }

        public ICommand NewEntrySubmitCommand
        {
            get
            {
                return _newEntrySubmitCommand ?? (_newEntrySubmitCommand = new RelayCommand(() =>
                {
                    if (_newEntryProduct == null || _newTransactionFromWarehouse == null || _newTransactionToWarehouse == null || (_newEntryUnits == null && _newEntryPieces == null))
                    {
                        MessageBox.Show("Please enter the missing field(s).", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    if (_newTransactionFromWarehouse.Equals(_newTransactionToWarehouse))
                    {
                        MessageBox.Show("Cannot move between the same warehouses.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    var units = _newEntryUnits == null ? 0 : (int)_newEntryUnits;
                    var pieces = _newEntryPieces == null ? 0 : (int)_newEntryPieces;
                    var quantity = (units * _newEntryProduct.PiecesPerUnit) + pieces;

                    if (units < 0 || pieces < 0)
                    {
                        MessageBox.Show("Please enter a value higher than 0.", "Invalid Quantity", MessageBoxButton.OK);
                        return;
                    }

                    if (GetRemainingStock(_newEntryProduct.Model, _newTransactionFromWarehouse.Model) == 0 || quantity > GetRemainingStock(_newEntryProduct.Model, _newTransactionFromWarehouse.Model))
                    {
                        MessageBox.Show("Not enough stock to be moved.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    foreach (var line in _lines)
                    {
                        if (_newEntryProduct.ID.Equals(line.Item.ItemID))
                            line.Quantity += quantity;
                    }

                    var newEntry = new MoveStockTransactionLine
                    {
                        MoveStockTransaction = this.Model,
                        Item = _newEntryProduct.Model,
                        Quantity = quantity
                    };

                    _lines.Add(new MoveStockTransactionLineVM { Model = newEntry });
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region New Transaction Properties
        public string NewTransactionID
        {
            get { return _newTransactionID; }
            set
            {
                if (!CheckIDExists(value))
                {
                    MessageBox.Show("Transaction does not exists.", "Invalid ID", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionID, value, "NewTransactionID");

                SetEditMode();
            }
        }

        public DateTime NewTransactionDate
        {
            get { return _newTransactionDate; }
            set
            {
                if (value > UtilityMethods.GetCurrentDate().Date)
                {
                    MessageBox.Show("Cannot set to a future date.", "Invalid Date", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionDate, value, "NewTransactionDate");
            }
        }

        public WarehouseVM NewTransactionFromWarehouse
        {
            get { return _newTransactionFromWarehouse; }
            set
            {
                SetProperty(ref _newTransactionFromWarehouse, value, "NewTransactionFromWarehouse");

                if (_newTransactionFromWarehouse == null) return;
                IsFromWarehouseNotSelected = false;
                UpdateProducts();
            }
        }

        public WarehouseVM NewTransactionToWarehouse
        {
            get { return _newTransactionToWarehouse; }
            set
            {
                SetProperty(ref _newTransactionToWarehouse, value, "NewTransactionToWarehouse");

                if (_newTransactionToWarehouse == null) return;
                IsToWarehouseNotSelected = false;
            }
        }

        public ICommand NewTransactionCommand
        {
            get
            {
                return _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(() =>
                {
                    ResetTransaction();
                }));
            }
        }

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(() =>
                {
                    if (_newTransactionToWarehouse == null)
                    {
                        MessageBox.Show("Please select a destination warehouse.", "Missing Selection", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm movement?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

                    // Verification
                    if (!UtilityMethods.GetVerification()) return;

                    using (var context = new ERPContext())
                    {
                        var fromWarehouse = context.Warehouses.Where(e => e.ID.Equals(_newTransactionFromWarehouse.ID)).FirstOrDefault();
                        var toWarehouse = context.Warehouses.Where(e => e.ID.Equals(_newTransactionToWarehouse.ID)).FirstOrDefault();

                        foreach (var line in _lines)
                        {
                            if (GetRemainingStock(line.Item, fromWarehouse) == 0 || line.Quantity > GetRemainingStock(line.Item, fromWarehouse))
                            {
                                MessageBox.Show("Not enough stock to be moved.", "Invalid Command", MessageBoxButton.OK);
                                return;
                            }

                            var item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();

                            line.Item = item;

                            Model.MoveStockTransactionLines.Add(line.Model);

                            // Adjust the stock for the affected warehouses
                            var fromStock = context.Stocks.Where(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(fromWarehouse.ID)).FirstOrDefault();
                            var toStock = context.Stocks.Where(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(toWarehouse.ID)).FirstOrDefault();
 
                            fromStock.Pieces -= line.Quantity;
                            if (fromStock.Pieces == 0) context.Stocks.Remove(fromStock);

                            if (toStock == null)
                            {
                                var newStock = new Stock
                                {
                                    Item = item,
                                    Warehouse = toWarehouse,
                                    Pieces = line.Quantity
                                };

                                context.Stocks.Add(newStock);
                            }
                            else toStock.Pieces += line.Quantity;
                        }

                        Model.Date = _newTransactionDate;
                        Model.FromWarehouse = fromWarehouse;
                        Model.ToWarehouse = toWarehouse;
                        var user = App.Current.TryFindResource("CurrentUser") as User;
                        var _user = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                        Model.User = _user;
                        context.MoveStockTransactions.Add(Model);
                        context.SaveChanges();
                    }

                    MessageBox.Show("Transaction is saved.", "Success", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }
        #endregion

        #region Line Properties
        public MoveStockTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (_selectedLine == null)
                    {
                        MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm deletion?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        _lines.Remove(_selectedLine);
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
                    if (_lines.Count == 0) return;

                    var stockMovementReportWindow = new StockMovementReportWindow(this);
                    stockMovementReportWindow.Owner = App.Current.MainWindow;
                    stockMovementReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    stockMovementReportWindow.Show();
                }));
            }
        }

        #region Helper Methods
        private void UpdateProducts()
        {
            _products.Clear();

            using (var context = new ERPContext())
            {
                var stocks = context.Stocks.Where(e => e.WarehouseID.Equals(_newTransactionFromWarehouse.ID))
                .Include("Item")
                .Include("Warehouse")
                .ToList();

                foreach (var s in stocks)
                    _products.Add(new ItemVM { Model = s.Item });
            }
        }

        private void UpdateWarehouses()
        {
           _warehouses.Clear();

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();
                foreach (var warehouse in warehouses)
                    _warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private bool CheckIDExists(string id)
        {
            using (var context = new ERPContext())
            {
                var transaction = context.MoveStockTransactions.Where(e => e.MoveStrockTransactionID.Equals(id)).FirstOrDefault();
                return transaction != null;
            }
        }

        private void SetTransactionID()
        {
            var month = UtilityMethods.GetCurrentDate().Month;
            var year = UtilityMethods.GetCurrentDate().Year;
            _newTransactionID = "MS" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from MoveStockTransaction in context.MoveStockTransactions
                           where MoveStockTransaction.MoveStrockTransactionID.CompareTo(_newTransactionID) >= 0
                           orderby MoveStockTransaction.MoveStrockTransactionID descending
                           select MoveStockTransaction.MoveStrockTransactionID);
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _newTransactionID = "MS" + (Convert.ToInt64(lastTransactionID.Substring(2)) + 1).ToString();

            Model.MoveStrockTransactionID = _newTransactionID;
            OnPropertyChanged("NewTransactionID");
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
            _lines.Clear();
            _products.Clear();
            Model = new MoveStockTransaction();
            Model.MoveStockTransactionLines = new ObservableCollection<MoveStockTransactionLine>();
            SetTransactionID();
            IsNotEditMode = true;
            NewTransactionFromWarehouse = null;
            NewTransactionToWarehouse = null;
            NewTransactionDate = UtilityMethods.GetCurrentDate().Date;
            IsFromWarehouseNotSelected = true;
            IsToWarehouseNotSelected = true;
        }

        private void SetEditMode()
        {
            IsNotEditMode = false;

            ResetEntryFields();
            _lines.Clear();

            using (var context = new ERPContext())
            {
                var transaction = context.MoveStockTransactions
                    .Include("MoveStockTransactionLines")
                    .Include("MoveStockTransactionLines.Item")
                    .Include("FromWarehouse")
                    .Include("ToWarehouse")
                    .Where(e => e.MoveStrockTransactionID.Equals(_newTransactionID))
                    .FirstOrDefault();

                NewTransactionFromWarehouse = new WarehouseVM { Model = transaction.FromWarehouse };
                NewTransactionToWarehouse = new WarehouseVM { Model = transaction.ToWarehouse };
                NewTransactionDate = transaction.Date;
                var lines = transaction.MoveStockTransactionLines.ToList();

                foreach (var line in lines)
                    _lines.Add(new MoveStockTransactionLineVM { Model = line });
            }
        }

        private int GetRemainingStock(Item item, Warehouse warehouse)
        {
            using (var context = new ERPContext())
            {
                var stock = context.Stocks.Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID)).FirstOrDefault();

                if (stock == null) return 0;
                else return stock.Pieces;
            }
        }

        private void SetRemainingStock()
        {
            var remainingStock = GetRemainingStock(_newEntryProduct.Model, _newTransactionFromWarehouse.Model);
            NewEntryRemainingStock = (remainingStock / _newEntryProduct.PiecesPerUnit) + "/" + (remainingStock % _newEntryProduct.PiecesPerUnit) + " " + _newEntryProduct.UnitName;
        }
        #endregion
    }
}
