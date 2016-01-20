﻿using MVVMFramework;
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

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PurchaseTransactionVM : ViewModelBase<PurchaseTransaction>
    {
        ObservableCollection<PurchaseTransactionLineVM> _lines;
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<Item> _supplierItems;
        ObservableCollection<Warehouse> _warehouses;

        bool _notEditMode;

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

        public PurchaseTransactionVM()
        {
            _notEditMode = true;
            Model = new PurchaseTransaction();
            _lines = new ObservableCollection<PurchaseTransactionLineVM>();
            // Add an event handler for this collection
            _lines.CollectionChanged += OnCollectionChanged;
            _suppliers = new ObservableCollection<Supplier>();
            _supplierItems = new ObservableCollection<Item>();
            _warehouses = new ObservableCollection<Warehouse>();

            UpdateSuppliers();

            NewTransactionDate = DateTime.Now.Date;
            NewTransactionDueDate = DateTime.Now.Date;

            SetTransactionID();
            Model.DueDate = _newTransactionDueDate;
            Model.Total = 0;
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
                if (value > DateTime.Now.Date)
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
                        && _newEntryPrice.Equals(l.PurchasePrice)
                        && discount.Equals(l.Discount))
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

        public ICommand ConfirmTransactionCommand
        {
            get
            {
                return _confirmTransactionCommand ?? (_confirmTransactionCommand = new RelayCommand(() =>
                {
                    if (_lines.Count == 0)
                    {
                        MessageBox.Show("Please input at least one entry.", "Empty Transaction", MessageBoxButton.OK);
                        return;
                    }
                    
                    if (MessageBox.Show("Confirm Purchase?", "Confirmation", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope(TransactionScopeOption.Required))
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

                            Model.DOID = _newTransactionDOID;
                            Model.Date = _newTransactionDate;
                            Model.DueDate = _newTransactionDueDate;
                            Model.Total = (decimal) _newTransactionNetTotal;
                            Model.Tax = _newTransactionTax == null ? 0 : (decimal)_newTransactionTax;
                            Model.Discount = _newTransactionDiscount == null ? 0 : (decimal) _newTransactionDiscount;
                            Model.GrossTotal = _newTransactionGrossTotal == null ? 0 : (decimal) _newTransactionGrossTotal;
                            Model.Note = _newTransactionNote;
                            Model.Supplier = context.Suppliers.Where(e => e.ID == _newTransactionSupplier.ID).FirstOrDefault();
                            var user = App.Current.FindResource("CurrentUser") as User;
                            Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                            context.PurchaseTransactions.Add(Model);

                            // Record the accounting journal entry for this purchase
                            LedgerTransaction transaction = new LedgerTransaction();
                            string accountsPayableName = _newTransactionSupplier.Name + " Accounts Payable";

                            if (!LedgerDBHelper.AddTransaction(context, transaction, _newTransactionDate, _newTransactionID.ToString(), "Purchase Transaction")) return;
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, transaction, "Inventory", "Debit", Model.Total);
                            LedgerDBHelper.AddTransactionLine(context, transaction, accountsPayableName, "Credit", Model.Total);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        // Reset the fields for a new transaction
                        Model = new PurchaseTransaction();
                        ResetFields();
                        SetTransactionID();
                        Model.Total = 0;
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

        private void SetTransaction(PurchaseTransaction transaction)
        {
            Model = transaction;
            NewTransactionDOID = transaction.DOID;
            NewTransactionSupplier = null; // to prevent buggy outcome
            NewTransactionSupplier = transaction.Supplier;
            NewTransactionDate = transaction.Date;
            NewTransactionDueDate = transaction.DueDate;
            NewTransactionNote = transaction.Note;

            _lines.Clear();
            foreach (var line in transaction.PurchaseTransactionLines)
                _lines.Add(new PurchaseTransactionLineVM { Model = line });

            NewTransactionDiscount = transaction.Discount;
            NewTransactionTax = transaction.Tax;
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

            NotEditMode = true;

            Model = new PurchaseTransaction();
            NewTransactionSupplier = null;
            NewTransactionDOID = null;
            NewTransactionDiscount = null;
            NewTransactionNote = null;
            NewTransactionDate = DateTime.Now.Date;
            NewTransactionDueDate = DateTime.Now.Date;

            _warehouses.Clear();
            _supplierItems.Clear();
            _lines.Clear();
            UpdateSuppliers();

            OnPropertyChanged("Suppliers");
            OnPropertyChanged("NewTransactionGrossTotal");
            OnPropertyChanged("NewTransactionNetTotal");

            SetTransactionID();
        }

        #region Purchase Transaction Lines Event Handler
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                // Remove the corresponding Transaction Line in the Model's collection
                foreach (PurchaseTransactionLineVM line in e.OldItems)
                {
                    // Unsuscribe to event handler to prevent strong reference
                    line.PropertyChanged -= LinePropertyChangedHandler;
                }
            }

            else if (e.NewItems != null)
            {
                // Suscribe an event handler to the line
                foreach (PurchaseTransactionLineVM line in e.NewItems)
                {
                    line.PropertyChanged += LinePropertyChangedHandler;
                }
            }
        }

        void LinePropertyChangedHandler(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Total")
                OnPropertyChanged("NewTransactionGrossTotal");
        }
        #endregion
    }
}
