using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Purchase;
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
    class AdjustStockTransactionVM : ViewModelBase<AdjustStockTransaction>
    {
        #region Collections Backing Fields
        ObservableCollection<WarehouseVM> _warehouses;
        ObservableCollection<ItemVM> _products;
        ObservableCollection<AdjustStockTransactionLineVM> _lines;
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
        int _remainingStock;
        string _newEntryRemainingStock;
        ICommand _newEntryCommand;
        #endregion

        AdjustStockTransactionLineVM _selectedLine;
        ICommand _deleteLineCommand;

        bool _isNotEditMode;

        public AdjustStockTransactionVM()
        {
            Model = new AdjustStockTransaction();
            Model.AdjustStockTransactionLines = new ObservableCollection<AdjustStockTransactionLine>();

            _warehouses = new ObservableCollection<WarehouseVM>();
            _products = new ObservableCollection<ItemVM>();
            _lines = new ObservableCollection<AdjustStockTransactionLineVM>();

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

        public ObservableCollection<AdjustStockTransactionLineVM> Lines
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
                if (!CheckIDExists(value))
                {
                    MessageBox.Show("Transaction does not exists.", "Invalid ID", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionID, value, "NewTransactionID");

                SetEditMode();
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
                    // Verification
                    if (!UtilityMethods.GetVerification()) return;

                    if (MessageBox.Show("Confirm transaction?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

                    if (_lines.Count == 0)
                    {
                        MessageBox.Show("Transaction is empty.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    using (var ts = new TransactionScope())
                    {
                        var context = new ERPContext();

                        var user = App.Current.TryFindResource("CurrentUser") as User;
                        var _user = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();

                        var newPurchaseTransaction = new PurchaseTransaction
                        {
                            PurchaseID = _newTransactionID,
                            Supplier = context.Suppliers.Where(e => e.Name.Equals("-")).FirstOrDefault(),
                            Date = DateTime.Now.Date,
                            DueDate = DateTime.Now.Date,
                            Discount = 0,
                            GrossTotal = 0,
                            Total = 0,
                            Paid = 0,
                            User = _user,
                            PurchaseTransactionLines = new ObservableCollection<PurchaseTransactionLine>()
                        };

                        decimal cogs = 0;

                        var decreaseAdjustment = false;
                        var increaseAdjustment = false;

                        // Add each line to the transaction
                        foreach (var line in _lines)
                        {

                            line.Item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                            line.Warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();
                            Model.AdjustStockTransactionLines.Add(line.Model);

                            if (line.Quantity < 0)
                            {
                                decreaseAdjustment = true;

                                // Decrease the stock for this item in the corresponding warehouse
                                var stock = context.Stocks.Where(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID)).FirstOrDefault();
                                stock.Pieces += line.Quantity;
                                if (stock.Pieces == 0) context.Stocks.Remove(stock);

                                // Increase SoldOrReturned for this item and COGS
                                var purchases = context.PurchaseTransactionLines
                                .Include("PurchaseTransaction")
                                .Where(e => e.ItemID.Equals(line.Item.ItemID) && e.SoldOrReturned < e.Quantity)
                                .OrderBy(e => e.PurchaseTransactionID)
                                .ToList();

                                var tracker = -line.Quantity;

                                foreach (var purchase in purchases)
                                {
                                    var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                                    var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;

                                    if (tracker <= availableQuantity)
                                    {
                                        purchase.SoldOrReturned += tracker;
                                        if (purchaseLineNetTotal == 0) break;
                                        var fractionOfTransactionDiscount = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                                        var fractionOfTransactionTax = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                                        cogs += (tracker * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                                        break;
                                    }
                                    else if (tracker > availableQuantity)
                                    {
                                        purchase.SoldOrReturned += availableQuantity;
                                        tracker -= availableQuantity;
                                        if (purchaseLineNetTotal == 0) continue;
                                        var fractionOfTransactionDiscount = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                                        var fractionOfTransactionTax = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                                        cogs += (availableQuantity * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                                    }
                                }
                            }

                            else
                            {
                                increaseAdjustment = true;

                                // Increase the stock for this item in the corresponding warehouse
                                var stock = context.Stocks.Where(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID)).FirstOrDefault();
                                if (stock == null)
                                {
                                    var newStock = new Stock
                                    {
                                        Item = line.Item,
                                        Warehouse = line.Warehouse,
                                        Pieces = line.Quantity
                                    };

                                    context.Stocks.Add(newStock);
                                }
                                stock.Pieces += line.Quantity;

                                // Add the line into the new purchase transaction
                                var l = new PurchaseTransactionLine
                                {
                                    PurchaseTransaction = newPurchaseTransaction,
                                    Item = line.Item,
                                    Warehouse = line.Warehouse,
                                    PurchasePrice = 0,
                                    Discount = 0,
                                    Quantity = line.Quantity,
                                    Total = 0,
                                    SoldOrReturned = 0
                                };
                                newPurchaseTransaction.PurchaseTransactionLines.Add(l);
                            }
                        }

                        // Add the transaction to the database
                        Model.Date = DateTime.Now.Date;
                        Model.User = _user;
                        context.AdjustStockTransactions.Add(Model);

                        // Add the journal entries for this transaction
                        if (decreaseAdjustment)
                        {
                            var transaction = new LedgerTransaction();
                            if (!LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, _newTransactionID, "Stock Adjustment (Decrement)")) return;
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, transaction, "Cost of Goods Sold", "Debit", cogs);
                            LedgerDBHelper.AddTransactionLine(context, transaction, "Inventory", "Credit", cogs);
                        }

                        if (increaseAdjustment)
                        {
                            context.PurchaseTransactions.Add(newPurchaseTransaction);
                        }

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
                _remainingStock = UtilityMethods.GetRemainingStock(_newEntryProduct.Model, _newEntryWarehouse.Model);
                NewEntryRemainingStock = (_remainingStock / _newEntryProduct.PiecesPerUnit) + "/" + (_remainingStock % _newEntryProduct.PiecesPerUnit);
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

        public string NewEntryRemainingStock
        {
            get { return _newEntryRemainingStock; }
            set { SetProperty(ref _newEntryRemainingStock, value, "NewEntryRemainingStock"); }
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

                    var quantity = (_newEntryPieces == null ? 0 : (int)_newEntryPieces) + ((_newEntryUnits == null ? 0 : (int)_newEntryUnits) * _newEntryProduct.PiecesPerUnit);
                    if (quantity < 0 && -quantity > _remainingStock)
                    {
                        MessageBox.Show("There is not enough stock.", "Insufficient Stock", MessageBoxButton.OK);
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

                    var newEntry = new AdjustStockTransactionLineVM
                    {
                        Model = new AdjustStockTransactionLine
                        {
                            AdjustStockTransaction = this.Model,
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

        #region Line Properties
        public AdjustStockTransactionLineVM SelectedLine
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

        private bool CheckIDExists(string id)
        {
            using (var context = new ERPContext())
            {
                var transaction = context.AdjustStockTransactions.Where(e => e.AdjustStrockTransactionID.Equals(id)).FirstOrDefault();
                return transaction != null;
            }
        }

        private void SetEditMode()
        {
            IsNotEditMode = false;
            ResetEntryFields();
            _lines.Clear();

            using (var context = new ERPContext())
            {
                var lines = context.AdjustStockTransactionLines
                    .Include("Item")
                    .Include("Warehouse")
                    .Where(e => e.AdjustStockTransactionID.Equals(_newTransactionID))
                    .ToList();

                foreach (var line in lines)
                    _lines.Add(new AdjustStockTransactionLineVM { Model = line });
            }
        }

        private void SetTransactionID()
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            _newTransactionID = "SA" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from AdjustStockTransaction in context.AdjustStockTransactions
                           where AdjustStockTransaction.AdjustStrockTransactionID.CompareTo(_newTransactionID) >= 0 && AdjustStockTransaction.AdjustStrockTransactionID.Substring(0, 2).Equals("SA")
                           orderby AdjustStockTransaction.AdjustStrockTransactionID descending
                           select AdjustStockTransaction.AdjustStrockTransactionID);
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _newTransactionID = "SA" + (Convert.ToInt64(lastTransactionID.Substring(2)) + 1).ToString();

            Model.AdjustStrockTransactionID = _newTransactionID;
            OnPropertyChanged("NewTransactionID");
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
            NewEntryWarehouse = null;
            ResetEntryFields();
            _lines.Clear();
            Model = new AdjustStockTransaction();
            Model.AdjustStockTransactionLines = new ObservableCollection<AdjustStockTransactionLine>();
            SetTransactionID();
        }
        #endregion
    }
}
