namespace PutraJayaNT.ViewModels.Customers
{
    using MVVMFramework;
    using PutraJayaNT.Utilities;
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using PutraJayaNT.Models.Inventory;
    using PutraJayaNT.Models.Sales;
    using System.Transactions;
    using PutraJayaNT.Models.Accounting;
    using System.Data;
    using PutraJayaNT.Models;
    using PutraJayaNT.Views.Customers;
    using PutraJayaNT.Models.Salesman;
    using Microsoft.Reporting.WinForms;
    using System.Collections.Generic;
    using PutraJayaNT.Reports.Windows;

    public class SalesTransactionVM : ViewModelBase<SalesTransaction>
    {
        ObservableCollection<CustomerVM> _customers;
        ObservableCollection<ItemVM> _products;
        ObservableCollection<SalesTransactionLineVM> _salesTransactionLines;
        ObservableCollection<SalesTransactionLineVM> _deletedLines;
        ObservableCollection<Warehouse> _warehouses;
        ObservableCollection<Salesman> _salesmans;

        #region Transaction Backing Fields
        string _newTransactionID;
        DateTime _newTransactionDate;
        CustomerVM _newTransactionCustomer;
        string _newTransactionCustomerCity;
        string _newTransactionNotes;
        decimal? _newTransactionDiscountPercent;
        decimal? _newTransactionDiscount;
        decimal? _newTransactionSalesExpense;
        decimal _newTransactionGrossTotal;
        decimal _netTotal;
        ICommand _newTransactionCommand;
        ICommand _saveTransactionCommand;
        #endregion

        #region New Entry Backing Fields
        string _remainingStock;
        ItemVM _newEntryProduct;
        Warehouse _newEntryWarehouse;
        decimal? _newEntryPrice;
        decimal? _newEntryDiscountPercent;
        decimal? _newEntryDiscount;
        string _newEntryUnitName;
        int? _newEntryPiecesPerUnit;
        int? _newEntryUnits;
        int? _newEntryPieces;
        Salesman _newEntrySalesman;
        bool _newEntrySubmitted;
        ICommand _newEntryCommand;
        #endregion

        ICommand _browseCommand;
        ICommand _printListCommand;
        ICommand _printDOCommand;
        ICommand _printInvoiceCommand;
        ICommand _previewInvoiceCommand;
        ICommand _previewDOCommand;
        ICommand _issueInvoiceCommand;

        #region Edit Line Properties
        SalesTransactionLineVM _selectedLine;
        int _selectedIndex;
        ICommand _editLineCommand;
        int _editLineUnits;
        int _editLinePieces;
        decimal _editLineDiscount;
        decimal _editLineSalesPrice;
        Salesman _editLineSalesman;
        ICommand _editConfirmCommand;
        ICommand _editCancelCommand;
        bool _isEditWindowNotOpen;
        Visibility _editWindowVisibility;
        ICommand _deleteLineCommand;
        #endregion

        bool _editMode = false;
        bool _invoiceNotIssued = true;
        bool _invoiceNotPaid = true;
        CustomerVM _editCustomer;

        public SalesTransactionVM()
        {
            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;

             _customers = new ObservableCollection<CustomerVM>();
            _products = new ObservableCollection<ItemVM>();
            _salesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _deletedLines = new ObservableCollection<SalesTransactionLineVM>();
            _warehouses = new ObservableCollection<Warehouse>();
            _salesmans = new ObservableCollection<Salesman>();

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

        public bool InvoiceNotIssued
        {
            get { return _invoiceNotIssued; }
            set { SetProperty(ref _invoiceNotIssued, value, "InvoiceNotIssued"); }
        }

        public bool InvoiceNotPaid
        {
            get { return _invoiceNotPaid; }
            set { SetProperty(ref _invoiceNotPaid, value, "InvoiceNotPaid"); }
        }

        #region Collections
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

        public ObservableCollection<Salesman> Salesmans
        {
            get { return _salesmans; }
        }

        public ObservableCollection<SalesTransactionLineVM> SalesTransactionLines
        {
            get { return _salesTransactionLines; }
        }
        #endregion

        #region Line Properties
        public SalesTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value, "SelectedIndex"); }
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

