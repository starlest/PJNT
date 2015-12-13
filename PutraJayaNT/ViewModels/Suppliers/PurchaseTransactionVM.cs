using MVVMFramework;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using PutraJayaNT.Models.Accounting;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Transactions;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PurchaseTransactionVM : ViewModelBase<PurchaseTransaction>
    {
        ObservableCollection<PurchaseTransactionLineVM> _lines;
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<Item> _supplierItems;
        ObservableCollection<Warehouse> _warehouses;

        Supplier _newTransactionSupplier;

        Warehouse _newEntryWarehouse;
        string _newEntryID;
        DateTime _newEntryDate;
        DateTime _newEntryDueDate;
        Item _newEntryItem;
        string _newEntryUnitName;
        int _newEntryQuantity;
        int? _newEntryUnits;
        int? _newEntryPieces;
        int? _newEntryPiecesPerUnit;
        decimal? _newEntryPrice;
        decimal? _newEntryTotal;

        bool _newEntrySubmitted;

        ICommand _newEntryCommand;
        ICommand _confirmTransactionCommand;
        ICommand _newTransactionCommand;

        public PurchaseTransactionVM()
        {
            Model = new PurchaseTransaction();
            _lines = new ObservableCollection<PurchaseTransactionLineVM>();
            // Add an event handler for this collection
            _lines.CollectionChanged += OnCollectionChanged;
            _suppliers = new ObservableCollection<Supplier>();
            _supplierItems = new ObservableCollection<Item>();
            _warehouses = new ObservableCollection<Warehouse>();

            UpdateSuppliers();

            NewEntryDate = DateTime.Now;
            NewEntryDueDate = DateTime.Now;

            SetTransactionID();
            Model.DueDate = _newEntryDueDate;
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

        public Supplier NewTransactionSupplier
        {
            get { return _newTransactionSupplier; }
            set
            {
                SetProperty(ref _newTransactionSupplier, value, "NewTransactionSupplier");

                if (value == null)
                {
                    _supplierItems.Clear();                    
                    return;
                }

                if (_suppliers.Contains(value))
                {
                    // No longer allows user to select another supplier
                    _suppliers.Clear();
                    _suppliers.Add(value);

                    UpdateWarehouses();

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

                    OnPropertyChanged("SupplierItems");
                    Model.Supplier = value;
                }
            }
        }

        private void UpdateSuppliers()
        {
            _suppliers.Clear();

            using (var uow = new UnitOfWork())
            {
                var suppliers = uow.SupplierRepository.GetAll();
                foreach (var supplier in suppliers)
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

        public string NewEntryID
        {
            get { return _newEntryID; }
            set { SetProperty(ref _newEntryID, value, "NewEntryID"); }
        }

        public DateTime NewEntryDate
        {
            get { return _newEntryDate; }
            set
            {
                SetProperty(ref _newEntryDate, value, "NewEntryDate");
                Model.Date = _newEntryDate;
                SetTransactionID();
            }
        }

        public DateTime NewEntryDueDate
        {
            get { return _newEntryDueDate; }
            set
            {
                SetProperty(ref _newEntryDueDate, value, "NewEntryDueDate");
                Model.DueDate = _newEntryDueDate;
            }
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

        public decimal? NewEntryPrice
        {
            get { return _newEntryPrice; }
            set { SetProperty(ref _newEntryPrice, value, "NewEntryPrice"); }
        }

        public decimal? NewEntryTotal
        {
            get
            {
                _newEntryTotal = 0;
                foreach (var line in _lines)
                {
                    _newEntryTotal += line.Total;
                }
                return _newEntryTotal;
            }
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
                    _newEntryPrice /= _newEntryItem.PiecesPerUnit;

                    // Check if the item exists in one of the lines 
                    foreach (var l in _lines)
                    {
                        if (_newEntryItem.Equals(l.Item) && _newEntryWarehouse.ID.Equals(l.Warehouse.ID) && !_newEntryPrice.Equals(l.PurchasePrice))
                        {
                            l.Quantity += _newEntryQuantity;
                            ResetEntryFields();
                            return;
                        }
                    }

                    var line = new PurchaseTransactionLine
                    {
                        PurchasePrice = (decimal)_newEntryPrice,
                        Quantity = _newEntryQuantity,
                        Total = (decimal)_newEntryPrice * _newEntryQuantity,
                        Item = _newEntryItem,
                        Warehouse = _newEntryWarehouse,
                        PurchaseTransaction = Model
                    };

                    var lineVM = new PurchaseTransactionLineVM { Model = line };
                    _lines.Add(lineVM);

                    // Update Total Amount
                    OnPropertyChanged("NewEntryTotal");

                    ResetEntryFields();

                    NewEntrySubmitted = true;
                    NewEntrySubmitted = false;
                }));
            }
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
                            foreach (var line in Model.PurchaseTransactionLines)
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
                                    context.Stocks.Add(new Stock { Item = item, Warehouse = warehouse, Pieces = line.Quantity });
                                else
                                    stock.Pieces += line.Quantity;

                                line.Item = item;
                                line.Warehouse = warehouse;

                                Model.Total += line.Total;
                            }

                            Model.Supplier = context.Suppliers.Where(e => e.ID == Model.Supplier.ID).FirstOrDefault();
                            context.PurchaseTransactions.Add(Model);

                            // Record the accounting journal entry for this purchase
                            LedgerTransaction transaction = new LedgerTransaction();
                            string accountsPayableName = _newTransactionSupplier.Name + " Accounts Payable";

                            LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, _newEntryID.ToString(), "Purchase Transaction");
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

        private void SetTransactionID()
        {
            var month = _newEntryDate.Month;
            var year = _newEntryDate.Year;
            NewEntryID = "P" + ((long) ((year - 2000) * 100 + month) * 1000000).ToString();

            string lastEntryID = null;
            using(var context = new ERPContext())
            {
                var IDs = (from PurchaseTransaction in context.PurchaseTransactions
                           where PurchaseTransaction.PurchaseID.CompareTo(_newEntryID.ToString()) >= 0
                           orderby PurchaseTransaction.PurchaseID descending
                           select PurchaseTransaction.PurchaseID);
                if (IDs.Count() != 0) lastEntryID = IDs.First();
            }

            if (lastEntryID != null) NewEntryID = "P" + (Convert.ToInt64(lastEntryID.Substring(1)) + 1).ToString();

            Model.PurchaseID = _newEntryID;
        }

        private void ResetFields()
        {
            ResetEntryFields();

            NewTransactionSupplier = null;

            Model.Date = _newEntryDate;
            Model.DueDate = _newEntryDueDate;
            Model.Total = 0;

            _warehouses.Clear();
            _supplierItems.Clear();
            UpdateSuppliers();

            Model.PurchaseTransactionLines.Clear();
            _lines.Clear();

            OnPropertyChanged("Suppliers");
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
                    foreach (PurchaseTransactionLine l in Model.PurchaseTransactionLines)
                    {
                        if (l.Item.Name == line.Item.Name)
                        {
                            Model.PurchaseTransactionLines.Remove(l);
                            break;
                        }
                    }
                }
            }

            else if (e.NewItems != null)
            {
                // Suscribe an event handler to the line and
                // add the corresponding Transaction Line into the Model's collection too
                foreach (PurchaseTransactionLineVM line in e.NewItems)
                {
                    line.PropertyChanged += LinePropertyChangedHandler;
                    Model.PurchaseTransactionLines.Add(line.Model);
                }
            }

            // Refresh Total Amount
            OnPropertyChanged("NewEntryTotal");
        }

        void LinePropertyChangedHandler(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Total")
                OnPropertyChanged("NewEntryTotal");
        }
        #endregion
    }
}
