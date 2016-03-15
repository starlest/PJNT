namespace PutraJayaNT.ViewModels.Customers.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using Microsoft.Reporting.WinForms;
    using Models;
    using Models.Accounting;
    using Models.Inventory;
    using Models.Sales;
    using Models.Salesman;
    using MVVMFramework;
    using PutraJayaNT.Reports.Windows;
    using Utilities;
    using Utilities.ModelHelpers;
    using ViewModels.Sales;
    using Views.Customers;

    public class SalesVM : ViewModelBase<SalesTransaction>
    {
        private readonly ObservableCollection<SalesTransactionLineVM> _deletedLines;

        #region Transaction Backing Fields
        private string _transactionID;
        private DateTime _transactionDate;
        private CustomerVM _transactionCustomer;
        private string _transactionCustomerCity;
        private string _transactionNotes;
        private decimal? _transactionDiscountPercent;
        private decimal _transactionDiscount;
        private decimal _transactionSalesExpense;
        private decimal _transactionGrossTotal;
        private decimal _transactionNetTotal;
        private ICommand _newTransactionCommand;
        private ICommand _deleteTransactionCommand;
        private ICommand _saveTransactionCommand;
        #endregion

        #region Command Backing Fields
        private ICommand _browseCommand;
        private ICommand _printListCommand;
        private ICommand _printDOCommand;
        private ICommand _printInvoiceCommand;
        private ICommand _previewInvoiceCommand;
        private ICommand _previewDOCommand;
        private ICommand _issueInvoiceCommand;
        #endregion

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

        private bool _editMode;
        private bool _invoiceNotIssued = true;
        private bool _invoiceNotPaid = true;
        private bool _salesNotReturned = true;
        private CustomerVM _editCustomer;

        public SalesVM()
        {
            NewEntryVM = new SalesNewEntryVM(this);
            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;

            Customers = new ObservableCollection<CustomerVM>();
            SalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _deletedLines = new ObservableCollection<SalesTransactionLineVM>();

            Model = new SalesTransaction();
            _transactionDate = UtilityMethods.GetCurrentDate().Date;
            Model.Date = _transactionDate;
            Model.InvoiceIssued = null;
            SetTransactionID();

            UpdateCustomers();
        }

        public SalesNewEntryVM NewEntryVM { get; }

        public bool EditMode
        {
            get { return _editMode; }
            set { SetProperty(ref _editMode, value, "EditMode"); }
        }

        public bool InvoiceNotPaid
        {
            get { return _invoiceNotPaid; }
            set { SetProperty(ref _invoiceNotPaid, value, () => InvoiceNotPaid); }
        }

        public bool InvoiceNotIssued
        {
            get { return _invoiceNotIssued; }
            set { SetProperty(ref _invoiceNotIssued, value, "InvoiceNotIssued"); }
        }

        public bool SalesNotReturned
        {
            get { return _salesNotReturned; }
            set { SetProperty(ref _salesNotReturned, value, () => SalesNotReturned); }
        }

        #region Collections
        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<SalesTransactionLineVM> SalesTransactionLines { get; }
        #endregion

        #region Transaction Properties
        public string TransactionID
        {
            get { return _transactionID; }
            set
            {
                var transactionFromDatabase = GetSalesTransactionFromDatabase(value);
                if (transactionFromDatabase == null)
                {
                    MessageBox.Show("Sales transaction could not be found.", "Invalid ID", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _transactionID, value, () => TransactionID);
                NewEntryVM.ResetEntryFields();
                NewEntryVM.NewEntryWarehouse = null;
                Model = transactionFromDatabase;
                SetEditMode();
            }
        }
        
        public CustomerVM TransactionCustomer
        {
            get { return _transactionCustomer; }
            set
            {
                if (value != null && ((_editCustomer == null  && !value.Name.Equals("Kontan")) || (_editCustomer != null && !value.ID.Equals(_editCustomer.ID))))
                {
                    if (DoesCustomerHasMaximumNumberOfInvoices(value) && DoesCustomerHasOverduedInvoices(value))
                    {
                        if (!UtilityMethods.GetVerification()) return;
                    }
                }

                SetProperty(ref _transactionCustomer, value, () => TransactionCustomer);
                if (_transactionCustomer == null) return;
                TransactionCustomerCity = _transactionCustomer.City;
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

        public string TransactionCustomerCity
        {
            get { return _transactionCustomerCity; }
            set { SetProperty(ref _transactionCustomerCity, value, () => TransactionCustomerCity); }
        }

        public string TransactionNotes
        {
            get { return _transactionNotes; }
            set { SetProperty(ref _transactionNotes, value, () => TransactionNotes); }
        }

        public decimal? TransactionDiscountPercent
        {
            get { return _transactionDiscountPercent; }
            set
            {
                if (value != null && (value < 0 || value > 100))
                {
                    MessageBox.Show("Please enter a value from the range of 0 - 100.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionDiscountPercent, value, () => TransactionDiscountPercent);

                if (_transactionDiscountPercent == null) return;

                TransactionDiscount = (decimal)_transactionDiscountPercent / 100 * _transactionGrossTotal;
                TransactionDiscountPercent = null;
            }
        }

        public decimal TransactionDiscount
        {
            get { return _transactionDiscount; }
            set
            {
                if (value < 0 || value > _transactionGrossTotal)
                {
                    MessageBox.Show($"a Please enter a value from the range of 0 - {_transactionGrossTotal}.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionDiscount, value, () => TransactionDiscount);
                OnPropertyChanged("TransactionNetTotal");
            }
        }

        public decimal TransactionSalesExpense
        {
            get { return _transactionSalesExpense; }
            set
            {
                if (value < 0)
                {
                    MessageBox.Show("Please enter a value greater than 0.", "Invalid Value", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionSalesExpense, value, () => TransactionSalesExpense);
                OnPropertyChanged("TransactionNetTotal");
            }
        }

        public decimal TransactionGrossTotal
        {
            get
            {
                _transactionGrossTotal = 0;
                foreach (var line in SalesTransactionLines)
                    _transactionGrossTotal += line.Total;

                OnPropertyChanged("TransactionNetTotal");
                return _transactionGrossTotal;
            }
        }

        public decimal TransactionNetTotal
        {
            get
            {
                _transactionNetTotal = Math.Round(_transactionGrossTotal + _transactionSalesExpense - _transactionDiscount, 2);
                return _transactionNetTotal;
            }
        }
        #endregion

        #region Transaction Commands
        public ICommand NewTransactionCommand => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));

        public ICommand DeleteTransactionCommand
        {
            get
            {
                return _deleteTransactionCommand ?? (_deleteTransactionCommand = new RelayCommand(() =>
                {
                    if (!_editMode) return;

                    if (MessageBox.Show("Confirm deleting this transaction?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                    var _user = Application.Current.FindResource("CurrentUser") as User;
                    if (_user != null && _user.CanDeleteInvoice && UtilityMethods.GetVerification())
                    {
                        DeleteInvoice();
                        MessageBox.Show("Successfully deleted transaction!", "Success", MessageBoxButton.OK);
                        ResetTransaction();
                    }

                    else
                        MessageBox.Show("You are not authorised to delete transactions!", "Invalid User", MessageBoxButton.OK);
                }));
            }
        }

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(() =>
                {
                    if (!IsConfirmationYes() || !IsTransactionCustomerSelected() || !AreThereEnoughStockForAllTransactionItems()) return;
                    if (_editMode)
                        SaveEditedTransaction();
                    else
                        SaveNewTransaction();
                    MessageBox.Show("Successfully saved transaction.", "Success", MessageBoxButton.OK);
                    Model = GetSalesTransactionFromDatabase(_transactionID); 
                    SetEditMode();
                }));
            }
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

                    _selectedLine.Quantity = _editLineUnits * _selectedLine.Item.PiecesPerUnit + _editLinePieces;
                    _selectedLine.Discount = _editLineDiscount;
                    _selectedLine.SalesPrice = _editLineSalesPrice;
                    _selectedLine.Salesman = _editLineSalesman;

                    // Run a check to see if this line can be combined with another line of the same in transaction
                    for (int i = 0; i < SalesTransactionLines.Count; i++)
                    {
                        var line = SalesTransactionLines[i];
                        if (CompareLines(line.Model, _selectedLine.Model) && i != _selectedIndex)
                        {
                            line.Quantity += _selectedLine.Quantity;
                            line.StockDeducted += _selectedLine.StockDeducted;

                            // Some operations for the removal to be correct
                            _selectedLine.Discount = oldDiscount;
                            _selectedLine.SalesPrice = oldSalesPrice;
                            _selectedLine.Salesman = oldSalesman;
                            SalesTransactionLines.Remove(_selectedLine);
                            break;
                        }
                    }

                    IsEditWindowNotOpen = true;
                    EditWindowVisibility = Visibility.Hidden;
                    OnPropertyChanged("TransactionGrossTotal");
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
                        SalesTransactionLines.Remove(_selectedLine);
                    }

                    OnPropertyChanged("TransactionGrossTotal");
                }));
            }
        }
        #endregion

        #region Other Commands
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
                    if (SalesTransactionLines.Count == 0) return;

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("SalesTransactionLines")
                        .Include("SalesTransactionLines.Item")
                        .Include("SalesTransactionLines.Warehouse")
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
                    if (SalesTransactionLines.Count == 0) return;

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("SalesTransactionLines")
                        .Include("SalesTransactionLines.Item")
                        .Include("SalesTransactionLines.Warehouse")
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
                    if (SalesTransactionLines.Count == 0) return;

                    if (MessageBox.Show("Confirm printing?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                    if (Model.DOPrinted == true)
                    {
                        if (!UtilityMethods.GetVerification()) return;
                    }

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("SalesTransactionLines")
                        .Include("SalesTransactionLines.Item")
                        .Include("SalesTransactionLines.Warehouse")
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
                    if (SalesTransactionLines.Count == 0) return;

                    if (MessageBox.Show("Confirm printing?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                    if (Model.InvoicePrinted == true)
                    {
                        if (!UtilityMethods.GetVerification()) return;
                    }

                    SalesTransaction transaction;
                    using (var context = new ERPContext())
                    {
                        transaction = context.SalesTransactions
                        .Include("SalesTransactionLines")
                        .Include("SalesTransactionLines.Item")
                        .Include("SalesTransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID
                        .Equals(Model.SalesTransactionID))
                        .FirstOrDefault();

                        if (transaction == null)
                        {
                            MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                            return;
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

                        if (transaction.InvoicePrinted == false)
                        {
                            transaction.InvoicePrinted = true;
                            Model.InvoicePrinted = true;
                            context.SaveChanges();
                        }
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

                            transaction.InvoiceIssued = UtilityMethods.GetCurrentDate().Date;
                            var user = App.Current.TryFindResource("CurrentUser") as User;
                            if (user != null) transaction.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                            Model = transaction;

                            var costOfGoodsSoldAmount = CalculateCOGS(context, transaction.SalesTransactionLines.ToList());

                            // Recognise revenue recognitition at this point and record the corresponding journal entries
                            var transaction1 = new LedgerTransaction();
                            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, transaction1, UtilityMethods.GetCurrentDate().Date, transaction.SalesTransactionID, "Sales Revenue")) return;
                            context.SaveChanges();
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction1, string.Format("{0} Accounts Receivable", transaction.Customer.Name), "Debit", Model.NetTotal);
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction1, "Sales Revenue", "Credit", Model.NetTotal);
                            context.SaveChanges();

                            var transaction2 = new LedgerTransaction();
                            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, transaction2, UtilityMethods.GetCurrentDate().Date, transaction.SalesTransactionID, "Cost of Goods Sold")) return;
                            context.SaveChanges();
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction2, "Cost of Goods Sold", "Debit", costOfGoodsSoldAmount);
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction2, "Inventory", "Credit", costOfGoodsSoldAmount);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        MessageBox.Show("Invoice has been successfully issued.", "Success", MessageBoxButton.OK);
                        InvoiceNotIssued = false;
                    }
                }));
            }
        }
        #endregion

        #region Collections Helper Methods
        private void UpdateCustomers()
        {
            Customers.Clear();

            using (var context = new ERPContext())
            {
                var customers = context.Customers
                    .Where(e => e.Active == true)
                    .OrderBy(e => e.Name)
                    .Include("Group")
                    .ToList();

                foreach (var customer in customers)
                    Customers.Add(new CustomerVM { Model = customer });
            }
        }
        #endregion

        #region Transacton Helper Methods
        private static SalesTransaction GetSalesTransactionFromDatabase(string salesTransactionID)
        {
            using (var context = new ERPContext())
            {
                return context.SalesTransactions
                    .Include("Customer")
                    .Include("SalesReturnTransactions")
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Salesman")
                    .Include("SalesTransactionLines.Item")
                    .Include("SalesTransactionLines.Warehouse")
                    .SingleOrDefault(e => e.SalesTransactionID.Equals(salesTransactionID));
            }
        }

        private static bool DoesCustomerHasMaximumNumberOfInvoices(CustomerVM customer)
        {
            using (var context = new ERPContext())
            {
                var customerTransactions =
                    context.SalesTransactions.Where(e => e.Customer.ID.Equals(customer.ID) && e.Paid < e.NetTotal);
                if (customerTransactions.Count() <= customer.MaxInvoices) return false;
                MessageBox.Show("This customer has maximum number of invoice(s).", "Invalid Customer",
                    MessageBoxButton.OK);
                return true;
            }
        }

        private static bool DoesCustomerHasOverduedInvoices(CustomerVM customer)
        {
            using (var context = new ERPContext())
            {
                var customerTransactions =
                    context.SalesTransactions.Where(e => e.Customer.ID.Equals(customer.ID) && e.Paid < e.NetTotal);

                if (!customerTransactions.Any()) return false;
                foreach (var t in customerTransactions)
                {
                    if (t.DueDate >= UtilityMethods.GetCurrentDate().Date) continue;
                    MessageBox.Show("This customer has overdued invoice(s).", "Invalid Customer",
                        MessageBoxButton.OK);
                    return true;
                }
                return false;
            }
        }

        private void DeleteInvoice()
        {
            using (var context = new ERPContext())
            {
                var transaction = context.SalesTransactions
                .Include("SalesTransactionLines")
                .Include("SalesTransactionLines.Item")
                .Include("SalesTransactionLines.Warehouse").Single(e => e.SalesTransactionID.Equals(_transactionID));

                // Increase the stock for each line's item
                foreach (var line in transaction.SalesTransactionLines.ToList())
                {
                    var stock = context.Stocks.SingleOrDefault(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID));

                    if (stock != null)
                        stock.Pieces += line.Quantity;

                    else
                    {
                        var newStock = new Stock
                        {
                            Item = context.Inventory.SingleOrDefault(e => e.ItemID.Equals(line.Item.ItemID)),
                            Warehouse = context.Warehouses.SingleOrDefault(e => e.ID.Equals(line.Warehouse.ID)),
                            Pieces = line.Quantity
                        };

                        context.Stocks.Add(newStock);
                    }
                }

                context.SalesTransactions.Remove(transaction);
                context.SaveChanges();
            }
        }
        #endregion

        #region Save Transaction Helper Methods
        private bool IsTransactionCustomerSelected()
        {
            if (_transactionCustomer != null) return true;
            MessageBox.Show("Please select a customer.", "Missing Field(s)", MessageBoxButton.OK);
            return false;
        }

        private static bool IsConfirmationYes()
        {
            return MessageBox.Show("Confirm saving transaction?", "Confirmation", MessageBoxButton.YesNo) ==
                   MessageBoxResult.Yes;
        }

        private bool AreThereEnoughStockForAllTransactionItems()
        {
            foreach (var line in SalesTransactionLines)
            {
                var availableQuantity = GetAvailableQuantity(line.Item, line.Warehouse);
                if (availableQuantity >= 0) continue;
                MessageBox.Show(
                    $"{line.Item.Name} has only {availableQuantity / line.Item.PiecesPerUnit + line.Units} units, {availableQuantity % line.Item.PiecesPerUnit + line.Pieces} pieces available.",
                    "Insufficient Stock", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void SaveNewTransaction()
        {
            SetTransactionID();     // To prevent concurrent users saving to the same ID
            AssignSelectedPropertiesToModel();
            SalesTransactionHelper.AddTransactionToDatabase(Model);
        }

        private void AssignSelectedPropertiesToModel()
        {
            Model.SalesTransactionID = _transactionID;
            Model.Customer = _transactionCustomer.Model;
            Model.Notes = _transactionNotes;
            Model.Discount = _transactionDiscount;
            Model.SalesExpense = _transactionSalesExpense;
            Model.GrossTotal = _transactionGrossTotal;
            Model.NetTotal = _transactionNetTotal;
            Model.Date = _transactionDate;
            Model.DueDate = _transactionDate.AddDays(_transactionCustomer.CreditTerms);
            var user = Application.Current.FindResource("CurrentUser") as User;
            Model.User = user;

            foreach (var line in Model.SalesTransactionLines.ToList())
                Model.SalesTransactionLines.Remove(line);

            foreach (var line in SalesTransactionLines)
                Model.SalesTransactionLines.Add(line.Model);
        }

        private void SaveEditedTransaction()
        {
            AssignSelectedPropertiesToModel();

            if (Model.InvoiceIssued == null)
                SaveInvoiceNotIssuedEditedTransaction();
            
            else
                SaveInvoiceIssuedEditedTransaction();       
        }

        private void SaveInvoiceNotIssuedEditedTransaction()
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                var originalTransaction = context.SalesTransactions
                    .Include("Customer")
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Salesman")
                    .Include("SalesTransactionLines.Warehouse")
                    .Include("SalesTransactionLines.Item")
                    .Single(e => e.SalesTransactionID.Equals(_transactionID));

                var originalTransactionLines = originalTransaction.SalesTransactionLines.ToList();

                originalTransaction.Date = Model.Date;
                originalTransaction.DueDate = Model.DueDate;
                originalTransaction.Customer = context.Customers.Single(customer => customer.ID.Equals(Model.Customer.ID));
                originalTransaction.Notes = Model.Notes;
                originalTransaction.Discount = Model.Discount;
                originalTransaction.SalesExpense = Model.SalesExpense;
                originalTransaction.GrossTotal = Model.GrossTotal;
                originalTransaction.NetTotal = Model.NetTotal;
                originalTransaction.InvoiceIssued = Model.InvoiceIssued;
                originalTransaction.User = context.Users.Single(user => user.Username.Equals(Model.User.Username));

                // Remove the original transaction lines
                foreach (var line in originalTransactionLines)
                {
                    originalTransaction.SalesTransactionLines.Remove(line);

                    // Retrieve the item's current stock from the database
                    var stock = context.Stocks.SingleOrDefault(
                        e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID));
                    if (stock != null) stock.Pieces += line.Quantity;
                    else
                    {
                        var newStock = new Stock
                        {
                            Item = context.Inventory.Single(item => item.ItemID.Equals(line.Item.ItemID)),
                            Warehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(line.Warehouse.ID)),
                            Pieces = line.Quantity
                        };
                        context.Stocks.Add(newStock);
                    }
                }
                context.SaveChanges();

                // Add the edited transaction lines
                foreach (var line in Model.SalesTransactionLines.ToList())
                {
                    line.SalesTransaction = originalTransaction;
                    line.Item = context.Inventory.Single(item => item.ItemID.Equals(line.Item.ItemID));
                    line.Warehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(line.Warehouse.ID));
                    line.Salesman = context.Salesmans.Single(salesman => salesman.ID.Equals(line.Salesman.ID));
                    context.SalesTransactionLines.Add(line);

                    // Retrieve the item's current stock from the database
                    var stock = context.Stocks.Single(
                        e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID));
                    stock.Pieces -= line.Quantity;
                }

                context.SaveChanges();
                ts.Complete();
            }

        }

        private void SaveInvoiceIssuedEditedTransaction()
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                var transactionFromDatabase = context.SalesTransactions
                    .Include("Customer")
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Salesman")
                    .Include("SalesTransactionLines.Warehouse")
                    .Include("SalesTransactionLines.Item")
                    .Single(transaction => transaction.SalesTransactionID.Equals(_transactionID));

                var originalTransactionLines = transactionFromDatabase.SalesTransactionLines.ToList();

                if (transactionFromDatabase.NetTotal != _transactionNetTotal)
                {
                    // Adjust the Sales Revenue, AR, COGS for this transaction due to the price changes 
                    var transactionTotalDifference = _transactionNetTotal - transactionFromDatabase.NetTotal;

                    var adjustmentLedgerTransaction1 = new LedgerTransaction();
                    LedgerTransactionHelper.AddTransactionToDatabase(context, adjustmentLedgerTransaction1,
                        UtilityMethods.GetCurrentDate().Date, _transactionID, "Sales Revenue Adjustment");
                    context.SaveChanges();
                    if (transactionTotalDifference > 0)
                    {
                        LedgerTransactionHelper.AddTransactionLineToDatabase(context, adjustmentLedgerTransaction1,
                            string.Format("{0} Accounts Receivable", transactionFromDatabase.Customer.Name), "Debit",
                            transactionTotalDifference);
                        LedgerTransactionHelper.AddTransactionLineToDatabase(context, adjustmentLedgerTransaction1,
                            "Sales Revenue", "Credit", transactionTotalDifference);
                    }

                    else
                    {
                        LedgerTransactionHelper.AddTransactionLineToDatabase(context, adjustmentLedgerTransaction1,
                            "Sales Revenue", "Debit", -transactionTotalDifference);
                        LedgerTransactionHelper.AddTransactionLineToDatabase(context, adjustmentLedgerTransaction1,
                            string.Format("{0} Accounts Receivable", transactionFromDatabase.Customer.Name),
                            "Credit", -transactionTotalDifference);
                    }

                    foreach (var line in originalTransactionLines)
                    {
                        var found = false;
                        foreach (var l in SalesTransactionLines)
                        {
                            if (CompareLines(line, l.Model))
                            {
                                found = true;
                                break;
                            }
                        }

                        // If not found, it means the line has been edited
                        if (!found)
                        {
                            context.SalesTransactionLines.Remove(line);
                            context.SaveChanges();
                        }
                    }

                    foreach (var line in SalesTransactionLines)
                    {
                        var found = false;
                        foreach (var l in originalTransactionLines)
                        {
                            if (CompareLines(line.Model, l))
                            {
                                found = true;
                                break;
                            }
                        }

                        // If not found, add the edited line to the transaction
                        if (!found)
                        {
                            line.SalesTransaction = transactionFromDatabase;
                            line.Salesman = context.Salesmans.Single(e => e.ID.Equals(line.Salesman.ID));
                            line.Item = context.Inventory.Single(e => e.ItemID.Equals(line.Item.ItemID));
                            line.Warehouse = context.Warehouses.Single(e => e.ID.Equals(line.Warehouse.ID));
                            context.SalesTransactionLines.Add(line.Model);
                        }
                    }

                    transactionFromDatabase.Notes = _transactionNotes;
                    transactionFromDatabase.GrossTotal = _transactionGrossTotal;
                    transactionFromDatabase.Discount = _transactionDiscount;
                    transactionFromDatabase.NetTotal = _transactionNetTotal;

                    context.SaveChanges();
                    ts.Complete();
                }
            }

            MessageBox.Show("Invoice successfully edited.", "Success", MessageBoxButton.OK);
        }
        #endregion

        #region Helper Methods
        private void SetTransactionID()
        {
            InvoiceNotIssued = true;

            var month = _transactionDate.Month;
            var year = _transactionDate.Year;
            var leadingIDString = "M" + (long) ((year - 2000)*100 + month) + "-";
            var endingIDString = 0.ToString().PadLeft(4, '0');
            _transactionID = leadingIDString + endingIDString;

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = from SalesTransaction in context.SalesTransactions
                    where SalesTransaction.SalesTransactionID.Substring(0, 6).Equals(leadingIDString)
                    && string.Compare(SalesTransaction.SalesTransactionID, _transactionID, StringComparison.Ordinal) >= 0
                    orderby SalesTransaction.SalesTransactionID descending
                    select SalesTransaction.SalesTransactionID;
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null)
            {
                var newIDIndex = Convert.ToInt64(lastTransactionID.Substring(6, 4)) + 1;
                endingIDString = newIDIndex.ToString().PadLeft(4, '0');
                _transactionID = leadingIDString + endingIDString;
            }

            Model.SalesTransactionID = _transactionID;
            OnPropertyChanged("TransactionID");
        }

        private void ResetTransaction()
        {
            _editCustomer = null;
            InvoiceNotIssued = true;
            SalesNotReturned = true;
            Model = new SalesTransaction();
            SalesTransactionLines.Clear();
            _deletedLines.Clear();
            NewEntryVM.ResetEntryFields();
            NewEntryVM.NewEntryWarehouse = null;
            SetTransactionID();
            TransactionNotes = null;
            TransactionDiscount = 0;
            TransactionSalesExpense = 0;
            TransactionCustomer = null;
            TransactionCustomerCity = null;
            TransactionDate = UtilityMethods.GetCurrentDate().Date;
            UpdateCustomers();
            EditMode = false;
        }

        private void SetEditMode()
        {
            EditMode = true;
            _editCustomer = new CustomerVM { Model = Model.Customer };

            InvoiceNotIssued = Model.InvoiceIssued == null;
            InvoiceNotPaid = Model.Paid == 0;
            SalesNotReturned = Model.SalesReturnTransactions.Count == 0;
            TransactionDate = Model.Date;
            TransactionCustomer = new CustomerVM { Model = Model.Customer };
            TransactionNotes = Model.Notes;

            SalesTransactionLines.Clear();
            _deletedLines.Clear();
            foreach (var line in Model.SalesTransactionLines)
            {
                var vm = new SalesTransactionLineVM { Model = line, StockDeducted = line.Quantity };
                SalesTransactionLines.Add(vm);
            }

            OnPropertyChanged("TransactionGrossTotal");
            TransactionSalesExpense = Model.SalesExpense;
            TransactionDiscount = Model.Discount;

            NewEntryVM.ResetEntryFields();
            NewEntryVM.NewEntryWarehouse = null;
        }

        private decimal CalculateCOGS(ERPContext context, List<SalesTransactionLine> lines)
        {
            var costOfGoodsSoldAmount = 0m;

            // Determine the COGS for each line
            foreach (var line in lines)
            {
                var itemID = line.Item.ItemID;

                var purchases = context.PurchaseTransactionLines
                .Include("PurchaseTransaction")
                .Where(e => e.ItemID == itemID && e.SoldOrReturned < e.Quantity)
                .OrderBy(e => e.PurchaseTransactionID)
                .ThenByDescending(transaction => transaction.Quantity - transaction.SoldOrReturned)
                .ThenByDescending(transaction => transaction.PurchasePrice)
                .ThenByDescending(transaction => transaction.Discount)
                .ThenByDescending(transaction => transaction.WarehouseID)
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

            return costOfGoodsSoldAmount;
        }

        private static int GetStock(Item item, Warehouse warehouse)
        {
            int s;
            using (var context = new ERPContext())
            {
                var stock = context
                    .Stocks
                    .FirstOrDefault(e => e.Item.ItemID.Equals(item.ItemID) && e.Warehouse.ID.Equals(warehouse.ID));
                s = stock?.Pieces ?? 0;
            }
            return s;
        }

        public int GetAvailableQuantity(Item item, Warehouse warehouse)
        {
            var availableQuantity = GetStock(item, warehouse);

            // Decrease availableQuantity by the number of items from the same warehouse that are already in the transaction
            foreach (var line in SalesTransactionLines)
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
                if (line.Item.ItemID.Equals(item.ItemID) && line.Warehouse.ID.Equals(warehouse.ID))
                {
                    availableQuantity += line.StockDeducted;
                }
            }

            return availableQuantity;
        }

        private bool CheckWarehouseExistsInTransaction(int warehouseID)
        {
            foreach (var line in SalesTransactionLines)
            {
                if (line.Warehouse.ID.Equals(warehouseID))
                    return true;
            }
            return false;
        }

        private static bool CompareLines(SalesTransactionLine l1, SalesTransactionLine l2)
        {
            using (var context = new ERPContext())
            {
                if (l1.Item == null) l1.Item = context.Inventory.Single(e => e.ItemID.Equals(l1.ItemID));
                if (l2.Item == null) l2.Item = context.Inventory.Single(e => e.ItemID.Equals(l2.ItemID));

                if (l1.Warehouse == null) l1.Warehouse = context.Warehouses.Single(e => e.ID.Equals(l1.WarehouseID));
                if (l2.Warehouse == null) l2.Warehouse = context.Warehouses.Single(e => e.ID.Equals(l2.WarehouseID));

            }
            return l1.Item.ItemID.Equals(l2.Item.ItemID) && l1.Warehouse.ID.Equals(l2.Warehouse.ID)
                && l1.SalesPrice.Equals(l2.SalesPrice)
                && l1.Discount.Equals(l2.Discount);
        }

        public void UpdateTransactionTotal()
        {
            OnPropertyChanged("TransactionGrossTotal");
        }
        #endregion

        #region Reports Creation Methods
        private LocalReport CreateInvoiceLocalReport()
        {
            SalesTransaction salesTransaction;
            using (var context = new ERPContext())
            {
                salesTransaction = context.SalesTransactions
                        .Include("SalesTransactionLines")
                        .Include("SalesTransactionLines.Item")
                        .Include("SalesTransactionLines.Warehouse")
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
            foreach (var line in salesTransaction.SalesTransactionLines)
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
            dt2.Columns.Add(new DataColumn("Copy", typeof(string)));
            dr2["InvoiceGrossTotal"] = salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = salesTransaction.NetTotal;
            dr2["Customer"] = salesTransaction.Customer.Name;
            dr2["Address"] = salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = salesTransaction.SalesTransactionID;
            dr2["Date"] = salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = salesTransaction.DueDate.ToString("dd-MM-yyyy");
            dr2["Notes"] = salesTransaction.Notes;
            dr2["Copy"] = salesTransaction.InvoicePrinted == true ? "Copy" : "";

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
                        .Include("SalesTransactionLines")
                        .Include("SalesTransactionLines.Item")
                            .Include("SalesTransactionLines.Warehouse")
                        .Include("Customer")
                        .Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID))
                        .FirstOrDefault();
            }

            int count = 1;
            foreach (var line in salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(warehouseID)).ToList())
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
            dr2["InvoiceNetTotal"] = salesTransaction.NetTotal;
            dr2["Customer"] = salesTransaction.Customer.Name;
            dr2["Address"] = salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = salesTransaction.SalesTransactionID;
            dr2["Date"] = salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = salesTransaction.DueDate.ToString("dd-MM-yyyy");
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
    }
}
