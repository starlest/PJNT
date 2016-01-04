using MVVMFramework;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Specialized;
using System.ComponentModel;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;
using System.Transactions;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Reports;
using System.Data;
using PutraJayaNT.Models;

namespace PutraJayaNT.ViewModels.Customers
{
    public class SalesTransactionVM : ViewModelBase<SalesTransaction>
    {
        ObservableCollection<CustomerVM> _customers;
        ObservableCollection<ItemVM> _products;
        ObservableCollection<SalesTransactionLineVM> _salesTransactionLines;
        ObservableCollection<SalesTransactionLineVM> _originalSalesTransactionLines;
        ObservableCollection<Warehouse> _warehouses;
        ObservableCollection<SalesmanVM> _salesmans;

        bool _invoiceNotIssued;

        string _newTransactionID;
        DateTime _newTransactionDate;
        CustomerVM _newTransactionCustomer;
        SalesmanVM _newTransactionSalesman;
        string _newTransactionNotes;
        decimal? _newTransactionDiscountPercent;
        decimal? _newTransactionDiscount;
        decimal? _newTransactionSalesExpense;
        decimal _newTransactionGrossTotal;
        decimal _netTotal;

        ItemVM _newEntryProduct;
        Warehouse _newEntryWarehouse;
        decimal? _newEntryPrice;
        decimal? _newEntryDiscountPercent;
        decimal? _newEntryDiscount;
        string _newEntryUnitName;
        int? _newEntryPiecesPerUnit;
        int? _newEntryUnits;
        int? _newEntryPieces;
        bool _newEntrySubmitted;
        ICommand _newEntryCommand;

        ICommand _newTransactionCommand;
        ICommand _saveTransactionCommand;
        ICommand _printDOCommand;
        ICommand _printInvoiceCommand;
        ICommand _issueInvoiceCommand;

        SalesTransactionLineVM _selectedLine;
        ICommand _editLineCommand;
        int _editLineUnits;
        int _editLinePieces;
        decimal _editLineDiscount;
        decimal _editLineSalesPrice;
        ICommand _editConfirmCommand;
        ICommand _editCancelCommand;
        bool _isEditWindowNotOpen;
        Visibility _editWindowVisibility;
        ICommand _deleteLineCommand;

        bool _editMode = false;
        
        public SalesTransactionVM()
        {
            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;

             _customers = new ObservableCollection<CustomerVM>();
            _products = new ObservableCollection<ItemVM>();
            _salesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _salesTransactionLines.CollectionChanged += OnCollectionChanged;
            _originalSalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _warehouses = new ObservableCollection<Warehouse>();
            _salesmans = new ObservableCollection<SalesmanVM>();

            Model = new SalesTransaction();
            _newTransactionDate = DateTime.Now.Date;
            Model.When = _newTransactionDate;
            Model.InvoiceIssued = null;
            SetTransactionID();

            RefreshCustomers();
            UpdateWarehouses();
            UpdateSalesmans();
        }

