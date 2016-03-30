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

        #region Transaction backing fields
        private string _transactionID;
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

        public PurchaseVM()
        {
            NewEntryVM = new PurchaseNewEntryVM(this);

            Model = new PurchaseTransaction();
            DisplayedLines = new ObservableCollection<PurchaseTransactionLineVM>();
            Suppliers = new ObservableCollection<SupplierVM>();
            SupplierItems = new ObservableCollection<ItemVM>();
            Warehouses = new ObservableCollection<WarehouseVM>();

            UpdateSuppliers();

            TransactionDate = UtilityMethods.GetCurrentDate().Date;
            TransactionDueDate = UtilityMethods.GetCurrentDate().Date;

            SetTransactionID();
            Model.DueDate = _transactionDueDate;
            Model.Total = 0;

            _notEditMode = true;
            _isTransactionNotPaid = true;
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
                using (var context = new ERPContext(UtilityMethods.GetDBName()))
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
                    if (DisplayedLines.Count == 0 || !IsSaveConfirmationYes()) return;

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

        public ICommand NewTransactionCommand => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));
        #endregion

        #region Helper Methods
        private void UpdateSuppliers()
        {
            Suppliers.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
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
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var items = context.Inventory.Include("Suppliers").Where(item => item.Active).OrderBy(item => item.Name).ToList();
                foreach (var item in items.Where(item => item.Active && item.Suppliers.Contains(_transactionSupplier.Model)))
                    SupplierItems.Add(new ItemVM { Model= item });
            }
        }

        private void UpdateWarehouses()
        {
            Warehouses.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
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
            _transactionID = "P" + (long)((year - 2000) * 100 + month) * 1000000;

            string lastEntryID = null;
            using(var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var lastPurchaseTransaction =
                    context.PurchaseTransactions.Where(
                        PurchaseTransaction =>
                            string.Compare(PurchaseTransaction.PurchaseID, _transactionID, StringComparison.Ordinal) >= 0 &&
                            PurchaseTransaction.PurchaseID.Substring(0, 1).Equals("P"))
                        .OrderByDescending(PurchaseTransaction => PurchaseTransaction.PurchaseID)
                        .FirstOrDefault();
                if (lastPurchaseTransaction != null) lastEntryID = lastPurchaseTransaction.PurchaseID;
            }

            if (lastEntryID != null) _transactionID = "P" + (Convert.ToInt64(lastEntryID.Substring(1)) + 1);

            Model.PurchaseID = _transactionID;
            OnPropertyChanged("TransactionID");
        }

        private void ResetTransaction()
        {
            NewEntryVM.ResetEntryFields();
            NewEntryVM.NewEntryWarehouse = null;

            IsTransactionNotPaid = true;
            NotEditMode = true;
            IsTransactionTaxCheckBoxSelected = false;

            Model = new PurchaseTransaction();
            TransactionSupplier = null;
            TransactionDOID = null;
            TransactionTax = 0;
            TransactionDiscount = 0;
            TransactionNote = null;
            TransactionDate = UtilityMethods.GetCurrentDate().Date;
            TransactionDueDate = UtilityMethods.GetCurrentDate().Date;

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
    }
}
