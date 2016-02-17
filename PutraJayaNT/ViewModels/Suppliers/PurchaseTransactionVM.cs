using MVVMFramework;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using PutraJayaNT.Models.Accounting;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Transactions;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Purchase;
using System.Collections.Generic;
using System.Data.Entity;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PurchaseTransactionVM : ViewModelBase<PurchaseTransaction>
    {
        ObservableCollection<PurchaseTransactionLineVM> _lines;
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<Item> _supplierItems;
        ObservableCollection<Warehouse> _warehouses;

        bool _notEditMode;
        bool _isTransactionNotPaid;

        #region Transaction backing fields
        Supplier _newTransactionSupplier;
        string _newTransactionID;
        string _newTransactionDOID;
        DateTime _newTransactionDate;
        DateTime _newTransactionDueDate;
        decimal? _newTransactionDiscountPercent;
        decimal? _newTransactionDiscount;
        decimal? _newTransactionTax;
        decimal? _newTransactionGrossTotal;
        decimal? _newTransactionNetTotal;
        string _newTransactionNote;
        #endregion

        Warehouse _newEntryWarehouse;
        Item _newEntryItem;
        string _newEntryUnitName;
        int _newEntryQuantity;
        int? _newEntryUnits;
        int? _newEntryPieces;
        int? _newEntryPiecesPerUnit;
        decimal? _newEntryPrice;
        decimal? _newEntryDiscountPercent;
        decimal? _newEntryDiscount;

        bool _newEntrySubmitted;

        ICommand _newEntryCommand;
        ICommand _confirmTransactionCommand;
        ICommand _newTransactionCommand;

        PurchaseTransactionLineVM _selectedLine;
        decimal _editLinePurchasePrice;
        decimal _editLineDiscount;
        bool _isEditWindowNotOpen;
        Visibility _editWindowVisibility;
        ICommand _editLineCommand;
        ICommand _editLineConfirmCommand;
        ICommand _editLineCancelCommand;
        ICommand _deleteLineCommand;

        public PurchaseTransactionVM()
        {
            Model = new PurchaseTransaction();
            _lines = new ObservableCollection<PurchaseTransactionLineVM>();
            _suppliers = new ObservableCollection<Supplier>();
            _supplierItems = new ObservableCollection<Item>();
            _warehouses = new ObservableCollection<Warehouse>();

            UpdateSuppliers();

            NewTransactionDate = UtilityMethods.GetCurrentDate().Date;
            NewTransactionDueDate = UtilityMethods.GetCurrentDate().Date;

            SetTransactionID();
            Model.DueDate = _newTransactionDueDate;
            Model.Total = 0;

            _notEditMode = true;
            _isTransactionNotPaid = true;
            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;
        }

        public ObservableCollection<PurchaseTransactionLineVM> Lines
        {
            get { return _lines; }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get { return _suppliers; } 
        } 

        public ObservableCollection<Item> SupplierItems
        {
            get { return _supplierItems; }
        }

        public ObservableCollection<Warehouse> Warehouses
        {
            get { return _warehouses; }
        }

        public bool IsTransactionNotPaid
        {
            get { return _isTransactionNotPaid; }
            set { SetProperty(ref _isTransactionNotPaid, value, "IsTransactionNotPaid"); }
        }

        public bool NotEditMode
        {
            get { return _notEditMode; }
            set { SetProperty(ref _notEditMode, value, "NotEditMode"); }
        }

        #region New Transaction Properties
        public string NewTransactionID
        {
            get { return _newTransactionID; }
            set
            {
                if (_newTransactionID == null) return;

                ResetEntryFields();
                NewEntryWarehouse = null;

                // Search the database for the transaction
                using (var context = new ERPContext())
                {
                    var transaction = context.PurchaseTransactions
                        .Include("Supplier")
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .Where(e => e.PurchaseID.Equals(value))
                        .FirstOrDefault();

                    if (transaction == null)
                    {
                        MessageBox.Show("The sales transaction could not be found.", "Invalid Sales Transaction", MessageBoxButton.OK);
                        return;
                    }

                    NotEditMode = false;
                    SetTransaction(transaction);
                    SetProperty(ref _newTransactionID, value, "NewTransactionID");
                }
            }
        }

        public string NewTransactionDOID
        {
            get { return _newTransactionDOID; }
            set { SetProperty(ref _newTransactionDOID, value, "NewTransactionDOID"); }
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

        public DateTime NewTransactionDueDate
        {
            get { return _newTransactionDueDate; }
            set
            {
                if (value < _newTransactionDate)
                {
                    MessageBox.Show("Cannot set to before transaction date.", "Invalid Date", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionDueDate, value, "NewTransactionDueDate");
            }
        }

        public Supplier NewTransactionSupplier
        {
            get { return _newTransactionSupplier; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _newTransactionSupplier, value, "NewTransactionSupplier");
                    _supplierItems.Clear();                    
                    return;
                }

                UpdateSuppliers();
                UpdateWarehouses();

                foreach (var supplier in _suppliers)
                {
                    if (supplier.ID.Equals(value.ID))
                    {
                        value = supplier;
                        break;
                    }
                }

                _suppliers.Clear();
                _suppliers.Add(value);

                // Display Supplier's Items and IDs
                _supplierItems.Clear();
                using (var context = new ERPContext())
                {
                    var items = context.Inventory
                        .Include("Suppliers")
                        .ToList();

                    foreach (var item in items)
                    {
                        if (item.Active == true && item.Suppliers.Contains(value))
                            _supplierItems.Add(item);
                    }
                }

                SetProperty(ref _newTransactionSupplier, value, "NewTransactionSupplier");
            }
        }

        public decimal? NewTransactionGrossTotal
        {
            get
            {
                _newTransactionGrossTotal = 0;
                foreach (var line in _lines)
                {
                    _newTransactionGrossTotal += line.Total;
                }
                OnPropertyChanged("NewTransactionNetTotal");
                return _newTransactionGrossTotal;
            }
        }

        public decimal? NewTransactionDiscountPercent
        {
            get { return _newTransactionDiscountPercent; }
            set
            {
                if (value != null && (value < 0 || value > 100))
                {
                    MessageBox.Show("Please enter a value from the range of 0 - 100.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionDiscountPercent, value, "NewTransactionDiscountPercent");

                if (_newTransactionDiscountPercent == null) return;

                NewTransactionDiscount = _newTransactionDiscountPercent / 100 * _newTransactionGrossTotal;
                NewTransactionDiscountPercent = null;
            }
        }

        public decimal? NewTransactionDiscount
        {
            get { return _newTransactionDiscount; }
            set
            {
                if (value != null && (value < 0 || value > _newTransactionGrossTotal))
                {
                    MessageBox.Show(string.Format("Please enter a value from the range of 0 - {0}.", _newTransactionGrossTotal), "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionDiscount, value, "NewTransactionDiscount");
                OnPropertyChanged("NewTransactionNetTotal");
            }
        }

        public decimal? NewTransactionTax
        {
            get { return _newTransactionTax; }
            set
            {
                if (value != null && (value < 0 || value > _newTransactionGrossTotal))
                {
                    MessageBox.Show(string.Format("Please enter a value from the range of 0 - {0}.", _newTransactionGrossTotal), "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionTax, value, "NewTransactionTax");
                OnPropertyChanged("NewTransactionNetTotal");
            }
        }

        public decimal? NewTransactionNetTotal
        {
            get
            {
                _newTransactionNetTotal = (_newTransactionGrossTotal == null ? 0 : (decimal)_newTransactionGrossTotal) - (_newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount) + (_newTransactionTax == null ? 0 : (decimal)_newTransactionTax);
                return _newTransactionNetTotal;
            }
        }

        public string NewTransactionNote
        {
            get { return _newTransactionNote; }
            set { SetProperty(ref _newTransactionNote, value, "NewTransactionNote"); }
        }
        #endregion

        #region New Entry Properties
        public Warehouse NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set { SetProperty(ref _newEntryWarehouse, value, "NewEntryWarehouse"); }
        }

        public Item NewEntryItem
        {
            get { return _newEntryItem; }
            set
            {
                SetProperty(ref _newEntryItem, value, "NewEntryItem");

                if (_newEntryItem == null) return;

                NewEntryUnitName = _newEntryItem.UnitName;
                NewEntryPrice = _newEntryItem.PurchasePrice * _newEntryItem.PiecesPerUnit;
                NewEntryPiecesPerUnit = _newEntryItem.PiecesPerUnit;
            }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, "NewEntryUnitName"); }
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

        public int? NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, "NewEntryPiecesPerUnit"); }
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

                SetProperty(ref _newEntryDiscountPercent, value, "NewEntryDiscountPercent");

                if (_newEntryDiscountPercent == null) return;

                NewEntryDiscount = _newEntryDiscountPercent / 100 * _newEntryPrice;
                NewEntryDiscountPercent = null;
            }
        }

        public decimal? NewEntryDiscount
        {
            get { return _newEntryDiscount; }
            set
            {
                if (value != null && (value < 0 || value > _newEntryPrice))
                {
                    MessageBox.Show(string.Format("Please enter a value from the range of 0 - {0}.", _newEntryPrice), "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newEntryDiscount, value, "NewEntryDiscount");
            }
        }

        public decimal? NewEntryPrice
        {
            get { return _newEntryPrice; }
            set { SetProperty(ref _newEntryPrice, value, "NewEntryPrice"); }
        }

        public bool NewEntrySubmitted
        {
            get { return _newEntrySubmitted; }
            set { SetProperty(ref _newEntrySubmitted, value, "NewEntrySubmitted"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (_newTransactionSupplier == null || _newEntryItem == null || _newEntryWarehouse == null
                    || (_newEntryUnits == null && _newEntryPieces == null) || _newEntryPrice == null) 
                    {
                        MessageBox.Show("Please enter all required fields", "Missing field(s)", MessageBoxButton.OK);
                        return;
                    }

                    _newEntryQuantity = ((_newEntryUnits != null ?  (int) _newEntryUnits : 0)  * _newEntryItem.PiecesPerUnit)
                    + (_newEntryPieces != null ? (int)_newEntryPieces : 0);

                    // Check if the item exists in one of the lines 
                    foreach (var l in _lines)
                    {
                        var discount = _newEntryDiscount == null ? 0 : (decimal) _newEntryDiscount;
                        if (_newEntryItem.ItemID.Equals(l.Item.ItemID) 
                        && _newEntryWarehouse.ID.Equals(l.Warehouse.ID) 
                        && Math.Round((double)_newEntryPrice, 2).Equals(Math.Round(l.PurchasePrice, 2))
                        && Math.Round(discount, 2).Equals(Math.Round(l.Discount, 2)))
                        {
                            l.Quantity += _newEntryQuantity;
                            ResetEntryFields();
                            return;
                        }
                    }

                    var line = new PurchaseTransactionLine
                    {
                        Quantity = _newEntryQuantity,
                        PurchasePrice = (decimal)_newEntryPrice / _newEntryItem.PiecesPerUnit,
                        Discount = (_newEntryDiscount == null ? 0 : (decimal)_newEntryDiscount) / _newEntryItem.PiecesPerUnit,
                        Item = _newEntryItem,
                        Warehouse = _newEntryWarehouse,
                        SoldOrReturned = 0,
                        PurchaseTransaction = Model
                    };

                    line.Total = line.Quantity * (line.PurchasePrice - line.Discount);

                    var lineVM = new PurchaseTransactionLineVM { Model = line };
                    _lines.Add(lineVM);

                    // Update Total Amount
                    OnPropertyChanged("NewTransactionGrossTotal");

                    ResetEntryFields();

                    NewEntrySubmitted = true;
                    NewEntrySubmitted = false;
                }));
            }
        }
        #endregion

        #region Line Properties
        public bool IsEditWindowNotOpen
        {
            get { return _isEditWindowNotOpen; }
            set { SetProperty(ref _isEditWindowNotOpen, value, "IsEditWindowNotOpen"); }
        }

        public Visibility EditWindowVisibility
        {
            get { return _editWindowVisibility; }
            set { SetProperty(ref _editWindowVisibility, value, "EditWindowVisibility"); }
        }

        public PurchaseTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        public decimal EditLinePurchasePrice
        {
            get { return _editLinePurchasePrice; }
            set { SetProperty(ref _editLinePurchasePrice, value, "EditLinePurchasePrice"); }
        }

        public decimal EditLineDiscount
        {
            get { return _editLineDiscount; }
            set { SetProperty(ref _editLineDiscount, value, "EditLineDiscount"); }
        }

        public ICommand EditLineCommand
        {
            get
            {
                return _editLineCommand ?? (_editLineCommand = new RelayCommand(() =>
                {
                    if (_selectedLine != null)
                    {
                        IsEditWindowNotOpen = false;
                        EditWindowVisibility = Visibility.Visible;

                        EditLinePurchasePrice = _selectedLine.PurchasePrice;
                        EditLineDiscount = _selectedLine.Discount;
                    }
                }));
            }
        }

        public ICommand EditLineCancelCommand
        {
            get
            {
                return _editLineCancelCommand ?? (_editLineCancelCommand = new RelayCommand(() =>
                {
                    IsEditWindowNotOpen = true;
                    EditWindowVisibility = Visibility.Hidden;
                }));
            }
        }

        public ICommand EditLineConfirmCommand
        {
            get
            {
                return _editLineConfirmCommand ?? (_editLineConfirmCommand = new RelayCommand(() =>
                {
                    _selectedLine.PurchasePrice = _editLinePurchasePrice;
                    _selectedLine.Discount = _editLineDiscount;
                    IsEditWindowNotOpen = true;
                    EditWindowVisibility = Visibility.Hidden;

                    OnPropertyChanged("NewTransactionGrossTotal");
                }));
            }
        }

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (_selectedLine != null)
                    {
                        _lines.Remove(_selectedLine);
                        OnPropertyChanged("NewTransactionGrossTotal");
                    }
                }));
            }
        }
        #endregion

        public ICommand ConfirmTransactionCommand
        {
            get
            {
                return _confirmTransactionCommand ?? (_confirmTransactionCommand = new RelayCommand(() =>
                {
                    if (_notEditMode)
                    {
                        if (_lines.Count == 0)
                        {
                            MessageBox.Show("Please input at least one entry.", "Empty Transaction", MessageBoxButton.OK);
                            return;
                        }

                        if (MessageBox.Show("Confirm saving transaction?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                        SaveTransaction();
                    }       
                    
                    else
                    {
                        if (MessageBox.Show("Confirm saving transaction?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                        EditTransaction();
                    }  
                }));
            }
        }

        public ICommand NewTransactionCommand
        {
            get
            {
                return _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(() =>
                {
                        // Reset the fields for a new transaction
                        ResetFields();
                }));
            }
        }

        #region Helper Methods
        private void UpdateSuppliers()
        {
            _suppliers.Clear();

            using (var uow = new UnitOfWork())
            {
                var suppliers = uow.SupplierRepository.GetAll();
                foreach (var supplier in suppliers)
                    if (supplier.Name != "-")
                        _suppliers.Add(supplier);
            }
        }

        private void UpdateWarehouses()
        {
            _warehouses.Clear();

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();
                foreach (var warehouse in warehouses)
                    _warehouses.Add(warehouse);
            }
        }

        private void SetTransaction(PurchaseTransaction transaction)
        {
            IsTransactionNotPaid = transaction.Paid == 0; 
            Model = transaction;
            NewTransactionDOID = transaction.DOID;
            NewTransactionSupplier = null; // to prevent buggy outcome
            NewTransactionSupplier = transaction.Supplier;
            NewTransactionDate = transaction.Date;
            NewTransactionDueDate = transaction.DueDate;
            NewTransactionNote = transaction.Note;

            _lines.Clear();
            foreach (var line in transaction.PurchaseTransactionLines.OrderBy(e => e.Item.Name).ThenBy(e => e.Warehouse.Name).ThenBy(e => e.PurchasePrice).ThenBy(e => e.Discount).ToList())
                _lines.Add(new PurchaseTransactionLineVM { Model = line });

            _newTransactionDiscount = transaction.Discount;
            _newTransactionTax = transaction.Tax;
            OnPropertyChanged("NewTransactionDiscount");
            OnPropertyChanged("NewTransactionTax");
            OnPropertyChanged("NewTransactionGrossTotal"); // Updates transaction's net total too
        }

        private void SetTransactionID()
        {
            var month = _newTransactionDate.Month;
            var year = _newTransactionDate.Year;
            _newTransactionID = "P" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastEntryID = null;
            using(var context = new ERPContext())
            {
                var IDs = (from PurchaseTransaction in context.PurchaseTransactions
                           where PurchaseTransaction.PurchaseID.CompareTo(_newTransactionID.ToString()) >= 0 && PurchaseTransaction.PurchaseID.Substring(0, 1).Equals("P")
                           orderby PurchaseTransaction.PurchaseID descending
                           select PurchaseTransaction.PurchaseID);
                if (IDs.Count() != 0)
                {
                    foreach (var id in IDs)
                    {
                        if (id.Substring(0, 2) == "SA") continue;
                        else
                        {
                            lastEntryID = id;
                            break;
                        }
                    }
                } 
            }

            if (lastEntryID != null) _newTransactionID = "P" + (Convert.ToInt64(lastEntryID.Substring(1)) + 1).ToString();

            Model.PurchaseID = _newTransactionID;

            OnPropertyChanged("NewTransactionID");
        }

        private void ResetEntryFields()
        {
            NewEntryItem = null;
            _newEntryQuantity = 0;
            NewEntryPieces = null;
            NewEntryUnits = null;
            NewEntryPrice = null;
            NewEntryUnitName = null;
            NewEntryPiecesPerUnit = null;
        }

        private void ResetFields()
        {
            ResetEntryFields();
            NewEntryWarehouse = null;

            IsTransactionNotPaid = true;
            NotEditMode = true;

            Model = new PurchaseTransaction();
            NewTransactionSupplier = null;
            NewTransactionDOID = null;
            NewTransactionTax = null;
            NewTransactionDiscount = null;
            NewTransactionNote = null;
            NewTransactionDate = UtilityMethods.GetCurrentDate().Date;
            NewTransactionDueDate = UtilityMethods.GetCurrentDate().Date;

            _warehouses.Clear();
            _supplierItems.Clear();
            _lines.Clear();
            UpdateSuppliers();

            OnPropertyChanged("Suppliers");
            OnPropertyChanged("NewTransactionGrossTotal");
            OnPropertyChanged("NewTransactionNetTotal");

            SetTransactionID();
        }

        private void SaveTransaction()
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                // Increase the respective item's Quantity
                foreach (var line in _lines)
                {
                    var item = context.Inventory
                    .Where(e => e.ItemID.Equals(line.Item.ItemID))
                    .FirstOrDefault();

                    var warehouse = context.Warehouses
                    .Where(e => e.ID.Equals(line.Warehouse.ID))
                    .FirstOrDefault();

                    var stock = context.Stocks
                    .Where(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID))
                    .FirstOrDefault();

                    if (stock == null)
                    {
                        context.Stocks.Add(new Stock { Item = item, Warehouse = warehouse, Pieces = line.Quantity });
                        context.SaveChanges();
                    }
                    else
                        stock.Pieces += line.Quantity;

                    line.Item = item;
                    line.Warehouse = warehouse;

                    Model.PurchaseTransactionLines.Add(line.Model);
                }

                SetTransactionID();
                Model.DOID = _newTransactionDOID;
                Model.Date = _newTransactionDate;
                Model.DueDate = _newTransactionDueDate;
                Model.GrossTotal = _newTransactionGrossTotal == null ? 0 : (decimal)_newTransactionGrossTotal;
                Model.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
                Model.Tax = _newTransactionTax == null ? 0 : (decimal)_newTransactionTax;
                Model.Total = (decimal)_newTransactionNetTotal;
                Model.Note = _newTransactionNote;
                Model.Supplier = context.Suppliers.Where(e => e.ID == _newTransactionSupplier.ID).FirstOrDefault();
                var user = App.Current.FindResource("CurrentUser") as User;
                Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();

                context.PurchaseTransactions.Add(Model); // Insert into database

                // Record the accounting journal entry for this purchase
                LedgerTransaction transaction = new LedgerTransaction();
                string accountsPayableName = _newTransactionSupplier.Name + " Accounts Payable";

                if (!LedgerDBHelper.AddTransaction(context, transaction, _newTransactionDate, _newTransactionID.ToString(), "Purchase Transaction")) return;
                context.SaveChanges();
                LedgerDBHelper.AddTransactionLine(context, transaction, "Inventory", "Debit", Model.Total);
                LedgerDBHelper.AddTransactionLine(context, transaction, accountsPayableName, "Credit", Model.Total);
                context.SaveChanges();

                ts.Complete();

                MessageBox.Show("Transaction saved!", "Success", MessageBoxButton.OK);
            }

            // Reset the fields for a new transaction
            Model = new PurchaseTransaction();
            ResetFields();
            SetTransactionID();
            Model.Total = 0;
        }

        private void EditTransaction()
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                var transaction = context.PurchaseTransactions
                    .Include("PurchaseTransactionLines")
                    .Include("PurchaseTransactionLines.Item")
                    .Include("PurchaseTransactionLines.Warehouse")
                    .Where(e => e.PurchaseID.Equals(Model.PurchaseID))
                    .FirstOrDefault();

                var originalLines = transaction.PurchaseTransactionLines.OrderBy(e => e.Item.Name).ThenBy(e => e.Warehouse.Name).ThenBy(e => e.PurchasePrice).ThenBy(e => e.Discount).ToList();

                // Ledger adjustments when the purchase total is changed
                if (transaction.Total != _newTransactionNetTotal)
                {
                    // Adjust Supplier's Accounts Payable and Inventory
                    var ledgerTransaction1 = new LedgerTransaction();
                    LedgerDBHelper.AddTransaction(context, ledgerTransaction1, UtilityMethods.GetCurrentDate().Date, _newTransactionID, "Purchase Transaction Adjustment");
                    context.SaveChanges();

                    if (_newTransactionNetTotal > transaction.Total)
                    {
                        var valueChanged = (decimal)_newTransactionNetTotal - transaction.Total;
                        LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Inventory", "Debit", valueChanged);
                        LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, string.Format("{0} Accounts Payable", _newTransactionSupplier.Name), "Credit", valueChanged);
                    }
                    else
                    {
                        var valueChanged = transaction.Total - (decimal)_newTransactionNetTotal;
                        LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, string.Format("{0} Accounts Payable", _newTransactionSupplier.Name), "Debit", valueChanged);
                        LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Inventory", "Credit", valueChanged);
                    }

                    var originalTransactionGrossTotal = transaction.GrossTotal;
                    var originalTransactionDiscount = transaction.Discount;
                    var originalTransactionTax = transaction.Tax;
                    var totalValueChanged = 0m;
                    var changesMade = false; // Flag to indicate if the COGS and Inventory needs to be adjusted
                    List<int> linesChanged = new List<int>(); // List of indexes of lines that have been changed (Price/Discount)
                    for (int i = 0; i < _lines.Count; i++)
                    {
                        var line = _lines[i];
                        var originalLine = originalLines[i];

                        if (originalLine.SoldOrReturned > 0)
                        {
                            changesMade = true;
                            // Calculate the original line's COGS
                            decimal originalLineCOGS;
                            var originalLineNetTotal = originalLine.PurchasePrice - originalLine.Discount;
                            if (originalLineNetTotal == 0) originalLineCOGS = 0;
                            var originalLineFractionOfTransactionDiscount = (originalLine.SoldOrReturned * originalLineNetTotal / originalTransactionGrossTotal) * originalTransactionDiscount;
                            var originalLineFractionOfTransactionTax = (originalLine.SoldOrReturned * originalLineNetTotal / originalTransactionGrossTotal) * originalTransactionTax;
                            originalLineCOGS = (originalLine.SoldOrReturned * originalLineNetTotal) - originalLineFractionOfTransactionDiscount + originalLineFractionOfTransactionTax;

                            // Calculate the current line's COGS
                            decimal lineCOGS;
                            var lineNetTotal = (line.PurchasePrice - line.Discount) / line.Item.PiecesPerUnit;
                            if (lineNetTotal == 0) lineCOGS = 0;
                            var lineFractionOfTransactionDiscount = (line.SoldOrReturned * lineNetTotal / (decimal)_newTransactionGrossTotal)
                                * (_newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount);
                            var lineFractionOfTransactionTax = (line.SoldOrReturned * lineNetTotal / (decimal)_newTransactionGrossTotal)
                                * (_newTransactionTax == null ? 0 : (decimal)_newTransactionTax);
                            lineCOGS = (line.SoldOrReturned * lineNetTotal) - lineFractionOfTransactionDiscount + lineFractionOfTransactionTax;

                            totalValueChanged += (lineCOGS - originalLineCOGS);
                        }

                        if (Math.Round(originalLine.PurchasePrice, 2) != Math.Round((line.PurchasePrice / line.Item.PiecesPerUnit), 2) 
                            || Math.Round(originalLine.Discount, 2) != Math.Round((line.Discount / line.Item.PiecesPerUnit), 2))
                        {
                            linesChanged.Add(i);
                        }
                    }

                    foreach (var index in linesChanged)
                    {
                        var line = _lines[index];
                        var originalLine = originalLines[index];
                        line.PurchaseTransaction = transaction;
                        line.Item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                        line.Warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();
                        context.PurchaseTransactionLines.Remove(originalLine); 
                        context.PurchaseTransactionLines.Add(line.Model); // Add the modified line
                    }

                    if (changesMade)
                    {
                        var ledgerTransaction2 = new LedgerTransaction();
                        LedgerDBHelper.AddTransaction(context, ledgerTransaction2, UtilityMethods.GetCurrentDate().Date, _newTransactionID, "Purchase Transaction Adjustment (COGS)");
                        context.SaveChanges();

                        // Indicates there is an increase in COGS
                        if (totalValueChanged > 0)
                        {
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Cost of Goods Sold", "Debit", totalValueChanged);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Inventory", "Credit", totalValueChanged);
                        }

                        else
                        {
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Inventory", "Debit", -totalValueChanged);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Cost of Goods Sold", "Credit", -totalValueChanged);
                        }
                    }
                }

                Model = transaction;
                Model.DOID = _newTransactionDOID;
                Model.Date = _newTransactionDate;
                Model.DueDate = _newTransactionDueDate;
                Model.GrossTotal = _newTransactionGrossTotal == null ? 0 : (decimal)_newTransactionGrossTotal;
                Model.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
                Model.Tax = _newTransactionTax == null ? 0 : (decimal)_newTransactionTax;
                Model.Total = (decimal)_newTransactionNetTotal;
                Model.Note = _newTransactionNote;
                Model.Supplier = context.Suppliers.Where(e => e.ID == _newTransactionSupplier.ID).FirstOrDefault();
                var user = App.Current.FindResource("CurrentUser") as User;
                Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();

                context.SaveChanges();
                ts.Complete();

                MessageBox.Show("Transaction saved!", "Success", MessageBoxButton.OK);
            }

            // Reset the fields for a new transaction
            Model = new PurchaseTransaction();
            ResetFields();
            SetTransactionID();
            Model.Total = 0;
        }
        #endregion
    }
}