        public bool EditMode
        {
            get { return _editMode; }
            set { SetProperty(ref _editMode, value, "EditMode"); }
        }

        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
        }

        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
        }

        public ObservableCollection<Warehouse> Warehouses 
        {
            get { return _warehouses; }
        }

        public ObservableCollection<SalesmanVM> Salesmans
        {
            get { return _salesmans; }
        }

        public ObservableCollection<SalesTransactionLineVM> SalesTransactionLines
        {
            get { return _salesTransactionLines; }
        }

        public bool InvoiceNotIssued
        {
            get { return _invoiceNotIssued; }
            set { SetProperty(ref _invoiceNotIssued, value, "InvoiceNotIssued"); }
        }

        private void UpdateProducts()
        {
            _products.Clear();

            using (var context = new ERPContext())
            {
                var stocks = context.Stocks
                    .Include("Item")
                    .Include("Item.Stocks")
                    .Include("Warehouse")
                    .Where(e => e.Pieces > 0 && e.WarehouseID.Equals(_newEntryWarehouse.ID))
                    .OrderBy(e => e.Item.Name)
                    .ToList();

                foreach (var stock in stocks)
                    _products.Add(new ItemVM { Model = stock.Item });
            }
        }

        private void RefreshCustomers()
        {
            _customers.Clear();

            using (var context = new ERPContext())
            {
                var customers = context.Customers
                    .Where(e => e.Active == true)
                    .OrderBy(e => e.Name)
                    .Include("Group")
                    .ToList();

                foreach (var customer in customers)
                    _customers.Add(new CustomerVM { Model = customer });
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

        private void UpdateSalesmans()
        {
            _salesmans.Clear();

            using (var context = new ERPContext())
            {
                var salesmans = context.Salesmans.ToList();

                foreach (var salesman in salesmans)
                    _salesmans.Add(new SalesmanVM { Model = salesman });
            }
        }

        private void SetTransactionID()
        {
            InvoiceNotIssued = true;

            var month = _newTransactionDate.Month;
            var year = _newTransactionDate.Year;
            _newTransactionID = "S" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from SalesTransaction in context.SalesTransactions
                           where SalesTransaction.SalesTransactionID.CompareTo(_newTransactionID) >= 0
                           orderby SalesTransaction.SalesTransactionID descending
                           select SalesTransaction.SalesTransactionID);
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _newTransactionID = "S" + (Convert.ToInt64(lastTransactionID.Substring(1)) + 1).ToString();

            Model.SalesTransactionID = _newTransactionID;
            OnPropertyChanged("NewTransactionID");
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
                    var transaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("TransactionLines.Item.Stocks")
                        .Include("Salesman")
                        .Where(e => e.SalesTransactionID.Equals(value))
                        .FirstOrDefault();

                    if (transaction == null)
                    {
                        SetProperty(ref _newTransactionID, value, "NewTransactionID");
                        Model.SalesTransactionID = _newTransactionID;
                        return;
                    }

                    if (transaction.InvoiceIssued != null) InvoiceNotIssued = false;
                    else InvoiceNotIssued = true;

                    SetProperty(ref _newTransactionID, value, "NewTransactionID");

                    EditMode = true;
                    Model = transaction;
                    NewTransactionDate = transaction.When;
                    NewTransactionCustomer = new CustomerVM { Model = transaction.Customer };
                    NewTransactionSalesman = new SalesmanVM { Model = transaction.Salesman };
                    NewTransactionNotes = transaction.Notes;

                    _originalSalesTransactionLines.Clear();
                    _salesTransactionLines.Clear();
                    foreach (var line in Model.TransactionLines)
                    {
                        var vm = new SalesTransactionLineVM { Model = line, StockDeducted = line.Quantity };
                        _originalSalesTransactionLines.Add(vm);
                        vm.PropertyChanged += LinePropertyChangedHandler;
                        _salesTransactionLines.Add(vm);
                    }

                    OnPropertyChanged("NewTransactionGrossTotal");
                    NewTransactionSalesExpense = transaction.SalesExpense;
                    NewTransactionDiscount = transaction.Discount;
                }
            }
        }

        public DateTime NewTransactionDate
        {
            get { return _newTransactionDate; }
            set
            {
                SetProperty(ref _newTransactionDate, value, "NewTransactionDate");
            }
        }

        public CustomerVM NewTransactionCustomer
        {
            get { return _newTransactionCustomer; }
            set
            {
                if (_editMode == false && value != null)
                {
                    using (var context = new ERPContext())
                    {
                        var customerTransactions = context.SalesTransactions.Where(e => e.Customer.ID.Equals(value.ID) && e.Paid < e.Total).ToList();

                        if (customerTransactions.Count != 0)
                        {
                            foreach (var t in customerTransactions)
                            {
                                if (t.DueDate < DateTime.Now.Date)
                                {
                                    MessageBox.Show("This customer has overdued invoice(s).", "Invalid Customer", MessageBoxButton.OK);
                                    App.Current.MainWindow.IsEnabled = false;
                                    var window = new VerificationWindow();
                                    window.ShowDialog();
                                    App.Current.MainWindow.IsEnabled = true;
                                    var isVerified = App.Current.TryFindResource("IsVerified");
                                    if (isVerified != null) break;
                                    _newTransactionCustomer = null;
                                    RefreshCustomers();
                                    return;
                                }
                            }
                        }
                    }
                }

                SetProperty(ref _newTransactionCustomer, value, "NewTransactionCustomer");
            }
        }

        public SalesmanVM NewTransactionSalesman
        {
            get { return _newTransactionSalesman; }
            set { SetProperty(ref _newTransactionSalesman, value, "NewTransactionSalesman"); }
        }

        public string NewTransactionNotes
        {
            get { return _newTransactionNotes; }
            set { SetProperty(ref _newTransactionNotes, value, "NewTransactionNotes"); }
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
                    MessageBox.Show(string.Format("a Please enter a value from the range of 0 - {0}.", _newTransactionGrossTotal), "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionDiscount, value, "NewTransactionDiscount");
                OnPropertyChanged("NetTotal");
            }
        }

        public decimal? NewTransactionSalesExpense
        {
            get { return _newTransactionSalesExpense; }
            set
            {
                if (value != null && (value < 0))
                {
                    MessageBox.Show("Please enter a value greater than 0.", "Invalid Value", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newTransactionSalesExpense, value, "NewTransactionSalesExpense");
                OnPropertyChanged("NetTotal");
            }
        }

        public decimal NewTransactionGrossTotal
        {
            get
            {
                _newTransactionGrossTotal = 0;
                foreach (var line in _salesTransactionLines)
                    _newTransactionGrossTotal += line.Total;

                OnPropertyChanged("NetTotal");
                return _newTransactionGrossTotal;
            }
        }

        public decimal NetTotal
        {
            get
            {
                _netTotal = _newTransactionGrossTotal + (_newTransactionSalesExpense == null ? 0 : (decimal) _newTransactionSalesExpense) - (_newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount);
                return _netTotal;
            }
        }
        #endregion

        #region New Entry Properties
        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, "NewEntryProduct");

                if (_newEntryProduct == null) return;

                NewEntryPrice = _newEntryProduct.SalesPrice;
                NewEntryUnitName = _newEntryProduct.UnitName;
                NewEntryPiecesPerUnit = _newEntryProduct.PiecesPerUnit;
            }
        }

        public Warehouse NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set
            {
                SetProperty(ref _newEntryWarehouse, value, "NewEntryWarehouse");

                if (_newEntryWarehouse == null)
                {
                    _products.Clear();
                    return;
                }

                UpdateProducts();
            }
        }

        public decimal? NewEntryPrice
        {
            get { return _newEntryPrice; }
            set
            {
                SetProperty(ref _newEntryPrice, value, "NewEntryPrice");

                if (value == null) return;

                NewEntryDiscount = null;
            }
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
                    if (_newEntryProduct == null || _newEntryPrice == null || (_newEntryUnits == null && _newEntryPieces == null))
                    {
                        MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
                        return;
                    }

                    // Convert units and pieces into pieces
                    var quantity = (_newEntryPieces != null ? (int)_newEntryPieces : 0) +
                     (_newEntryUnits != null ? (int)_newEntryUnits * _newEntryProduct.PiecesPerUnit : 0);

                    var availableQuantity = GetAvailableQuantity(_newEntryProduct.Model, _newEntryWarehouse); 

                    if (availableQuantity < quantity)
                    {
                        MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                            _newEntryProduct.Name, availableQuantity / _newEntryProduct.PiecesPerUnit, availableQuantity % _newEntryProduct.PiecesPerUnit),
                            "Insufficient Stock", MessageBoxButton.OK);
                        return;
                    }

                    var discount = (_newEntryDiscount != null ? (decimal)_newEntryDiscount : 0);

                    // Check if the item has a line already
                    foreach (var line in _salesTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_newEntryProduct.Model.ItemID) 
                        && line.Warehouse.ID.Equals(_newEntryWarehouse.ID) 
                        && _newEntryPrice.Equals(line.SalesPrice)
                        && (_newEntryDiscount == null ? 0 : (decimal) _newEntryDiscount).Equals(line.Discount))
                        {
                            if (availableQuantity < quantity)
                            {
                                MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                                    line.Item.Name, (availableQuantity / line.Item.PiecesPerUnit), (availableQuantity % line.Item.PiecesPerUnit)),
                                    "Insufficient Stock", MessageBoxButton.OK);
                                return;
                            }

                            line.Quantity += quantity;
                            line.Total += quantity * (_newEntryProduct.Model.SalesPrice - (discount / line.Item.PiecesPerUnit));
                            ResetEntryFields();
                            return;
                        }
                    }

                    var vm = new SalesTransactionLineVM
                    {
                        Model = new SalesTransactionLine
                        {
                            Item = _newEntryProduct.Model,
                            SalesTransaction = Model,
                            SalesPrice = (decimal) _newEntryPrice / _newEntryProduct.PiecesPerUnit,
                            Quantity = quantity,
                            Warehouse = _newEntryWarehouse,
                            Discount = discount / _newEntryProduct.PiecesPerUnit,
                            Total = (((decimal)_newEntryPrice - discount) / _newEntryProduct.PiecesPerUnit) * quantity
                        }
                    };
                    vm.PropertyChanged += LinePropertyChangedHandler;
                    _salesTransactionLines.Add(vm);

                    SubmitNewEntry();
                    ResetEntryFields();
                }));
            }
        }

        private void ResetEntryFields()
        {
            OnPropertyChanged("NewTransactionSalesExpense");
            OnPropertyChanged("NewTransactionGrossTotal");
            NewEntryPiecesPerUnit = null;
            NewEntryProduct = null;
            NewEntryPrice = 0;
            NewEntryUnitName = null;
            NewEntryPieces = null;
            NewEntryUnits = null;
        }

        private void SubmitNewEntry()
        {
            NewEntrySubmitted = true;
            NewEntrySubmitted = false;
        }
        #endregion

        #region Line Properties
        public SalesTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        #region Edit Properties
        public int EditLineUnits
        {
            get { return _editLineUnits; }
            set { SetProperty(ref _editLineUnits, value, "EditLineUnits"); }
        }

        public int EditLinePieces
        {
            get { return _editLinePieces; }
            set { SetProperty(ref _editLinePieces, value, "EditLinePieces"); }
        }

        public decimal EditLineDiscount
        {
            get { return _editLineDiscount; }
            set { SetProperty(ref _editLineDiscount, value, "EditLineDiscount"); }
        }

        public decimal EditLineSalesPrice
        {
            get { return _editLineSalesPrice; }
            set { SetProperty(ref _editLineSalesPrice, value, "EditLineSalesPrice"); }
        }

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

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    var oldUnits = _selectedLine.Units;
                    var oldPieces = _selectedLine.Pieces;
                    var oldDiscount = _selectedLine.Discount;
                    var oldSalesPrice = _selectedLine.SalesPrice;

                    _selectedLine.Units = _editLineUnits;
                    _selectedLine.Pieces = _editLinePieces;
                    _selectedLine.Discount = _editLineDiscount;
                    _selectedLine.SalesPrice = _editLineSalesPrice;
                    var availableQuantity = GetAvailableQuantity(_selectedLine.Item, _selectedLine.Warehouse);

                    if (availableQuantity < 0)
                    {
                        MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces available.",
                            _selectedLine.Item.Name, (availableQuantity / _selectedLine.Item.PiecesPerUnit) + _selectedLine.Units, (availableQuantity % _selectedLine.Item.PiecesPerUnit) + _selectedLine.Pieces),
                            "Insufficient Stock", MessageBoxButton.OK);

                        _selectedLine.Units = oldUnits;
                        _selectedLine.Pieces = oldPieces;
                        _selectedLine.Discount = oldDiscount;
                        _selectedLine.SalesPrice = oldSalesPrice;
                        return;
                    }

                    // Run a check to see if this line can be combined with another line of the same in transaction
                    foreach (var line in _salesTransactionLines)
                    {
                        if (line != _selectedLine && line.Equals(_selectedLine))
                        {
                            line.Units = line.Units + _selectedLine.Units;
                            line.Pieces = line.Pieces + _selectedLine.Pieces;
                            line.StockDeducted += _selectedLine.StockDeducted;

                            // Some operations for the removal to be correct
                            _selectedLine.Discount = oldDiscount;
                            _selectedLine.SalesPrice = oldSalesPrice;
                            _salesTransactionLines.Remove(_selectedLine);

                            break;
                        }
                    }

                    IsEditWindowNotOpen = true;
                    EditWindowVisibility = Visibility.Hidden;
                }));
            }
        }

        public ICommand EditCancelCommand
        {
            get
            {
                return _editCancelCommand ?? (_editCancelCommand = new RelayCommand(() =>
                {
                    IsEditWindowNotOpen = true;
                    EditWindowVisibility = Visibility.Hidden;
                }));
            }
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

                        EditLineUnits = _selectedLine.Units;
                        EditLinePieces = _selectedLine.Pieces;
                        EditLineDiscount = _selectedLine.Discount;
                        EditLineSalesPrice = _selectedLine.SalesPrice;
                    }
                }));
            }
        }
        #endregion

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (_selectedLine != null && MessageBox.Show("Confirm Deletion?", "Confirmation", MessageBoxButton.YesNo)
                     == MessageBoxResult.Yes)
                    {
                        _salesTransactionLines.Remove(_selectedLine);
                     }

                }));
            }
        }
        #endregion

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
                    #region Checks
                    if (_newTransactionCustomer == null)
                    {
                        MessageBox.Show("Please select a customer.", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    if (_newTransactionSalesman == null)
                    {
                        MessageBox.Show("Please select a salesman.", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }
                    #endregion

                    if (MessageBox.Show("Confirm saving transaction?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // Check if there are enough stock for each line
                        foreach (var line in _salesTransactionLines)
                        {
                            var availableQuantity = GetAvailableQuantity(line.Item, line.Warehouse);

                            if (availableQuantity < 0)
                            {
                                MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces available.",
                                    line.Item.Name, (availableQuantity / line.Item.PiecesPerUnit) + line.Units, (availableQuantity % line.Item.PiecesPerUnit) + line.Pieces),
                                    "Insufficient Stock", MessageBoxButton.OK);
                                return;
                            }
                        }

                        var context = new ERPContext();

                        #region Edit Mode
                        if (EditMode)
                        {
                            try
                            {
                                var transaction = context.SalesTransactions
                                    .Where(e => e.SalesTransactionID.Equals(_newTransactionID))
                                    .FirstOrDefault();

                                foreach (var line in _salesTransactionLines)
                                {
                                    var item = context.Inventory
                                    .Include("SalesTransactionLines")
                                    .Include("SalesTransactionLines.Item")
                                    .Include("Category")
                                    .Where(e => e.ItemID.Equals(line.Item.ItemID))
                                    .FirstOrDefault();

                                    line.Warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();
                                    line.Item = item;

                                    var stock = context
                                    .Stocks.Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID))
                                    .FirstOrDefault();

                                    var found = false;
                                    // Check if the line exists in the original transaction
                                    foreach (var l in transaction.TransactionLines)
                                    {
                                        if (line.Item.ItemID.Equals(l.Item.ItemID)
                                         && line.Warehouse.ID.Equals(l.Warehouse.ID)
                                         && line.SalesPrice.Equals(l.SalesPrice * l.Item.PiecesPerUnit)
                                         && line.Discount.Equals(l.Discount * l.Item.PiecesPerUnit))
                                        {
                                            found = true;
                                            var originalQuantity = line.StockDeducted;

                                            // If there are more quantity than the original, minus the additional quantity from stock
                                            if (line.Quantity > originalQuantity)
                                            {
                                                if (stock == null || stock.Pieces - (line.Quantity - originalQuantity) < 0)
                                                {
                                                    MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                                                        item.Name, (stock.Pieces / item.PiecesPerUnit), (stock.Pieces % item.PiecesPerUnit)),
                                                        "Insufficient Stock", MessageBoxButton.OK);
                                                    return;
                                                }

                                                stock.Pieces -= line.Quantity - originalQuantity;
                                            }

                                            // If there are lesser quantity than the original, add the additional quantity to stock
                                            else if (line.Quantity < originalQuantity)
                                            {
                                                if (stock != null) stock.Pieces += originalQuantity - line.Quantity;
                                                else
                                                {
                                                    var s = new Stock
                                                    {
                                                        Item = line.Item,
                                                        Warehouse = line.Warehouse,
                                                        Pieces = originalQuantity - line.Quantity
                                                    };
                                                    context.Stocks.Add(s);
                                                }
                                            }

                                            l.Quantity = line.Quantity;
                                            l.SalesPrice = line.SalesPrice / line.Item.PiecesPerUnit;
                                            l.Discount = line.Discount / line.Item.PiecesPerUnit;
                                            l.Total = l.Quantity * (l.SalesPrice - l.Discount);
                                            break;
                                        }
                                    }

                                    // If not found, minus the stock from the Items and add the line to the transaction
                                    if (!found)
                                    {
                                        if (stock.Pieces - line.Quantity < 0)
                                        {
                                            MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                                                item.Name, (stock.Pieces / item.PiecesPerUnit), (stock.Pieces % item.PiecesPerUnit)),
                                                "Insufficient Stock", MessageBoxButton.OK);
                                            return;
                                        }

                                        stock.Pieces -= line.Quantity;
                                        line.SalesTransaction = transaction;
                                        context.SalesTransactionLines.Add(line.Model);
                                    }

                                    // Remove the stock entry if it is 0
                                    if (stock != null && stock.Pieces == 0) context.Stocks.Remove(stock);
                                }

                                // Check if there are items deleted
                                foreach (var line in transaction.TransactionLines.ToList())
                                {
                                    var item = context.Inventory
                                    .Include("SalesTransactionLines")
                                    .Where(e => e.ItemID.Equals(line.Item.ItemID))
                                    .FirstOrDefault();

                                    var stock = context
                                    .Stocks.Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID))
                                    .FirstOrDefault();

                                    var found = false;
                                    foreach (var l in _salesTransactionLines)
                                    {
                                        if (line.Item.ItemID.Equals(l.Model.Item.ItemID)
                                        && line.Warehouse.ID.Equals(l.Model.Warehouse.ID)
                                        && line.SalesPrice.Equals(l.Model.SalesPrice)
                                        && line.Discount.Equals(l.Model.Discount))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }

                                    // If item has been deleted, delete transaction line as well as increasing the item's stock
                                    if (!found)
                                    {
                                        if (stock != null) stock.Pieces += line.Quantity;
                                        else
                                        {
                                            line.Item = item;
                                            line.Warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();
                                            var s = new Stock
                                            {
                                                Item = line.Item,
                                                Warehouse = line.Warehouse,
                                                Pieces = line.Quantity
                                            };
                                            context.Stocks.Add(s);
                                        }
                                        transaction.TransactionLines.Remove(line);
                                    }
                                }

                                transaction.When = _newTransactionDate;
                                transaction.DueDate = _newTransactionDate.AddDays(_newTransactionCustomer.CreditTerms);

                                transaction.Customer = context.Customers
                                .Where(e => e.ID.Equals(_newTransactionCustomer.Model.ID))
                                .FirstOrDefault();

                                transaction.Salesman = context.Salesmans
                                .Where(e => e.ID.Equals(_newTransactionSalesman.ID))
                                .FirstOrDefault();

                                transaction.Notes = _newTransactionNotes;
                                transaction.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
                                transaction.SalesExpense = _newTransactionSalesExpense == null ? 0 : (decimal) _newTransactionSalesExpense;
                                transaction.GrossTotal = _newTransactionGrossTotal;
                                transaction.Total = _netTotal;
                                transaction.When = _newTransactionDate;
                                transaction.DueDate = _newTransactionDate.AddDays(_newTransactionCustomer.CreditTerms);

                                transaction.InvoiceIssued = Model.InvoiceIssued;

                                // Save changes
                                context.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.InnerException.ToString(), "Error", MessageBoxButton.OK);
                                return;
                            }
                        }
                        #endregion

                        #region New Transaction
                        else
                        {
                            try
                            {
                                Model.Total = 0;
                                foreach (var line in _salesTransactionLines)
                                {
                                    line.Item = context.Inventory
                                    .Where(e => e.ItemID.Equals(line.Item.ItemID))
                                    .FirstOrDefault();

                                    var stock = context.Stocks
                                    .Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.WarehouseID == line.Warehouse.ID)
                                    .Include("Item")
                                    .Include("Item.Stocks")
                                    .FirstOrDefault();

                                    // Check if there are enough stock
                                    if (stock == null || stock.Pieces < line.Quantity)
                                    {
                                        MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                                            stock.Item.Name, (stock.Pieces / stock.Item.PiecesPerUnit), (stock.Pieces % stock.Item.PiecesPerUnit)),
                                            "Insufficient Stock", MessageBoxButton.OK);
                                        return;
                                    }

                                    // Add the item line's model to the sales transaction if there is enough stock
                                    stock.Pieces -= line.Quantity;
                                    line.Warehouse = context.Warehouses.Where(e => e.ID == line.Warehouse.ID).FirstOrDefault();
                                    Model.TransactionLines.Add(line.Model);

                                    // Remove the stock entry if it is 0
                                    if (stock.Pieces == 0) context.Stocks.Remove(stock);
                                }

                                Model.SalesTransactionID = _newTransactionID;
                                Model.Customer = context.Customers
                                .Where(e => e.ID.Equals(_newTransactionCustomer.Model.ID))
                                .FirstOrDefault();

                                Model.Salesman = context.Salesmans
                                .Where(e => e.ID.Equals(_newTransactionSalesman.ID))
                                .FirstOrDefault();

                                Model.Notes = _newTransactionNotes;
                                Model.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
                                Model.SalesExpense = _newTransactionSalesExpense == null ? 0 : (decimal)_newTransactionSalesExpense;
                                Model.GrossTotal = _newTransactionGrossTotal;
                                Model.Total = _netTotal;
                                Model.When = _newTransactionDate;
                                Model.DueDate = _newTransactionDate.AddDays(_newTransactionCustomer.CreditTerms);

                                context.SalesTransactions.Add(Model);
                                context.SaveChanges();
                            }
                            catch(Exception e)
                            {
                                MessageBox.Show(e.InnerException.ToString(), "Error", MessageBoxButton.OK);
                                return;
                            }
                        }
                        #endregion

                        MessageBox.Show("Successfully saved.", "Success", MessageBoxButton.OK);
                    }
                }));
            }
        }

        public ICommand IssueInvoiceCommand
        {
            get
            {
                return _issueInvoiceCommand ?? (_issueInvoiceCommand = new RelayCommand(() =>
                {
                    if (_editMode == false)
                    {
                        MessageBox.Show("Please save the transaction first.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (Model.InvoiceIssued != null)
                    {
                        MessageBox.Show("Invoice has been issued.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm issuing invoice?", "Confirmation", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            var transaction = context.SalesTransactions
                            .Include("Customer")
                            .Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID))
                            .FirstOrDefault();

                            transaction.InvoiceIssued = DateTime.Now.Date;
                            var user = App.Current.TryFindResource("CurrentUser") as User;
                            if (user != null) transaction.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                            Model = transaction;

                            #region Calculate Cost of Goods Sold
                            decimal costOfGoodsSoldAmount = 0;

                            // Determine the COGS for each line
                            foreach (var line in transaction.TransactionLines.ToList())
                            {
                                var itemID = line.Item.ItemID;
                                var quantity = line.Quantity;

                                var purchases = context.PurchaseTransactionLines
                                .Include("PurchaseTransaction")
                                .Where(e => e.ItemID == itemID && e.SoldOrReturned < e.Quantity)
                                .OrderBy(e => e.PurchaseTransaction.PurchaseID)
                                .ToList();

                                var tracker = line.Quantity;

                                foreach (var purchase in purchases)
                                {
                                    var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                                    var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;

                                    if (tracker <= availableQuantity)
                                    {
                                        var fractionOfTransactionDiscount = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                                        costOfGoodsSoldAmount += (tracker * purchaseLineNetTotal) - fractionOfTransactionDiscount;
                                        purchase.SoldOrReturned += tracker;
                                        break;
                                    }
                                    else if (tracker > availableQuantity)
                                    {
                                        var fractionOfTransactionDiscount = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                                        costOfGoodsSoldAmount += (availableQuantity * purchaseLineNetTotal) - fractionOfTransactionDiscount;
                                        purchase.SoldOrReturned += availableQuantity;
                                        tracker -= availableQuantity;
                                    }
                                }
                            }
                            #endregion

                            // Recognise revenue recognitition at this point and record the corresponding journal entries
                            var transaction1 = new LedgerTransaction();
                            LedgerDBHelper.AddTransaction(context, transaction1, DateTime.Now.Date, transaction.SalesTransactionID, "Sales Revenue");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, transaction1, string.Format("{0} Accounts Receivable", transaction.Customer.Name), "Debit", Model.Total);
                            LedgerDBHelper.AddTransactionLine(context, transaction1, "Sales Revenue", "Credit", Model.Total);
                            context.SaveChanges();

                            var transaction2 = new LedgerTransaction();
                            LedgerDBHelper.AddTransaction(context, transaction2, DateTime.Now.Date, transaction.SalesTransactionID, "Cost of Goods Sold");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, transaction2, "Cost of Goods Sold", "Debit", costOfGoodsSoldAmount);
                            LedgerDBHelper.AddTransactionLine(context, transaction2, "Inventory", "Credit", costOfGoodsSoldAmount);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        InvoiceNotIssued = false;
                    }
                }));
            }
        }

        public ICommand PrintDOCommand
        {
            get
            {
                return _printDOCommand ?? (_printDOCommand = new RelayCommand(() =>
                {
                    if (_salesTransactionLines.Count == 0) return;

                    var salesDOWindow = new SalesDOWindow(this);
                    salesDOWindow.Owner = App.Current.MainWindow;
                    salesDOWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    salesDOWindow.Show();
                }));
            }
        }

        public ICommand PrintInvoiceCommand
        {
            get
            {
                return _printInvoiceCommand ?? (_printInvoiceCommand = new RelayCommand(() =>
                {
                    if (_salesTransactionLines.Count == 0) return;

                    var salesInvoiceWindow = new SalesInvoiceWindow(this);
                    salesInvoiceWindow.Owner = App.Current.MainWindow;
                    salesInvoiceWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    salesInvoiceWindow.Show();
                }));
            }
        }

        private void ResetTransaction()
        {
            InvoiceNotIssued = true;
            Model = new SalesTransaction();
            _salesTransactionLines.Clear();
            ResetEntryFields();
            SetTransactionID();
            NewTransactionNotes = null;
            NewTransactionDiscount = null;
            NewTransactionSalesExpense = null;
            NewTransactionCustomer = null;
            NewTransactionDate = DateTime.Now.Date;
            NewTransactionSalesman = null;
            RefreshCustomers();
            ResetEntryFields();
            NewEntryWarehouse = null;
            _products.Clear();
            EditMode = false;
        }

        /// <summary>
        /// Get the stock currently available.
        /// </summary>
        /// <param name="item"> The item to check. </param>
        /// <returns> The value of stock. </returns>
        private int GetStock(Item item)
        {
            int s = 0;
            using (var context = new ERPContext())
            {
                var stocks = context
                    .Stocks
                    .Where(e => e.Item.ItemID.Equals(item.ItemID))
                    .ToList();

                foreach (var stock in stocks)
                    s += stock.Pieces;
            }
            return s;
        }

        /// <summary>
        /// Get the stock currently available.
        /// </summary>
        /// <param name="item"> The item to check. </param>
        /// <param name="warehouse"> The warehouse the item is located at. </param>
        /// <returns> The value of stock. </returns>
        private int GetStock(Item item, Warehouse warehouse)
        {
            int s = 0;
            using (var context = new ERPContext())
            {
                var stock = context
                    .Stocks.Where(e => e.Item.ItemID.Equals(item.ItemID) && e.Warehouse.ID.Equals(warehouse.ID))
                    .FirstOrDefault();
                s = stock != null ? stock.Pieces : 0;
            }
            return s;
        }

        /// <summary>
        /// Compute the available quantity of the item taking into account the stock 
        /// and existing lines containing the item too.
        /// </summary>
        /// <param name="item"> The item to check. </param>
        /// <param name="warehouse"> The warehouse the item is located at. </param>
        /// <returns> The available quantity. </returns>
        private int GetAvailableQuantity(Item item, Warehouse warehouse)
        {
            var availableQuantity = GetStock(item, warehouse);

            // Decrease availableQuantity by the number of items from the same warehouse that are already in the transaction
            foreach (var line in _salesTransactionLines)
            {
                if (line.Item.ItemID.Equals(item.ItemID)
                && line.Warehouse.ID.Equals(warehouse.ID))
                {
                    availableQuantity += line.StockDeducted;
                    availableQuantity -= line.Quantity;
                }
            }

            return availableQuantity;
        }

        #region Sales Transaction Lines Event Handlers
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                // Remove the corresponding Transaction Line in the Model's collection
                foreach (SalesTransactionLineVM line in e.OldItems)
                {
                    // Unsuscribe to event handler to prevent strong reference
                    line.PropertyChanged -= LinePropertyChangedHandler;
                    foreach (SalesTransactionLine l in Model.TransactionLines)
                    {
                        if (l.Item.ItemID.Equals(line.Item.ItemID) &&
                            l.Warehouse.ID.Equals(line.Warehouse.ID) &&
                            l.Discount.Equals(line.Discount) &&
                            l.SalesPrice.Equals(line.SalesPrice))
                        {
                            Model.TransactionLines.Remove(l);
                            break;
                        }
                    }
                }
            }

            else if (e.NewItems != null)
            {
                // Suscribe an event handler to the line and
                // add the corresponding Transaction Line into the Model's collection too
                foreach (SalesTransactionLineVM line in e.NewItems)
                {
                    line.PropertyChanged += LinePropertyChangedHandler;
                    Model.TransactionLines.Add(line.Model);
                }
            }

            // Refresh Total Amount
            OnPropertyChanged("NewTransactionGrossTotal");
        }

        void LinePropertyChangedHandler(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Total")
            {
                OnPropertyChanged("NewTransactionGrossTotal");
            }

            else if (e.PropertyName == "Pieces" || e.PropertyName == "Units" || e.PropertyName == "Quantity")
            {
                OnPropertyChanged("NewTransactionSalesExpense");
            }
        }
        #endregion 
    }
}