        public Salesman EditLineSalesman
        {
            get { return _editLineSalesman; }
            set { SetProperty(ref _editLineSalesman, value, "EditLineSalesman"); }
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
                    var oldQuantity = (_selectedLine.Units * _selectedLine.Item.PiecesPerUnit) + _selectedLine.Pieces;
                    var newQuantity = (_editLineUnits * _selectedLine.Item.PiecesPerUnit) + _editLinePieces;
                    var quantityDifference = newQuantity - oldQuantity;

                    var availableQuantity = GetAvailableQuantity(_selectedLine.Item, _selectedLine.Warehouse);

                    if (availableQuantity - quantityDifference < 0)
                    {
                        MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces available.",
                            _selectedLine.Item.Name, (availableQuantity / _selectedLine.Item.PiecesPerUnit) + _selectedLine.Units,
                            (availableQuantity % _selectedLine.Item.PiecesPerUnit) + _selectedLine.Pieces),
                            "Insufficient Stock", MessageBoxButton.OK);
                        return;
                    }

                    var oldDiscount = _selectedLine.Discount;
                    var oldSalesPrice = _selectedLine.SalesPrice;
                    var oldSalesman = _selectedLine.Salesman;


                    if (oldDiscount != _editLineDiscount || oldSalesPrice != _editLineSalesPrice)
                    {
                        var deletedLine = _selectedLine.Clone();
                        _deletedLines.Add(deletedLine);
                    }

                    _selectedLine.Units = _editLineUnits;
                    _selectedLine.Pieces = _editLinePieces;
                    _selectedLine.Discount = _editLineDiscount;
                    _selectedLine.SalesPrice = _editLineSalesPrice;
                    _selectedLine.Salesman = _editLineSalesman;

                    // Run a check to see if this line can be combined with another line of the same in transaction
                    for (int i = 0; i < _salesTransactionLines.Count; i++)
                    {
                        var line = _salesTransactionLines[i];
                        if (line.Equals(_selectedLine) && i != _selectedIndex)
                        {
                            line.Quantity += _selectedLine.Quantity;
                            line.StockDeducted += _selectedLine.StockDeducted;

                            // Some operations for the removal to be correct
                            _selectedLine.Discount = oldDiscount;
                            _selectedLine.SalesPrice = oldSalesPrice;
                            _selectedLine.Salesman = oldSalesman;
                            _salesTransactionLines.Remove(_selectedLine);

                            break;
                        }
                    }

                    IsEditWindowNotOpen = true;
                    EditWindowVisibility = Visibility.Hidden;
                    OnPropertyChanged("NewTransactionGrossTotal");
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
                        EditLineSalesman = _selectedLine.Salesman;
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
                        _deletedLines.Add(_selectedLine);
                        _salesTransactionLines.Remove(_selectedLine);
                    }

                    OnPropertyChanged("NewTransactionGrossTotal");
                }));
            }
        }
        #endregion

        #region Transaction Properties
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
                        .Include("TransactionLines.Salesman")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("TransactionLines.Item.Stocks")
                        .Where(e => e.SalesTransactionID.Equals(value))
                        .FirstOrDefault();

                    if (transaction == null)
                    {
                        SetProperty(ref _newTransactionID, value, "NewTransactionID");
                        Model.SalesTransactionID = _newTransactionID;
                        return;
                    }

                    SetProperty(ref _newTransactionID, value, "NewTransactionID");

                    Model = transaction;
                    SetEditMode();
                }
            }
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

        public CustomerVM NewTransactionCustomer
        {
            get { return _newTransactionCustomer; }
            set
            {
                if ((_editCustomer == null && value != null && !value.Name.Equals("Kontan")) ||
                    (_editCustomer != null && !value.ID.Equals(_editCustomer.ID)))
                {
                    using (var context = new ERPContext())
                    {
                        var customerTransactions = context.SalesTransactions.Where(e => e.Customer.ID.Equals(value.ID) && e.Paid < e.Total).ToList();

                        var verified = false;
                        if (customerTransactions.Count > 5)
                        {
                            MessageBox.Show("This customer has maximum number of invoice(s).", "Invalid Customer", MessageBoxButton.OK);

                            // Verification
                            if (!UtilityMethods.GetVerification())
                            {
                                _newTransactionCustomer = null;
                                RefreshCustomers();
                                return;
                            }

                            verified = true;
                        }

                        if (customerTransactions.Count != 0 && !verified)
                        {
                            foreach (var t in customerTransactions)
                            {
                                if (t.DueDate < DateTime.Now.Date)
                                {
                                    MessageBox.Show("This customer has overdued invoice(s).", "Invalid Customer", MessageBoxButton.OK);
                                    
                                    // Verification
                                    if (UtilityMethods.GetVerification()) break;

                                    _newTransactionCustomer = null;
                                    RefreshCustomers();
                                    return;
                                }
                            }
                        }
                    }
                }

                SetProperty(ref _newTransactionCustomer, value, "NewTransactionCustomer");

                if (_newTransactionCustomer == null) return;

                NewTransactionCustomerCity = _newTransactionCustomer.City;
            }
        }

        public string NewTransactionCustomerCity
        {
            get { return _newTransactionCustomerCity; }
            set { SetProperty(ref _newTransactionCustomerCity, value, "NewTransactionCustomerCity"); }
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
                    }
                    #endregion

                    var context = new ERPContext();

                    if (EditMode)
                    {
                        SaveTransactionEditMode();
                    }

                    else
                    {
                        try
                        {
                            SaveNewTransaction(context);
                        }

                        catch (Exception e)
                        {
                            MessageBox.Show(e.InnerException.ToString(), "Error", MessageBoxButton.OK);
                            return;
                        }

                        finally
                        {
                            context.Dispose();
                        }
                    }

                    using (context)
                    {
                        Model = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Salesman")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("TransactionLines.Item.Stocks")
                        .Where(e => e.SalesTransactionID.Equals(_newTransactionID))
                        .FirstOrDefault();
                    }

                    SetEditMode();
                }));
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
                var remainingStock = GetAvailableQuantity(_newEntryProduct.Model, _newEntryWarehouse);
                RemainingStock = string.Format("{0}/{1}", remainingStock / _newEntryProduct.PiecesPerUnit, remainingStock % _newEntryProduct.PiecesPerUnit);
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

        public string RemainingStock
        {
            get { return _remainingStock; }
            set { SetProperty(ref _remainingStock, value, "RemainingStock"); }
        }

        public Salesman NewEntrySalesman
        {
            get { return _newEntrySalesman; }
            set { SetProperty(ref _newEntrySalesman, value, "NewEntrySalesman"); }
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
                    if (_newEntryProduct == null || _newEntryPrice == null || (_newEntryUnits == null && _newEntryPieces == null) || _newEntrySalesman == null)
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

                            line.Quantity += quantity; // LineVM automatically recalculates line total
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
                            Total = (((decimal)_newEntryPrice - discount) / _newEntryProduct.PiecesPerUnit) * quantity,
                            Salesman = _newEntrySalesman
                        }
                    };

                    _salesTransactionLines.Add(vm);

                    SubmitNewEntry();
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region Commands
        public ICommand BrowseCommand
        {
            get
            {
                return _browseCommand ?? (_browseCommand = new RelayCommand(() =>
                {
                    var browseWindow = new BrowseSalesTransactionsView();
                    browseWindow.Owner = App.Current.MainWindow;
                    browseWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    browseWindow.Show();
                }));
            }
        }

        public ICommand PrintListCommand
        {
            get
            {
                return _printListCommand ?? (_printListCommand = new RelayCommand(() =>
                {
                    var printListWindow = new PrintListView();
                    printListWindow.Owner = App.Current.MainWindow;
                    printListWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    printListWindow.Show();
                }));
            }
        }

        public ICommand PreviewInvoiceCommand
        {
            get
            {
                return _previewInvoiceCommand ?? (_previewInvoiceCommand = new RelayCommand(() =>
                {
                    if (_salesTransactionLines.Count == 0) return;

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID))
                        .FirstOrDefault();

                        if (transaction == null)
                        {
                            MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                            return;
                        }
                    }

                    var salesInvoiceWindow = new SalesInvoiceWindow(transaction);
                    salesInvoiceWindow.Owner = App.Current.MainWindow;
                    salesInvoiceWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    salesInvoiceWindow.ShowDialog();
                }));
            }
        }

        public ICommand PreviewDOCommand
        {
            get
            {
                return _previewDOCommand ?? (_previewDOCommand = new RelayCommand(() =>
                {
                    if (_salesTransactionLines.Count == 0) return;

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID))
                        .FirstOrDefault();

                        if (transaction == null)
                        {
                            MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                            return;
                        }
                    }

                    var salesDOWindow = new SalesDOWindow(transaction);
                    salesDOWindow.Owner = App.Current.MainWindow;
                    salesDOWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    salesDOWindow.ShowDialog();
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

                    if (MessageBox.Show("Confirm printing?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                    if (Model.DOPrinted == true)
                    {
                        if (!UtilityMethods.GetVerification()) return;
                    }

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID
                        .Equals(Model.SalesTransactionID))
                        .FirstOrDefault();

                        if (transaction == null)
                        {
                            MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                            return;
                        }

                        if (transaction.DOPrinted == false)
                        {
                            transaction.DOPrinted = true;
                            Model.DOPrinted = true;
                            context.SaveChanges();
                        }
                    }

                    List<LocalReport> localReports = CreateDOLocalReports();
                    PrintHelper printHelper = new PrintHelper();
                    try
                    {
                        foreach (var report in localReports)
                        {
                            printHelper.Run(report);
                        }
                    }

                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK);
                    }

                    finally
                    {
                        printHelper.Dispose();
                    }
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

                    if (MessageBox.Show("Confirm printing?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                    if (Model.InvoicePrinted == true)
                    {
                        if (!UtilityMethods.GetVerification()) return;
                    }

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID
                        .Equals(Model.SalesTransactionID))
                        .FirstOrDefault();

                        if (transaction == null)
                        {
                            MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                            return;
                        }

                        if (transaction.InvoicePrinted == false)
                        {
                            transaction.InvoicePrinted = true;
                            Model.InvoicePrinted = true;
                            context.SaveChanges();
                        }
                    }

                    LocalReport localReport = CreateInvoiceLocalReport();
                    PrintHelper printHelper = new PrintHelper();
                    try
                    {
                        printHelper.Run(localReport);
                    }

                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK);
                    }

                    finally
                    {
                        printHelper.Dispose();
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
                                        purchase.SoldOrReturned += tracker;
                                        if (purchaseLineNetTotal == 0) break;
                                        var fractionOfTransactionDiscount = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                                        var fractionOfTransactionTax = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                                        costOfGoodsSoldAmount += (tracker * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                                        break;
                                    }
                                    else if (tracker > availableQuantity)
                                    {
                                        purchase.SoldOrReturned += availableQuantity;
                                        tracker -= availableQuantity;
                                        if (purchaseLineNetTotal == 0) continue; 
                                        var fractionOfTransactionDiscount = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                                        var fractionOfTransactionTax = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                                        costOfGoodsSoldAmount += (availableQuantity * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                                     }
                                }
                            }
                            #endregion

                            // Recognise revenue recognitition at this point and record the corresponding journal entries
                            var transaction1 = new LedgerTransaction();
                            if (!LedgerDBHelper.AddTransaction(context, transaction1, DateTime.Now.Date, transaction.SalesTransactionID, "Sales Revenue")) return;
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, transaction1, string.Format("{0} Accounts Receivable", transaction.Customer.Name), "Debit", Model.Total);
                            LedgerDBHelper.AddTransactionLine(context, transaction1, "Sales Revenue", "Credit", Model.Total);
                            context.SaveChanges();

                            var transaction2 = new LedgerTransaction();
                            if (!LedgerDBHelper.AddTransaction(context, transaction2, DateTime.Now.Date, transaction.SalesTransactionID, "Cost of Goods Sold")) return;
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
        #endregion

        #region Helper Methods
        #region Update Collections Methods
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
                    _salesmans.Add(salesman);
            }
        }
        #endregion

        private void SetTransactionID()
        {
            InvoiceNotIssued = true;

            var month = _newTransactionDate.Month;
            var year = _newTransactionDate.Year;
            _newTransactionID = "M" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from SalesTransaction in context.SalesTransactions
                           where SalesTransaction.SalesTransactionID.CompareTo(_newTransactionID) >= 0
                           orderby SalesTransaction.SalesTransactionID descending
                           select SalesTransaction.SalesTransactionID);
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _newTransactionID = "M" + (Convert.ToInt64(lastTransactionID.Substring(1)) + 1).ToString();

            Model.SalesTransactionID = _newTransactionID;
            OnPropertyChanged("NewTransactionID");
        }

        private void ResetEntryFields()
        {
            OnPropertyChanged("NewTransactionGrossTotal");
            NewEntryPiecesPerUnit = null;
            NewEntryProduct = null;
            NewEntryPrice = null;
            NewEntryUnitName = null;
            NewEntryPieces = null;
            NewEntryUnits = null;
            RemainingStock = null;
            NewEntrySalesman = null;
            RemainingStock = null;
        }

        private void ResetTransaction()
        {
            _editCustomer = null;
            InvoiceNotIssued = true;
            InvoiceNotPaid = true;
            Model = new SalesTransaction();
            _salesTransactionLines.Clear();
            _deletedLines.Clear();
            ResetEntryFields();
            SetTransactionID();
            NewTransactionNotes = null;
            NewTransactionDiscount = null;
            NewTransactionSalesExpense = null;
            NewTransactionCustomer = null;
            NewTransactionCustomerCity = null;
            NewTransactionDate = DateTime.Now.Date;
            RefreshCustomers();
            ResetEntryFields();
            NewEntryWarehouse = null;
            _products.Clear();
            EditMode = false;
        }

        private void SubmitNewEntry()
        {
            NewEntrySubmitted = true;
            NewEntrySubmitted = false;
        }

        private void SetEditMode()
        {
            EditMode = true;
            _editCustomer = new CustomerVM { Model = Model.Customer };

            InvoiceNotIssued = Model.InvoiceIssued == null ? true : false;
            InvoiceNotPaid = Model.Paid == 0 ? true : false;
            NewTransactionDate = Model.When;
            NewTransactionCustomer = new CustomerVM { Model = Model.Customer };
            NewTransactionNotes = Model.Notes;

            NewTransactionSalesExpense = Model.SalesExpense;
            NewTransactionDiscount = Model.Discount;
            OnPropertyChanged("NewTransactionGrossTotal");

            _salesTransactionLines.Clear();
            _deletedLines.Clear();
            foreach (var line in Model.TransactionLines)
            {
                var vm = new SalesTransactionLineVM { Model = line, StockDeducted = line.Quantity };
                _salesTransactionLines.Add(vm);
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

            foreach (var line in _deletedLines)
            {
                availableQuantity += line.StockDeducted;
            }

            return availableQuantity;
        }

        private bool CheckWarehouseExistsInTransaction(int warehouseID)
        {
            foreach (var line in _salesTransactionLines)
            {
                if (line.Warehouse.ID.Equals(warehouseID))
                    return true;
            }
            return false;
        }

        private void SaveTransactionEditMode()
        {
            #region Invoice Issued
            if (Model.InvoiceIssued == null)
            {
                using (var context = new ERPContext())
                {

                    var transaction = context.SalesTransactions
                        .Include("Customer")
                        .Include("TransactionLines")
                        .Include("TransactionLines.Salesman")
                        .Include("TransactionLines.Warehouse")
                        .Include("TransactionLines.Item")
                        .Where(e => e.SalesTransactionID.Equals(_newTransactionID))
                        .FirstOrDefault();

                    var originalTransactionLines = transaction.TransactionLines.ToList();

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
                        line.Salesman = context.Salesmans.Where(e => e.ID.Equals(line.Salesman.ID)).FirstOrDefault();

                        // Retrieve the item's current stock from the database
                        var stock = context
                        .Stocks.Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID))
                        .FirstOrDefault();

                        var found = false;
                        foreach (var l in originalTransactionLines)
                        {
                            // Check if the line exists in the original transaction
                            if (line.Item.ItemID.Equals(l.Item.ItemID)
                             && line.Warehouse.ID.Equals(l.Warehouse.ID)
                             && line.SalesPrice.Equals(l.SalesPrice * l.Item.PiecesPerUnit)
                             && line.Discount.Equals(l.Discount * l.Item.PiecesPerUnit))
                            {
                                found = true;
                                var originalQuantity = l.Quantity;

                                // If there are more quantity than the original, minus the additional quantity from stock
                                if (line.Quantity > originalQuantity)
                                {
                                    stock.Pieces -= (line.Quantity - originalQuantity);
                                    if (stock.Pieces == 0) context.Stocks.Remove(stock);
                                }

                                // If there are lesser quantity than the original, add the additional quantity to stock
                                else if (line.Quantity < originalQuantity)
                                {
                                    if (stock != null) stock.Pieces += (originalQuantity - line.Quantity);
                                    else
                                    {
                                        var s = new Stock
                                        {
                                            Item = line.Item,
                                            Warehouse = line.Warehouse,
                                            Pieces = (originalQuantity - line.Quantity)
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

                        // If not found, minus stock and add the line to the transaction
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
                    foreach (var line in _deletedLines)
                    {
                        var item = context.Inventory
                        .Include("SalesTransactionLines")
                        .Where(e => e.ItemID.Equals(line.Item.ItemID))
                        .FirstOrDefault();

                        var stock = context
                        .Stocks.Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID))
                        .FirstOrDefault();

                        var deleted = true;
                        foreach (var l in _salesTransactionLines)
                        {
                            if (line.Item.ItemID.Equals(l.Model.Item.ItemID)
                            && line.Warehouse.ID.Equals(l.Model.Warehouse.ID)
                            && line.SalesPrice.Equals(l.SalesPrice)
                            && line.Discount.Equals(l.Discount))
                            {
                                deleted = false;
                                break;
                            }
                        }

                        // If item has been deleted, delete transaction line as well as increasing the item's stock
                        if (deleted)
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

                            foreach (var l in originalTransactionLines)
                            {
                                if (line.Item.ItemID.Equals(l.Item.ItemID)
                                    && line.Warehouse.ID.Equals(l.Warehouse.ID)
                                    && line.SalesPrice.Equals(l.SalesPrice * l.Item.PiecesPerUnit)
                                    && line.Discount.Equals(l.Discount * l.Item.PiecesPerUnit))
                                {
                                    transaction.TransactionLines.Remove(l);
                                    break;
                                }
                            }
                        }
                    }

                    transaction.When = _newTransactionDate;
                    transaction.DueDate = _newTransactionDate.AddDays(_newTransactionCustomer.CreditTerms);
                    transaction.Customer = context.Customers.Where(e => e.ID.Equals(_newTransactionCustomer.Model.ID)).FirstOrDefault();
                    transaction.Notes = _newTransactionNotes;
                    transaction.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
                    transaction.SalesExpense = _newTransactionSalesExpense == null ? 0 : (decimal)_newTransactionSalesExpense;
                    transaction.GrossTotal = _newTransactionGrossTotal;
                    transaction.Total = _netTotal;
                    transaction.When = _newTransactionDate;
                    transaction.DueDate = _newTransactionDate.AddDays(_newTransactionCustomer.CreditTerms);
                    transaction.InvoiceIssued = Model.InvoiceIssued;
                    var user = App.Current.FindResource("CurrentUser") as User;
                    transaction.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();

                    Model = transaction;

                    context.SaveChanges();
                    MessageBox.Show("Successfully saved.", "Success", MessageBoxButton.OK);
                }
            }
            #endregion

            else
            {
                using (var ts = new TransactionScope())
                {
                    var context = new ERPContext();

                    var transaction = context.SalesTransactions
                        .Include("Customer")
                        .Include("TransactionLines")
                        .Include("TransactionLines.Salesman")
                        .Include("TransactionLines.Warehouse")
                        .Include("TransactionLines.Item")
                        .Where(e => e.SalesTransactionID.Equals(_newTransactionID))
                        .FirstOrDefault();

                    var originalTransactionLines = transaction.TransactionLines.ToList();

                    if (transaction.Total != _netTotal)
                    {
                        // Adjust the Sales Revenue, AR, COGS for this transaction due to the price changes 
                        var transactionTotalDifference = _netTotal - transaction.Total;

                        var adjustmentLedgerTransaction1 = new LedgerTransaction();
                        LedgerDBHelper.AddTransaction(context, adjustmentLedgerTransaction1, DateTime.Now.Date, _newTransactionID, "Sales Revenue Adjustment");
                        context.SaveChanges();
                        if (transactionTotalDifference > 0)
                        {
                            LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction1, string.Format("{0} Accounts Receivable", transaction.Customer.Name), "Debit", transactionTotalDifference);
                            LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction1, "Sales Revenue", "Credit", transactionTotalDifference);
                        }

                        else
                        {
                            LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction1, "Sales Revenue", "Debit", -transactionTotalDifference);
                            LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction1, string.Format("{0} Accounts Receivable", transaction.Customer.Name), "Credit", -transactionTotalDifference);
                        }

                        //List<SalesTransactionLine> editedLines = new List<SalesTransactionLine>();
                        //foreach (var line in _salesTransactionLines)
                        //    editedLines.Add(line.Model);

                        //var COGSDifference = CalculateCOGS(editedLines) - CalculateCOGS(transaction.TransactionLines.ToList());

                        //var adjustmentLedgerTransaction2 = new LedgerTransaction();
                        //LedgerDBHelper.AddTransaction(context, adjustmentLedgerTransaction2, DateTime.Now.Date, _newTransactionID, "Sales Revenue Adjustment");
                        //context.SaveChanges();
                        //if (COGSDifference > 0)
                        //{
                        //    LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction2, "Cost of Goods Sold", "Debit", COGSDifference);
                        //    LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction2, "Inventory", "Credit", COGSDifference);
                        //}
                        //else
                        //{
                        //    LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction2, "Inventory", "Debit", -COGSDifference);
                        //    LedgerDBHelper.AddTransactionLine(context, adjustmentLedgerTransaction2, "Cost of Goods Sold", "Credit", -COGSDifference);
                        //}

                        foreach (var line in originalTransactionLines)
                        {
                            var found = false;
                            foreach (var l in _salesTransactionLines)
                            {
                                if (line.Item.ItemID.Equals(l.Item.ItemID)
                                    && line.Warehouse.ID.Equals(l.Warehouse.ID)
                                    && line.SalesPrice.Equals(l.SalesPrice / l.Item.PiecesPerUnit)
                                    && line.Discount.Equals(l.Discount / l.Item.PiecesPerUnit))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            // If not found, it means the line has been edited
                            if (!found)
                            {
                                context.SalesTransactionLines.Remove(line);
                            }
                        }

                        foreach (var line in _salesTransactionLines)
                        {
                            var found = false;
                            foreach (var l in originalTransactionLines)
                            {
                                if (line.Item.ItemID.Equals(l.ItemID)
                                    && line.Warehouse.ID.Equals(l.WarehouseID)
                                    && line.SalesPrice.Equals(l.SalesPrice * line.Item.PiecesPerUnit)
                                    && line.Discount.Equals(l.Discount * line.Item.PiecesPerUnit))
                                {
                                    found = true;
                                    break;
                                }
                            }

                            // If not found, add the edited line to the transaction
                            if (!found)
                            {
                                line.SalesTransaction = transaction;
                                line.Salesman = context.Salesmans.Where(e => e.ID.Equals(line.Salesman.ID)).FirstOrDefault();
                                line.Item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                                line.Warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();
                                context.SalesTransactionLines.Add(line.Model);
                            }
                        }

                        transaction.Notes = _newTransactionNotes;

                        transaction.GrossTotal = _newTransactionGrossTotal;
                        transaction.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
                        transaction.Total = _netTotal;

                        context.SaveChanges();
                        ts.Complete();
                    }
                }

                MessageBox.Show("Invoice successfully edited.", "Success", MessageBoxButton.OK);
            }
        }

        private void SaveNewTransaction(ERPContext context)
        {
            foreach (var line in _salesTransactionLines)
            {
                line.Item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                line.Warehouse = context.Warehouses.Where(e => e.ID == line.Warehouse.ID).FirstOrDefault();
                line.Salesman = context.Salesmans.Where(e => e.ID.Equals(line.Salesman.ID)).FirstOrDefault();

                var stock = context.Stocks
                .Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.WarehouseID == line.Warehouse.ID)
                .Include("Item")
                .Include("Item.Stocks")
                .FirstOrDefault();

                // Add the item line's model to the sales transaction if there is enough stock
                stock.Pieces -= line.Quantity;
                Model.TransactionLines.Add(line.Model);

                // Remove the stock entry if it is 0
                if (stock.Pieces == 0) context.Stocks.Remove(stock);
            }

            Model.SalesTransactionID = _newTransactionID;
            Model.Customer = context.Customers.Where(e => e.ID.Equals(_newTransactionCustomer.Model.ID)).FirstOrDefault();
            Model.Notes = _newTransactionNotes;
            Model.Discount = _newTransactionDiscount == null ? 0 : (decimal)_newTransactionDiscount;
            Model.SalesExpense = _newTransactionSalesExpense == null ? 0 : (decimal)_newTransactionSalesExpense;
            Model.GrossTotal = _newTransactionGrossTotal;
            Model.Total = _netTotal;
            Model.When = _newTransactionDate;
            Model.DueDate = _newTransactionDate.AddDays(_newTransactionCustomer.CreditTerms);
            var user = App.Current.FindResource("CurrentUser") as User;
            Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();

            context.SalesTransactions.Add(Model);
            context.SaveChanges();
            MessageBox.Show("Successfully saved.", "Success", MessageBoxButton.OK);
        }

        private decimal CalculateCOGS(List<SalesTransactionLine> lines)
        {
            var costOfGoodsSoldAmount = 0m;

            // Determine the COGS for each line
            foreach (var line in lines)
            {
                var itemID = line.Item.ItemID;
                var quantity = line.Quantity;

                using (var context = new ERPContext())
                {
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
                            purchase.SoldOrReturned += tracker;
                            if (purchaseLineNetTotal == 0) break;
                            var fractionOfTransactionDiscount = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                            var fractionOfTransactionTax = (tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                            costOfGoodsSoldAmount += (tracker * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                            break;
                        }
                        else if (tracker > availableQuantity)
                        {
                            purchase.SoldOrReturned += availableQuantity;
                            tracker -= availableQuantity;
                            if (purchaseLineNetTotal == 0) continue;
                            var fractionOfTransactionDiscount = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                            var fractionOfTransactionTax = (availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                            costOfGoodsSoldAmount += (availableQuantity * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                        }
                    }
                }
            } 

            return costOfGoodsSoldAmount;
        }

        #region Reports Creation Methods
        private LocalReport CreateInvoiceLocalReport()
        {
            SalesTransaction salesTransaction;
            using (var context = new ERPContext())
            {
                salesTransaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                        .Include("TransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID))
                        .FirstOrDefault();
            }

            var dt1 = new DataTable();
            var dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("LineNumber", typeof(int)));
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Units", typeof(int)));
            dt1.Columns.Add(new DataColumn("Pieces", typeof(int)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));

            int count = 1;
            foreach (var line in salesTransaction.TransactionLines)
            {
                var dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr1["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dr1["SalesPrice"] = line.SalesPrice * line.Item.PiecesPerUnit;
                dr1["Discount"] = line.Discount * line.Item.PiecesPerUnit;
                dr1["Total"] = line.Total;
                dt1.Rows.Add(dr1);
            }

            var dr2 = dt2.NewRow();
            dt2.Columns.Add(new DataColumn("InvoiceGrossTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceSalesExpense", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt2.Columns.Add(new DataColumn("Address", typeof(string)));
            dt2.Columns.Add(new DataColumn("InvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dt2.Columns.Add(new DataColumn("DueDate", typeof(string)));
            dt2.Columns.Add(new DataColumn("Notes", typeof(string)));
            dr2["InvoiceGrossTotal"] = salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = salesTransaction.Total;
            dr2["Customer"] = salesTransaction.Customer.Name;
            dr2["Address"] = salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = salesTransaction.SalesTransactionID;
            dr2["Date"] = salesTransaction.When.ToShortDateString();
            dr2["DueDate"] = salesTransaction.DueDate.ToShortDateString();
            dr2["Notes"] = salesTransaction.Notes;

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesInvoiceReport.rdlc"); // Path of the rdlc file
            localReport.DataSources.Add(reportDataSource1);
            localReport.DataSources.Add(reportDataSource2);

            return localReport;
        }
        
        private List<LocalReport> CreateDOLocalReports()
        {
            List<LocalReport> reports = new List<LocalReport>();

            if (CheckWarehouseExistsInTransaction(1))
                reports.Add(CreateDOLocalReport(1));

            if (CheckWarehouseExistsInTransaction(2))
                reports.Add(CreateDOLocalReport(2));

            if (CheckWarehouseExistsInTransaction(3))
                reports.Add(CreateDOLocalReport(3));

            if (CheckWarehouseExistsInTransaction(4))
                reports.Add(CreateDOLocalReport(4));

            return reports;
        }

        private LocalReport CreateDOLocalReport(int warehouseID)
        {
            var dt1 = new DataTable();
            var dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("LineNumber", typeof(int)));
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Units", typeof(int)));
            dt1.Columns.Add(new DataColumn("Pieces", typeof(int)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));

            dt2.Columns.Add(new DataColumn("InvoiceGrossTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceSalesExpense", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt2.Columns.Add(new DataColumn("Address", typeof(string)));
            dt2.Columns.Add(new DataColumn("InvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dt2.Columns.Add(new DataColumn("DueDate", typeof(string)));
            dt2.Columns.Add(new DataColumn("Notes", typeof(string)));
            dt2.Columns.Add(new DataColumn("Warehouse", typeof(string)));

            SalesTransaction salesTransaction;
            using (var context = new ERPContext())
            {
                salesTransaction = context.SalesTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Item")
                            .Include("TransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID))
                        .FirstOrDefault();
            }

            int count = 1;
            foreach (var line in salesTransaction.TransactionLines.Where(e => e.Warehouse.ID.Equals(warehouseID)).ToList())
            {
                var dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr1["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dt1.Rows.Add(dr1);
            }

            var dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = salesTransaction.Total;
            dr2["Customer"] = salesTransaction.Customer.Name;
            dr2["Address"] = salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = salesTransaction.SalesTransactionID;
            dr2["Date"] = salesTransaction.When.ToShortDateString();
            dr2["DueDate"] = salesTransaction.DueDate.ToShortDateString();
            dr2["Notes"] = salesTransaction.Notes;
            using (var context = new ERPContext())
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(warehouseID)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesDOReport.rdlc");
            localReport.DataSources.Add(reportDataSource1);
            localReport.DataSources.Add(reportDataSource2);

            return localReport;
        }
        #endregion
        #endregion
    }
}
