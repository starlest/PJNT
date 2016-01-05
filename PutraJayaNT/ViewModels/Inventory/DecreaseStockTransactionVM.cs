using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.StockCorrection;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Inventory
{
    class DecreaseStockTransactionVM : ViewModelBase<DecreaseStockTransaction>
    {
        #region Collections Backing Fields
        ObservableCollection<WarehouseVM> _warehouses;
        ObservableCollection<ItemVM> _products;
        ObservableCollection<DecreaseStockTransactionLineVM> _lines;
        #endregion

        #region Transaction Backing Fields
        string _newTransactionID;
        ICommand _newTransactionCommand;
        ICommand _confirmTransactionCommand;
        #endregion

        #region New Entry Backing Fields
        WarehouseVM _newEntryWarehouse;
        ItemVM _newEntryProduct;
        string _newEntryUnitName;
        int? _newEntryPiecesPerUnit;
        int? _newEntryUnits;
        int? _newEntryPieces;
        ICommand _newEntryCommand;
        #endregion

        bool _isNotEditMode;

        public DecreaseStockTransactionVM()
        {
            Model = new DecreaseStockTransaction();
            Model.DecreaseStockTransactionLines = new ObservableCollection<DecreaseStockTransactionLine>();

            _warehouses = new ObservableCollection<WarehouseVM>();
            _products = new ObservableCollection<ItemVM>();
            _lines = new ObservableCollection<DecreaseStockTransactionLineVM>();

            _isNotEditMode = true;

            UpdateWarehouses();
            SetTransactionID();
        }

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }

        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
        }

        public ObservableCollection<DecreaseStockTransactionLineVM> Lines
        {
            get { return _lines; }
        }

        public bool IsNotEditMode
        {
            get { return _isNotEditMode; }
            set { SetProperty(ref _isNotEditMode, value, "IsNotEditMode"); }
        }

        #region Transaction Properties 
        public string NewTransactionID
        {
            get { return _newTransactionID; }
            set
            {
                SetProperty(ref _newTransactionID, value, "NewTransactionID");
                IsNotEditMode = false;
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

        public ICommand ConfirmTransactionCommand
        {
            get
            {
                return _confirmTransactionCommand ?? (_confirmTransactionCommand = new RelayCommand(() =>
                {
                    // Verify user can perform operation
                    var window = new VerificationWindow();
                    window.ShowDialog();
                    App.Current.MainWindow.IsEnabled = true;
                    var isVerified = App.Current.TryFindResource("IsVerified");
                    if (isVerified == null) return;

                    if (MessageBox.Show("Confirm transaction?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

                    if (_lines.Count == 0)
                    {
                        MessageBox.Show("Transaction is empty.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    using (var ts = new TransactionScope())
                    {
                        var context = new ERPContext();

                        decimal cogs = 0;

                        // Add each line to the transaction
                        foreach (var line in _lines)
                        {
                            line.Item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                            line.Warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();
                            Model.DecreaseStockTransactionLines.Add(line.Model);

                            // Decrease the stock for this item in the corresponding warehouse
                            var stock = context.Stocks.Where(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID)).FirstOrDefault();
                            stock.Pieces -= line.Quantity;

                            // Increase SoldOrReturned for this item
                            var purchaseLine = context.PurchaseTransactionLines
                            .Include("PurchaseTransaction")
                            .Where(e => e.ItemID.Equals(line.Item.ItemID) && e.SoldOrReturned < e.Quantity)
                            .OrderBy(e => e.PurchaseTransactionID)
                            .FirstOrDefault();
                            purchaseLine.SoldOrReturned += line.Quantity;

                            var purchaseLineNetTotal = (purchaseLine.PurchasePrice - purchaseLine.Discount) * line.Quantity;
                            var fractionOfTransactionDiscount = (line.Quantity * purchaseLineNetTotal / purchaseLine.PurchaseTransaction.GrossTotal) * purchaseLine.PurchaseTransaction.Discount;
                            cogs += purchaseLineNetTotal - fractionOfTransactionDiscount;
                        }

                        // Add the transaction to the database
                        Model.Date = DateTime.Now.Date;
                        var user = App.Current.TryFindResource("CurrentUser") as User;
                        if (user != null) Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                        context.DecreaseStockTransactions.Add(Model);

                        // Add the journal entries for this transaction
                        var transaction = new LedgerTransaction();
                        LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, Model.DecreaseStrockTransactionID, "Stock Adjustment (Decrement)");
                        context.SaveChanges();
                        LedgerDBHelper.AddTransactionLine(context, transaction, "Cost of Goods Sold", "Debit", cogs);
                        LedgerDBHelper.AddTransactionLine(context, transaction, "Inventory", "Credit", cogs);

                        context.SaveChanges();
                        ts.Complete();
                    }

                    MessageBox.Show("Transaction is saved.", "Success", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }
        #endregion

        #region New Entry Properties
        public WarehouseVM NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set
            {
                SetProperty(ref _newEntryWarehouse, value, "NewEntryWarehouse");

                if (_newEntryWarehouse == null) return;

                UpdateProducts();
            }
        }

        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, "NewEntryProduct");

                if (_newEntryProduct == null) return;

                NewEntryUnitName = _newEntryProduct.UnitName;
                NewEntryPiecesPerUnit = _newEntryProduct.PiecesPerUnit;
            }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, "NewEntryUnitName"); }
        }

        public int? NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, "NewEntryPiecesPerUnit"); }
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

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (_newEntryWarehouse == null || _newEntryProduct == null ||
                    (_newEntryUnits == null && _newEntryPieces == null)) 
                    {
                        MessageBox.Show("Please enter all fields.", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    foreach (var line in _lines)
                    {
                        if (line.Item.ItemID.Equals(_newEntryProduct.ID) && 
                            line.Warehouse.ID.Equals(_newEntryWarehouse.ID))
                        {
                            line.Units += _newEntryUnits == null ? 0 : (int)_newEntryUnits;
                            line.Pieces += _newEntryPieces == null ? 0 : (int)_newEntryPieces;
                            ResetEntryFields();
                            return;
                        }
                    }

                    var quantity = (_newEntryPieces == null ? 0 : (int)_newEntryPieces) + ((_newEntryUnits == null ? 0 : (int)_newEntryUnits) * _newEntryProduct.PiecesPerUnit);
                    var newEntry = new DecreaseStockTransactionLineVM
                    {
                        Model = new DecreaseStockTransactionLine
                        {
                            DecreaseStockTransaction = this.Model,
                            Item = _newEntryProduct.Model,
                            Warehouse = _newEntryWarehouse.Model,
                            Quantity = quantity
                        },
                    };

                    _lines.Add(newEntry);
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region Helper Methods
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

        private void UpdateProducts()
        {
            _products.Clear();

            using (var context = new ERPContext())
            {
                var products = context.Stocks
                    .Include("Item")
                    .Where(e => e.WarehouseID.Equals(_newEntryWarehouse.ID))
                    .ToList();

                foreach (var product in products)
                    _products.Add(new ItemVM { Model = product.Item });
            }
        }

        private void SetTransactionID()
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            _newTransactionID = "DS" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from DecreaseStockTransaction in context.DecreaseStockTransactions
                           where DecreaseStockTransaction.DecreaseStrockTransactionID.CompareTo(_newTransactionID) >= 0
                           orderby DecreaseStockTransaction.DecreaseStrockTransactionID descending
                           select DecreaseStockTransaction.DecreaseStrockTransactionID);
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _newTransactionID = "DS" + (Convert.ToInt64(lastTransactionID.Substring(2)) + 1).ToString();

            Model.DecreaseStrockTransactionID = _newTransactionID;
            OnPropertyChanged("NewTransactionID");
        }

        private void ResetEntryFields()
        {
            NewEntryProduct = null;
            NewEntryUnitName = null;
            NewEntryPiecesPerUnit = null;
            NewEntryUnits = null;
            NewEntryPieces = null;
        }

        private void ResetTransaction()
        {
            IsNotEditMode = true;
            NewEntryWarehouse = null;
            ResetEntryFields();
            _lines.Clear();
            Model = new DecreaseStockTransaction();
            Model.DecreaseStockTransactionLines = new ObservableCollection<DecreaseStockTransactionLine>();
            SetTransactionID();
        }
        #endregion
    }
}
