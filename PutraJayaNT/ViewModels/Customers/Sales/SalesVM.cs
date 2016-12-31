namespace ECERP.ViewModels.Customers.Sales
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using ECERP.Reports.Windows;
    using Models;
    using Models.Inventory;
    using Models.Sales;
    using MVVMFramework;
    using Services;
    using Utilities;
    using Utilities.ModelHelpers;
    using Utilities.PrintHelpers;
    using ViewModels.Sales;
    using Views.Customers.Sales;

    public class SalesVM : ViewModelBase<SalesTransaction>
    {
        private SalesTransactionLineVM _selectedLine;

        #region Transaction Backing Fields
        private string _transactionID;
        private DateTime _transactionDate;
        private DateTime _transactionDueDate;
        private CustomerVM _transactionCustomer;
        private string _transactionCustomerCity;
        private string _transactionNotes;
        private decimal? _transactionDiscountPercent;
        private decimal _transactionDiscount;
        private bool _isTransactionTaxCheckBoxSelected;
        private decimal _transactionTax;
        private decimal _transactionSalesExpense;
        private decimal _transactionGrossTotal;
        private decimal _transactionNetTotal;
        private ICommand _newTransactionCommand;
        private ICommand _deleteTransactionCommand;
        private ICommand _saveTransactionCommand;
        #endregion

        #region Command Backing Fields
        private ICommand _editLineCommand;
        private ICommand _deleteLineCommand;
        private ICommand _browseCommand;
        private ICommand _printListCommand;
        private ICommand _printDOCommand;
        private ICommand _printInvoiceCommand;
        private ICommand _previewInvoiceCommand;
        private ICommand _previewDOCommand;
        private ICommand _issueInvoiceCommand;
        #endregion

        private bool _editMode;
        private bool _invoiceNotIssued = true;
        private bool _salesNotReturned = true;
        private bool _isSaveAllowed = true;
        private CustomerVM _editCustomer;

        public SalesVM()
        {
            NewEntryVM = new SalesNewEntryVM(this);

            Customers = new ObservableCollection<CustomerVM>();
            DisplayedSalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();

            Model = new SalesTransaction();
            var currentDate = UtilityMethods.GetCurrentDate().Date;
            _transactionDate = currentDate;
            _transactionDueDate = currentDate;
            Model.Date = _transactionDate;
            Model.InvoiceIssued = null;
            SetTransactionID();

            UpdateCustomers();
        }

        public SalesNewEntryVM NewEntryVM { get; }

        public bool EditMode
        {
            get { return _editMode; }
            set { SetProperty(ref _editMode, value, () => EditMode); }
        }

        public bool InvoiceNotIssued
        {
            get { return _invoiceNotIssued; }
            set { SetProperty(ref _invoiceNotIssued, value, () => InvoiceNotIssued); }
        }

        public bool SalesNotReturned
        {
            get { return _salesNotReturned; }
            set { SetProperty(ref _salesNotReturned, value, () => SalesNotReturned); }
        }

        public bool IsSaveAllowed
        {
            get { return _isSaveAllowed; }
            set { SetProperty(ref _isSaveAllowed, value, () => IsSaveAllowed); }
        }

        #region Collections
        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<SalesTransactionLineVM> DisplayedSalesTransactionLines { get; }
        #endregion

        public SalesTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }

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
                if (value != null &&
                    ((_editCustomer == null && !value.Name.Equals("Kontan")) ||
                     (_editCustomer != null && !value.ID.Equals(_editCustomer.ID))))
                {
                    if (DoesCustomerHasMaximumNumberOfInvoices(value) || DoesCustomerHasOverduedInvoices(value))
                    {
                        if (!UtilityMethods.GetVerification()) return;
                    }
                }

                SetProperty(ref _transactionCustomer, value, () => TransactionCustomer);
                if (_transactionCustomer == null) return;
                TransactionCustomerCity = _transactionCustomer.City.Name;
                if (!_editMode)
                    TransactionDueDate = _transactionDate.AddDays(_transactionCustomer.CreditTerms);
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

        public DateTime TransactionDueDate
        {
            get { return _transactionDueDate; }
            set
            {
                if (value < _transactionDate)
                {
                    MessageBox.Show("Cannot set a date before the transaction date.", "Invalid Date",
                        MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionDueDate, value, () => TransactionDueDate);
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
                    MessageBox.Show("Please enter a value from the range of 0 - 100.", "Invalid Range",
                        MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionDiscountPercent, value, () => TransactionDiscountPercent);

                if (_transactionDiscountPercent == null) return;

                TransactionDiscount = (decimal) _transactionDiscountPercent / 100 * _transactionGrossTotal;
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
                    MessageBox.Show($"a Please enter a value from the range of 0 - {_transactionGrossTotal}.",
                        "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionDiscount, value, () => TransactionDiscount);
                OnPropertyChanged("TransactionNetTotal");
            }
        }

        public bool IsTransactionTaxCheckBoxSelected
        {
            get { return _isTransactionTaxCheckBoxSelected; }
            set
            {
                SetProperty(ref _isTransactionTaxCheckBoxSelected, value, () => IsTransactionTaxCheckBoxSelected);
                CalculateTransactionTax();
            }
        }

        public decimal TransactionTax
        {
            get { return _transactionTax; }
            set
            {
                if (value < 0 || value > _transactionGrossTotal)
                {
                    MessageBox.Show($"Please enter a value from the range of 0 - {_transactionGrossTotal}.",
                        "Invalid Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _transactionTax, value, () => TransactionTax);
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
                foreach (var line in DisplayedSalesTransactionLines)
                    _transactionGrossTotal += line.Total;

                OnPropertyChanged("TransactionNetTotal");
                return _transactionGrossTotal;
            }
        }

        public decimal TransactionNetTotal
        {
            get
            {
                _transactionNetTotal =
                    Math.Round(
                        _transactionGrossTotal + _transactionSalesExpense - _transactionDiscount + _transactionTax, 2);
                return _transactionNetTotal;
            }
        }
        #endregion

        #region Transaction Commands
        public ICommand NewTransactionCommand
            => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));

        public ICommand DeleteTransactionCommand
        {
            get
            {
                return _deleteTransactionCommand ?? (_deleteTransactionCommand = new RelayCommand(() =>
                       {
                           if (!_editMode) return;

                           if (
                               MessageBox.Show("Confirm deleting this transaction?", "Confirmation",
                                   MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                           if (!UtilityMethods.GetMasterAdminVerification()) return;
                           DeleteInvoice();
                           MessageBox.Show("Successfully deleted transaction!", "Success", MessageBoxButton.OK);
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
                           if (!IsConfirmationYes() || !IsTransactionCustomerSelected() ||
                               !AreThereEnoughStockForAllTransactionItems() || !_isSaveAllowed) return;

                           if (_editMode) SaveEditedTransaction();
                           else SaveNewTransaction();

                           if (!SalesTransactionHelper.IsLastSaveSuccessful)
                           {
                               MessageBox.Show("Failed to save transaction. Please try again.", "Failure", MessageBoxButton.OK);
                               return;
                           }

                           MessageBox.Show("Successfully saved transaction.", "Success", MessageBoxButton.OK);
                           Model = GetSalesTransactionFromDatabase(_transactionID);
                           SetEditMode();
                           AddTelegramNotifications();
                       }));
            }
        }
        #endregion

        #region Other Commands
        public ICommand EditLineCommand
        {
            get
            {
                return _editLineCommand ?? (_editLineCommand = new RelayCommand(() =>
                       {
                           if (!IsThereLineSelected()) return;
                           ShowEditLineWindow();
                       }));
            }
        }

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                       {
                           if (_selectedLine != null &&
                               MessageBox.Show("Confirm Deletion?", "Confirmation", MessageBoxButton.YesNo)
                               == MessageBoxResult.No) return;
                           DisplayedSalesTransactionLines.Remove(_selectedLine);
                           UpdateTransactionTotal();
                       }));
            }
        }

        public ICommand BrowseCommand
            => _browseCommand ?? (_browseCommand = new RelayCommand(ShowBroseSalesTransactionsWindow));

        public ICommand PrintListCommand
            => _printListCommand ?? (_printListCommand = new RelayCommand(ShowPrintListWindow));

        public ICommand PreviewDOCommand
        {
            get
            {
                return _previewDOCommand ?? (_previewDOCommand = new RelayCommand(() =>
                       {
                           if (DisplayedSalesTransactionLines.Count == 0) return;

                           var salesTransactionFromDatabase = GetSalesTransactionFromDatabase(Model.SalesTransactionID);

                           if (salesTransactionFromDatabase == null)
                           {
                               MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                               return;
                           }

                           ShowSalesTransactionDOReportWindow(salesTransactionFromDatabase);
                       }));
            }
        }

        public ICommand PreviewInvoiceCommand
        {
            get
            {
                return _previewInvoiceCommand ?? (_previewInvoiceCommand = new RelayCommand(() =>
                       {
                           if (DisplayedSalesTransactionLines.Count == 0) return;

                           var salesTransactionFromDatabase = GetSalesTransactionFromDatabase(Model.SalesTransactionID);

                           if (salesTransactionFromDatabase == null)
                           {
                               MessageBox.Show("Transaction not found.", "Invalid Command", MessageBoxButton.OK);
                               return;
                           }

                           ShowSalesInvoiceReportWindow(salesTransactionFromDatabase);
                       }));
            }
        }

        public ICommand PrintDOCommand
        {
            get
            {
                return _printDOCommand ?? (_printDOCommand = new RelayCommand(() =>
                       {
                           if (DisplayedSalesTransactionLines.Count == 0) return;

                           if (
                               MessageBox.Show("Confirm printing?", "Confirmation", MessageBoxButton.YesNo,
                                   MessageBoxImage.Question) == MessageBoxResult.No) return;

                           if (Model.DOPrinted)
                           {
                               if (!UtilityMethods.GetVerification()) return;
                           }

                           SalesTransactionPrintHelper.PrintSalesTransactionDOFromDatabase(Model);
                           Model.DOPrinted = true;
                       }));
            }
        }

        public ICommand PrintInvoiceCommand
        {
            get
            {
                return _printInvoiceCommand ?? (_printInvoiceCommand = new RelayCommand(() =>
                       {
                           if (DisplayedSalesTransactionLines.Count == 0) return;

                           if (
                               MessageBox.Show("Confirm printing?", "Confirmation", MessageBoxButton.YesNo,
                                   MessageBoxImage.Question) == MessageBoxResult.No) return;

                           if (Model.InvoicePrinted)
                           {
                               if (!UtilityMethods.GetVerification()) return;
                           }

                           SalesTransactionPrintHelper.PrintSalesTransactionFromDatabaseInvoice(Model);
                           Model.InvoicePrinted = true;
                       }));
            }
        }

        public ICommand IssueInvoiceCommand
        {
            get
            {
                return _issueInvoiceCommand ?? (_issueInvoiceCommand = new RelayCommand(() =>
                       {
                           if (!_invoiceNotIssued) return;

                           if (_editMode == false)
                           {
                               MessageBox.Show("Please save the transaction first.", "Invalid Command",
                                   MessageBoxButton.OK);
                               return;
                           }

                           if (
                               MessageBox.Show(
                                   "Please save any changes before proceeding. \n \n Confirm issuing invoice?",
                                   "Confirmation", MessageBoxButton.YesNo) ==
                               MessageBoxResult.No) return;

                           SaveEditedTransaction();
                           SalesTransactionHelper.IssueSalesTransactionInvoice(Model);
                           Model.InvoiceIssued = UtilityMethods.GetCurrentDate().Date;
                           InvoiceNotIssued = false;
                           MessageBox.Show("Invoice has been successfully issued.", "Success", MessageBoxButton.OK);
                           Model = GetSalesTransactionFromDatabase(_transactionID);
                           SetEditMode();
                       }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateCustomers()
        {
            Customers.Clear();

            using (var context = UtilityMethods.createContext())
            {
                var customers = context.Customers
                    .Where(customer => customer.Active)
                    .OrderBy(customer => customer.Name)
                    .Include("City")
                    .Include("Group")
                    .ToList();

                foreach (var customer in customers)
                    Customers.Add(new CustomerVM { Model = customer });
            }
        }

        private void SetTransactionID()
        {
            var month = _transactionDate.Month;
            var year = _transactionDate.Year;
            var serverName = UtilityMethods.GetServerName();
            var initialID = serverName.Equals("") ? "M" : serverName.Substring(0, 1);
            var leadingIDString = initialID + (long) ((year - 2000) * 100 + month) + "-";
            var endingIDString = 0.ToString().PadLeft(4, '0');
            _transactionID = leadingIDString + endingIDString;

            string lastTransactionID = null;
            using (var context = UtilityMethods.createContext())
            {
                var IDs = from SalesTransaction in context.SalesTransactions
                    where SalesTransaction.SalesTransactionID.Substring(0, 6).Equals(leadingIDString)
                          &&
                          string.Compare(SalesTransaction.SalesTransactionID, _transactionID, StringComparison.Ordinal) >=
                          0
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

            EditMode = false;
            InvoiceNotIssued = true;
            SalesNotReturned = true;
            IsSaveAllowed = true;

            Model = new SalesTransaction();
            DisplayedSalesTransactionLines.Clear();
            NewEntryVM.ResetEntryFields();
            NewEntryVM.NewEntryWarehouse = null;
            TransactionNotes = null;
            TransactionDiscount = 0;
            TransactionSalesExpense = 0;
            TransactionTax = 0;
            IsTransactionTaxCheckBoxSelected = false;
            TransactionCustomer = null;
            TransactionCustomerCity = null;
            TransactionDate = UtilityMethods.GetCurrentDate().Date;
            TransactionDueDate = UtilityMethods.GetCurrentDate().Date;
            SetTransactionID();
            UpdateCustomers();
        }

        private void SetEditMode()
        {
            _editCustomer = new CustomerVM { Model = Model.Customer };

            EditMode = true;
            InvoiceNotIssued = Model.InvoiceIssued == null;
            SalesNotReturned = Model.SalesReturnTransactions.Count == 0;
            IsSaveAllowed = Model.Paid == 0 && Model.SalesReturnTransactions.Count == 0;

            TransactionDate = Model.Date;
            TransactionDueDate = Model.DueDate;
            TransactionCustomer = new CustomerVM { Model = Model.Customer };
            TransactionNotes = Model.Notes;

            DisplayedSalesTransactionLines.Clear();
            foreach (
                var salesTransactionLineVM in
                Model.SalesTransactionLines.Select(line => new SalesTransactionLineVM { Model = line }))
                DisplayedSalesTransactionLines.Add(salesTransactionLineVM);

            OnPropertyChanged("TransactionGrossTotal");
            TransactionSalesExpense = Model.SalesExpense;
            TransactionDiscount = Model.Discount;
            IsTransactionTaxCheckBoxSelected = Model.Tax > 0;
            TransactionTax = Model.Tax;
            OnPropertyChanged("TransactionNetTotal");

            NewEntryVM.ResetEntryFields();
            NewEntryVM.NewEntryWarehouse = null;
        }

        private void CalculateTransactionTax()
        {
            if (_isTransactionTaxCheckBoxSelected)
                TransactionTax = (_transactionGrossTotal - _transactionDiscount) * 0.1m;
            // auto recalculates transaction net total
            else TransactionTax = 0;
        }

        private static int GetStock(Item item, Warehouse warehouse)
        {
            int s;
            using (var context = UtilityMethods.createContext())
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
            var salesTransactionFromDatabae = GetSalesTransactionFromDatabase(Model.SalesTransactionID);

            availableQuantity = DisplayedSalesTransactionLines.Where(
                    line => line.Item.ItemID.Equals(item.ItemID) && line.Warehouse.ID.Equals(warehouse.ID))
                .Aggregate(availableQuantity, (current, line) => current - line.Quantity);

            if (salesTransactionFromDatabae != null)
            {
                availableQuantity += salesTransactionFromDatabae.SalesTransactionLines.ToList().Where(
                        line => line.Item.ItemID.Equals(item.ItemID) && line.Warehouse.ID.Equals(warehouse.ID))
                    .Sum(line => line.Quantity);
            }

            return availableQuantity;
        }

        public void UpdateTransactionTotal()
        {
            OnPropertyChanged("TransactionGrossTotal");
        }
        #endregion

        #region Transacton Helper Methods
        private static SalesTransaction GetSalesTransactionFromDatabase(string salesTransactionID)
        {
            using (var context = UtilityMethods.createContext())
            {
                return context.SalesTransactions
                    .Include("Customer")
                    .Include("Customer.City")
                    .Include("CollectionSalesman")
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
            using (var context = UtilityMethods.createContext())
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
            using (var context = UtilityMethods.createContext())
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
            using (var context = UtilityMethods.createContext())
            {
                var transaction = context.SalesTransactions
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Item")
                    .Include("SalesTransactionLines.Warehouse").Single(e => e.SalesTransactionID.Equals(_transactionID));

                // Increase the stock for each line's item
                foreach (var line in transaction.SalesTransactionLines.ToList())
                {
                    var stock =
                        context.Stocks.SingleOrDefault(
                            e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID));

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
            foreach (var line in DisplayedSalesTransactionLines)
            {
                var availableQuantity = GetAvailableQuantity(line.Item, line.Warehouse);
                if (availableQuantity >= 0) continue;
                MessageBox.Show(
                    $"{line.Item.Name} has only {availableQuantity / line.Item.PiecesPerUnit + line.Units} units," +
                    $" {availableQuantity % line.Item.PiecesPerUnit + line.Pieces} pieces available.",
                    "Insufficient Stock", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void SaveNewTransaction()
        {
            SetTransactionID(); // To prevent concurrent users saving to the same ID
            AssignSelectedPropertiesToModel();
            SalesTransactionHelper.AddTransactionToDatabase(Model);
        }

        private void AssignSelectedPropertiesToModel()
        {
            Model.SalesTransactionID = _transactionID;
            Model.Customer = _transactionCustomer.Model;
            Model.Notes = _transactionNotes;
            Model.Discount = _transactionDiscount;
            Model.Tax = _transactionTax;
            Model.SalesExpense = _transactionSalesExpense;
            Model.GrossTotal = _transactionGrossTotal;
            Model.NetTotal = _transactionNetTotal;
            Model.Date = _transactionDate;
            Model.DueDate = _transactionDueDate;
            var user = Application.Current.FindResource(Constants.CURRENTUSER) as User;
            Model.User = user;

            foreach (var line in Model.SalesTransactionLines.ToList())
                Model.SalesTransactionLines.Remove(line);

            foreach (var line in DisplayedSalesTransactionLines)
                Model.SalesTransactionLines.Add(line.Model);
        }

        private void SaveEditedTransaction()
        {
            AssignSelectedPropertiesToModel();
            var salesTransactionFromDatabase = GetSalesTransactionFromDatabase(Model.SalesTransactionID);
            if (!salesTransactionFromDatabase.InvoiceIssued.Equals(Model.InvoiceIssued) ||
                !salesTransactionFromDatabase.Paid.Equals(Model.Paid)
                ||
                !salesTransactionFromDatabase.SalesReturnTransactions.Count.Equals(Model.SalesReturnTransactions.Count))
            {
                MessageBox.Show("The transaction has changed, please reload it before making any changes.",
                    "Invalid Command", MessageBoxButton.OK);
                return;
            }
            if (Model.InvoiceIssued == null)
                SalesTransactionHelper.EditNotIssuedInvoiceTransaction(Model);
            else
                SalesTransactionHelper.EditIssuedInvoiceTransaction(Model);
        }

        private void AddTelegramNotifications()
        {
            using (var context = UtilityMethods.createContext())
            {
                foreach (var line in DisplayedSalesTransactionLines)
                {
                    if (line.Warehouse.Name.Contains("Kanvas")) continue;
                    var lineStock =
                        context.Stocks
                            .Include("Warehouse")
                            .Include("Item")
                            .SingleOrDefault(
                                stock =>
                                    stock.ItemID.Equals(line.Item.ItemID) &&
                                    stock.WarehouseID.Equals(line.Warehouse.ID));
                    var unitsLeft = lineStock?.Pieces / line.Item.PiecesPerUnit ?? 0;
                    if (lineStock == null || lineStock.Pieces / lineStock.Item.PiecesPerUnit <= 10)
                        TelegramService.AddTelegramNotification(DateTime.Now,
                            $"{line.Warehouse.Name} / {line.Item.Name} : {unitsLeft}");
                }
                context.SaveChanges();
            }
        }
        #endregion

        #region Other Commands Helper Methods
        private void ShowEditLineWindow()
        {
            var vm = new SalesEditVM(this);
            var editWindow = new SalesEditView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editWindow.ShowDialog();
        }

        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "No Selection", MessageBoxButton.OK);
            return false;
        }

        private static void ShowBroseSalesTransactionsWindow()
        {
            var browseWindow = new BrowseSalesTransactionsView
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            browseWindow.Show();
        }

        private static void ShowPrintListWindow()
        {
            var printListWindow = new PrintListView
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            printListWindow.Show();
        }

        private static void ShowSalesTransactionDOReportWindow(SalesTransaction salesTransaction)
        {
            var salesDOWindow = new SalesTransactionDOReportWindow(salesTransaction)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            salesDOWindow.ShowDialog();
        }

        private static void ShowSalesInvoiceReportWindow(SalesTransaction salesTransaction)
        {
            var salesInvoiceReportWindow = new SalesInvoiceReportWindow(salesTransaction)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            salesInvoiceReportWindow.ShowDialog();
        }
        #endregion
    }
}