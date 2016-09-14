namespace PutraJayaNT.ViewModels.Suppliers.Purchase
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.Purchase;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;
    using ViewModels.Purchase;
    using Views.Suppliers.Purchase;

    internal class PurchaseVM : ViewModelBase<PurchaseTransaction>
    {
        private bool _notEditMode;
        private bool _isTransactionNotPaid;
        private bool _isDeletionAllowed;

        #region Transaction backing fields
        private string _transactionID;
        private DateTime _transactionInvoiceDate;
        private string _transactionDoid;
        private SupplierVM _transactionSupplier;
        private DateTime _transactionDate;
        private DateTime _transactionDueDate;
        private decimal? _transactionDiscountPercent;
        private decimal _transactionDiscount;
        private bool _isTransactionTaxCheckBoxSelected;
        private decimal _transactionTax;
        private decimal _transactionGrossTotal;
        private decimal _transactionNetTotal;
        private string _transactionNote;
        #endregion

        private PurchaseTransactionLineVM _selectedLine;
        private ICommand _editLineCommand;
        private ICommand _deleteLineCommand;

        private ICommand _saveTransactionCommand;
        private ICommand _newTransactionCommand;
        private ICommand _deleteTransactionCommand;

        public PurchaseVM()
        {
            NewEntryVM = new PurchaseNewEntryVM(this);

            Model = new PurchaseTransaction();
            DisplayedLines = new ObservableCollection<PurchaseTransactionLineVM>();
            Suppliers = new ObservableCollection<SupplierVM>();
            SupplierItems = new ObservableCollection<ItemVM>();
            Warehouses = new ObservableCollection<WarehouseVM>();

            UpdateSuppliers();

            var currentDate = UtilityMethods.GetCurrentDate().Date;
            TransactionDate = currentDate;
            TransactionDueDate = currentDate;
            TransactionInvoiceDate = currentDate;

            SetTransactionID();
            Model.DueDate = _transactionDueDate;
            Model.Total = 0;

            _notEditMode = true;
            _isTransactionNotPaid = true;
            _isDeletionAllowed = false;
        }

        public PurchaseNewEntryVM NewEntryVM { get; }

        public bool IsTransactionNotPaid
        {
            get { return _isTransactionNotPaid; }
            set { SetProperty(ref _isTransactionNotPaid, value, () => IsTransactionNotPaid); }
        }

        public bool NotEditMode
        {
            get { return _notEditMode; }
            set { SetProperty(ref _notEditMode, value, () => NotEditMode); }
        }

        public bool IsDeletionAllowed
        {
            get { return _isDeletionAllowed; }
            set { SetProperty(ref _isDeletionAllowed, value, () => IsDeletionAllowed); }
        }

        public PurchaseTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }

        #region Collections
        public ObservableCollection<SupplierVM> Suppliers { get; }

        public ObservableCollection<ItemVM> SupplierItems { get; }

        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<PurchaseTransactionLineVM> DisplayedLines { get; }
        #endregion

        #region Transaction Properties
        public string TransactionID
        {
            get { return _transactionID; }
            set
            {
                if (_transactionID == null) return;

                NewEntryVM.ResetEntryFields();
                NewEntryVM.NewEntryWarehouse = null;

                // Search the database for the transaction
                using (var context = UtilityMethods.createContext())
                {
                    var purchaseTransactionFromDatabase = context.PurchaseTransactions
                        .Include("Supplier")
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .FirstOrDefault(e => e.PurchaseID.Equals(value));

                    if (purchaseTransactionFromDatabase == null)
                    {
                        MessageBox.Show("The purchase transaction could not be found.", "Invalid Sales Transaction", MessageBoxButton.OK);
                        return;
                    }

                    NotEditMode = false;
                    SetEditMode(purchaseTransactionFromDatabase);
                    SetProperty(ref _transactionID, value, () => TransactionID);
                }
            }
        }

        public string TransactionDOID
        {
            get { return _transactionDoid; }
            set { SetProperty(ref _transactionDoid, value, () => TransactionDOID); }
        }

        public DateTime TransactionInvoiceDate
        {
            get { return _transactionInvoiceDate; }
            set
            {
                SetProperty(ref _transactionInvoiceDate, value, () => TransactionInvoiceDate);
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
                    MessageBox.Show("Cannot set to before transaction date.", "Invalid Date", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _transactionDueDate, value, () => TransactionDueDate);
            }
        }

        public SupplierVM TransactionSupplier
        {
            get { return _transactionSupplier; }
            set
            {
                SetProperty(ref _transactionSupplier, value, () => TransactionSupplier);

                if (_transactionSupplier == null)
                {
                    SupplierItems.Clear();
                    return;
                }

                UpdateWarehouses();
                Suppliers.Clear();
                Suppliers.Add(value);
                UpdateSupplierItems();
            }
        }

        public string TransactionNote
        {
            get { return _transactionNote; }
            set { SetProperty(ref _transactionNote, value, () => TransactionNote); }
        }

        public decimal TransactionGrossTotal
        {
            get
            {
                _transactionGrossTotal = 0;
                foreach (var line in DisplayedLines)
                {
                    _transactionGrossTotal += line.Total;
                }
                OnPropertyChanged("TransactionNetTotal");
                CalculateTransactionTax();
                return _transactionGrossTotal;
            }
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
                    MessageBox.Show($"Please enter a value from the range of 0 - {_transactionGrossTotal}.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _transactionDiscount, value, () => TransactionDiscount);
                CalculateTransactionTax();
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
                    MessageBox.Show($"Please enter a value from the range of 0 - {_transactionGrossTotal}.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _transactionTax, value, () => TransactionTax);
                OnPropertyChanged("TransactionNetTotal");
            }
        }

        public decimal TransactionNetTotal
        {
            get
            {
                _transactionNetTotal = Math.Round(_transactionGrossTotal - _transactionDiscount + _transactionTax, 2);
                return _transactionNetTotal;
            }
        }
        #endregion

        #region Commands
        public ICommand EditLineCommand
        {
            get
            {
                return _editLineCommand ?? (_editLineCommand = new RelayCommand(() =>
                {
                    if (!IsThereASelectedLine()) return;
                    ShowEditWindow();
                    UpdateUIGrossTotal();
                }));
            }
        }

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (!IsThereASelectedLine()) return;
                    DisplayedLines.Remove(_selectedLine);
                    OnPropertyChanged("TransactionGrossTotal");
                }));
            }
        }

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(() =>
                {
                    if (DisplayedLines.Count == 0 || !_isTransactionNotPaid || !IsSaveConfirmationYes()) return;

                    AssignVMPropertiesToModel();

                    if (_notEditMode)
                        PurchaseTransactionHelper.AddNewTransactionToDatabase(Model);
                    else
                        PurchaseTransactionHelper.EditTransactionInDatabase(Model);

                    ResetTransaction();
                    if (PurchaseTransactionHelper.IsLastSaveSuccessful)
                        MessageBox.Show("Transaction saved!", "Success", MessageBoxButton.OK);
                }));
            }
        }

        public ICommand DeleteTransactionCommand
        {
            get
            {
                return _deleteTransactionCommand ?? (_deleteTransactionCommand = new RelayCommand(() =>
                {
                    if (!DoAllLinesHaveEnoughStock() || !IsAllLinesSoldOrReturnedZero() || 
                    IsTransactionPaid() || !UtilityMethods.GetMasterAdminVerification()) return;
                    PurchaseTransactionHelper.DeleteTransactionInDatabase(Model);
                    if (PurchaseTransactionHelper.IsLastSaveSuccessful)
                        MessageBox.Show("Purchase transaction successfully deleted!", "Success", MessageBoxButton.OK);
                    else
                        MessageBox.Show("Purchase transaction failed to be deleted!", "Failure", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }

        private bool IsTransactionPaid()
        {
            using (var context = UtilityMethods.createContext())
            {
                var transactionInDatabase =
                    context.PurchaseTransactions.Single(transaction => transaction.PurchaseID.Equals(Model.PurchaseID));
                if (transactionInDatabase.Paid == 0) return false;
                MessageBox.Show("The transaction had been paid!", "Failure", MessageBoxButton.OK);
                return true;
            }
        }

        public ICommand NewTransactionCommand => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));
        #endregion

        #region Helper Methods
        private void UpdateSuppliers()
        {
            Suppliers.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var suppliersFromDatabase = context.Suppliers.Where(
                    supplier => !supplier.Name.Equals("-") && supplier.Active)
                    .OrderBy(supplier => supplier.Name);
                foreach (var supplier in suppliersFromDatabase)
                    Suppliers.Add(new SupplierVM { Model = supplier });
            }
        }

        private void UpdateSupplierItems()
        {
            SupplierItems.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var items = context.Inventory.Include("Suppliers").Where(item => item.Active).OrderBy(item => item.Name).ToList();
                foreach (var item in items.Where(item => item.Active && item.Suppliers.Contains(_transactionSupplier.Model)))
                    SupplierItems.Add(new ItemVM { Model= item });
            }
        }

        private void UpdateWarehouses()
        {
            Warehouses.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var warehouses = context.Warehouses.OrderBy(warehouse => warehouse.Name);
                foreach (var warehouse in warehouses)
                    Warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void SetEditMode(PurchaseTransaction transaction)
        {
            Model = transaction;
            TransactionDOID = transaction.DOID;

            IsTransactionNotPaid = transaction.Paid == 0;
            IsTransactionTaxCheckBoxSelected = Model.Tax > 0;
            IsDeletionAllowed = IsTransactionNotPaid && IsAllLinesSoldOrReturnedZeroWithoutWarning();

            UpdateSuppliers();
            _transactionSupplier = Suppliers.Single(supplier => supplier.ID.Equals(transaction.Supplier.ID));
            OnPropertyChanged("TransactionSupplier");

            TransactionDate = transaction.Date;
            TransactionDueDate = transaction.DueDate;
            TransactionNote = transaction.Note;

            DisplayedLines.Clear();
            foreach (var line in transaction.PurchaseTransactionLines.OrderBy(e => e.Item.Name).ThenBy(e => e.Warehouse.Name).ThenBy(e => e.PurchasePrice).ThenBy(e => e.Discount).ToList())
                DisplayedLines.Add(new PurchaseTransactionLineVM { Model = line });

            OnPropertyChanged("TransactionGrossTotal"); // Updates transaction's net total too
            TransactionDiscount = transaction.Discount;
            TransactionTax = transaction.Tax;
        }

        private void SetTransactionID()
        {
            var month = _transactionDate.Month;
            var year = _transactionDate.Year;
            var leadingIDString = "P" + (long)((year - 2000) * 100 + month) + "-";
            var endingIDString = 0.ToString().PadLeft(4, '0');
            _transactionID = leadingIDString + endingIDString;

            string lastTransactionID = null;
            using(var context = UtilityMethods.createContext())
            {
                var IDs = from PurchaseTransaction in context.PurchaseTransactions
                          where PurchaseTransaction.PurchaseID.Substring(0, 6).Equals(leadingIDString)
                          && string.Compare(PurchaseTransaction.PurchaseID, _transactionID, StringComparison.Ordinal) >= 0
                          orderby PurchaseTransaction.PurchaseID descending
                          select PurchaseTransaction.PurchaseID;
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null)
            {
                var newIDIndex = Convert.ToInt64(lastTransactionID.Substring(6, 4)) + 1;
                endingIDString = newIDIndex.ToString().PadLeft(4, '0');
                _transactionID = leadingIDString + endingIDString;
            }

            Model.PurchaseID = _transactionID;
            OnPropertyChanged("TransactionID");
        }

        private void ResetTransaction()
        {
            NewEntryVM.ResetEntryFields();
            NewEntryVM.NewEntryWarehouse = null;

            IsTransactionNotPaid = true;
            NotEditMode = true;
            IsDeletionAllowed = false;
            IsTransactionTaxCheckBoxSelected = false;

            Model = new PurchaseTransaction();
            TransactionSupplier = null;
            TransactionDOID = null;
            TransactionTax = 0;
            TransactionDiscount = 0;
            TransactionNote = null;
            var currentDate = UtilityMethods.GetCurrentDate().Date;
            TransactionDate = currentDate;
            TransactionDueDate = currentDate;

            Warehouses.Clear();
            SupplierItems.Clear();
            DisplayedLines.Clear();
            UpdateSuppliers();

            OnPropertyChanged("TransactionGrossTotal");
            OnPropertyChanged("TransactionNetTotal");

            SetTransactionID();
        }

        private void CalculateTransactionTax()
        {
            if (_isTransactionTaxCheckBoxSelected)
                TransactionTax = (_transactionGrossTotal - _transactionDiscount) * 0.1m; // auto recalculates transaction net total
            else TransactionTax = 0; 
        }

        private static bool IsSaveConfirmationYes()
        {
            return
                MessageBox.Show("Confirm saving transaction?", "Confirmation", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private void AssignVMPropertiesToModel()
        {
            Model.Supplier = _transactionSupplier.Model;
            Model.DOID = _transactionDoid;
            Model.Date = _transactionDate;
            Model.DueDate = _transactionDueDate;
            Model.Note = _transactionNote;
            Model.GrossTotal = _transactionGrossTotal;
            Model.Discount = _transactionDiscount;
            Model.Tax = _transactionTax;
            Model.Total = _transactionNetTotal;

            Model.PurchaseTransactionLines.Clear();
            foreach (var lines in DisplayedLines)
                Model.PurchaseTransactionLines.Add(lines.Model);
        }

        private bool IsThereASelectedLine()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private void ShowEditWindow()
        {
            var vm = new PurchaseEditVM(_selectedLine);
            var editWindow = new PurchaseEditView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editWindow.ShowDialog();
        }

        public void UpdateUIGrossTotal()
        {
            OnPropertyChanged("TransactionGrossTotal");
        }
        #endregion

        #region Delete Transaction Helper Methods
        private bool DoAllLinesHaveEnoughStock()
        {
            using (var context = UtilityMethods.createContext())
            {
                if (DisplayedLines.Any(line => !IsThereEnoughLineItemStockInDatabaseContext(context, line.Model)))
                    return false;
            }
            return true;
        }

        private static bool IsThereEnoughLineItemStockInDatabaseContext(ERPContext context, PurchaseTransactionLine purchaseTransactionLine)
        {
            var stockFromDatabase = context.Stocks.SingleOrDefault(
                stock => stock.Item.ItemID.Equals(purchaseTransactionLine.Item.ItemID) &&
                stock.Warehouse.ID.Equals(purchaseTransactionLine.Warehouse.ID));
            if (stockFromDatabase != null && stockFromDatabase.Pieces >= purchaseTransactionLine.Quantity)
                return true;
            var availableQuantity = stockFromDatabase?.Pieces ?? 0;
            MessageBox.Show(
                $"{purchaseTransactionLine.Item.Name} has only {availableQuantity / purchaseTransactionLine.Item.PiecesPerUnit} units {availableQuantity % purchaseTransactionLine.Item.PiecesPerUnit} pieces left" +
                $" at {purchaseTransactionLine.Warehouse.Name}.",
                "Invalid Quantity", MessageBoxButton.OK);
            return false;
        }

        private bool IsAllLinesSoldOrReturnedZero()
        {
            using (var context = UtilityMethods.createContext())
            {
                var purchaseTransactionInDatabase =
                    context.PurchaseTransactions.Include("PurchaseTransactionLines").Single(transaction => transaction.PurchaseID.Equals(Model.PurchaseID));
                foreach (var line in purchaseTransactionInDatabase.PurchaseTransactionLines.Where(line => line.SoldOrReturned > 0))
                {
                    MessageBox.Show($"{line.Item.Name} has been sold or returned!", "Failure", MessageBoxButton.OK);
                    return false;
                }
                return true;
            }
        }

        private bool IsAllLinesSoldOrReturnedZeroWithoutWarning()
        {
            using (var context = UtilityMethods.createContext())
            {
                var purchaseTransactionInDatabase =
                    context.PurchaseTransactions.Include("PurchaseTransactionLines").Single(transaction => transaction.PurchaseID.Equals(Model.PurchaseID));
                return !purchaseTransactionInDatabase.PurchaseTransactionLines.Any(line => line.SoldOrReturned > 0);
            }
        }
        #endregion
    }
}
