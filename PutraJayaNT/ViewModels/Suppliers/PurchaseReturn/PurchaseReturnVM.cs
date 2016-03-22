namespace PutraJayaNT.ViewModels.Suppliers.PurchaseReturn
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

    internal class PurchaseReturnVM : ViewModelBase<PurchaseReturnTransaction>
    {
        private bool _notEditing;
        private PurchaseReturnTransactionLineVM _selectedLine;

        #region Purchase Return Transaction Backing Fields
        private string _purchaseReturnTransactionID;
        private DateTime _purchaseReturnTransactionDate;
        private decimal _purchaseReturnTransactionNetTotal;
        #endregion

        #region Selected Purchase Transaction Backing Fields
        string _selectedPurchaseTransactionID;
        private SupplierVM _selectedPurchaseTransactionSupplier;
        PurchaseTransactionLineVM _selectedPurchaseTransactionLine;
        #endregion

        #region Commands Backing Fields
        private ICommand _newCommand;
        private ICommand _saveCommand;
        private ICommand _deleteLineCommand;
        #endregion

        public PurchaseReturnVM()
        {
            NewEntryVM = new PurchaseReturnNewEntryVM(this);
            Model = new PurchaseReturnTransaction();
            Warehouses = new ObservableCollection<WarehouseVM>();
            PurchaseTransactionLines = new ObservableCollection<PurchaseTransactionLineVM>();
            PurchaseReturnTransactionLines = new ObservableCollection<PurchaseReturnTransactionLineVM>();
            PurchaseReturnTransactionDate = UtilityMethods.GetCurrentDate();
            UpdateWarehouses();
            _notEditing = true;
            SetPurchaseReturnTransactionID();
        }

        public PurchaseReturnNewEntryVM NewEntryVM { get; }

        public bool NotEditing
        {
            get { return _notEditing; }
            set { SetProperty(ref _notEditing, value, () => NotEditing); }
        }

        public PurchaseReturnTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }

        #region Collections
        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<PurchaseTransactionLineVM> PurchaseTransactionLines { get; }

        public ObservableCollection<PurchaseReturnTransactionLineVM> PurchaseReturnTransactionLines { get; }
        #endregion

        #region Purchase Return Transaction Properties
        public string PurchaseReturnTransactionID
        {
            get { return _purchaseReturnTransactionID; }
            set
            {
                using (var context = new ERPContext())
                {
                    var purchaseReturnTransactionFromDatabase = context.PurchaseReturnTransactions
                        .Include("PurchaseReturnTransactionLines")
                        .Include("PurchaseReturnTransactionLines.Warehouse")
                        .Include("PurchaseReturnTransactionLines.Item")
                        .SingleOrDefault(
                        purchaseReturnTransaction => purchaseReturnTransaction.PurchaseReturnTransactionID.Equals(value));

                    if (!IsPurchaseReturnTransactionInDatabase(purchaseReturnTransactionFromDatabase)) return;
                    SetEditTransactionMode(purchaseReturnTransactionFromDatabase);
                }
                SetProperty(ref _purchaseReturnTransactionID, value, () => PurchaseReturnTransactionID);
            }
        }

        public DateTime PurchaseReturnTransactionDate
        {
            get { return _purchaseReturnTransactionDate; }
            set { SetProperty(ref _purchaseReturnTransactionDate, value, () => PurchaseReturnTransactionDate); }
        }

        public decimal PurchaseReturnTransactionNetTotal
        {
            get { return _purchaseReturnTransactionNetTotal; }
            set { SetProperty(ref _purchaseReturnTransactionNetTotal, value, () => PurchaseReturnTransactionNetTotal); }
        }
        #endregion

        #region Selected Purchase Transaction Properties
        public string SelectedPurchaseTransactionID
        {
            get { return _selectedPurchaseTransactionID; }
            set
            {
                SetProperty(ref _selectedPurchaseTransactionID, value, "SelectedPurchaseTransactionID");
                if (_selectedPurchaseTransactionID == null) return;

                using (var context = new ERPContext())
                {
                    var purchaseTransactionFromDatabase = context.PurchaseTransactions
                        .Include("Supplier")
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .SingleOrDefault(transaction => transaction.PurchaseID.Equals(value));

                    if (!IsPurchaseTransactionInDatabase(purchaseTransactionFromDatabase)) return;
                    SetNewTransactionMode(purchaseTransactionFromDatabase);
                }
            }
        }

        public SupplierVM SelectedPurchaseTransactionSupplier
        {
            get { return _selectedPurchaseTransactionSupplier; }
            set { SetProperty(ref _selectedPurchaseTransactionSupplier, value, () => SelectedPurchaseTransactionSupplier); }
        }

        public PurchaseTransactionLineVM SelectedPurchaseTransactionLine
        {
            get { return _selectedPurchaseTransactionLine; }
            set
            {
                SetProperty(ref _selectedPurchaseTransactionLine, value, "SelectedPurchaseTransactionLine");
                if (_selectedPurchaseTransactionLine != null)
                    UpdateReturnEntryProperties();
            }
        }
        #endregion

        #region Commands
        public ICommand NewCommand => _newCommand ?? (_newCommand = new RelayCommand(ResetTransaction));

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (_selectedLine != null &&
                        MessageBox.Show("Confirm Deletion?", "Confirmation", MessageBoxButton.YesNo)
                        == MessageBoxResult.No)
                        return;
                    PurchaseReturnTransactionLines.Remove(_selectedLine);
                    if (_selectedLine != null) _purchaseReturnTransactionNetTotal -= _selectedLine.Total;
                    UpdateUINetTotal();
                }));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm transaction?", "Confirmation", MessageBoxButton.YesNo) ==
                        MessageBoxResult.No) return;
                    SetPurchaseReturnTransactionID();   // To avoid simultaneous input into the same ID
                    SetPurchaseReturnTransactionModelPropertiesToVMProperties();
                    PurchaseReturnTransactionHelper.AddPurchaseReturnTransactionToDatabase(Model);
                    if (PurchaseReturnTransactionHelper.IsLastSaveSuccessful)
                        MessageBox.Show("Transaction successfully saved!", "Success", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }
        #endregion

        #region Helper Methods
        public void UpdateWarehouses()
        {
            Warehouses.Clear();
            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.OrderBy(warehouse => warehouse.Name);
                foreach (var warehouse in warehouses)
                    Warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void SetPurchaseReturnTransactionID()
        {
            var year = _purchaseReturnTransactionDate.Year;
            var month = _purchaseReturnTransactionDate.Month;

            var newEntryID = "PR" + (long)((year - 2000) * 100 + month) * 1000000;

            string lastEntryID = null;
            using (var context = new ERPContext())
            {
                var latestPurchaseReturnTransaction = context.PurchaseReturnTransactions.Where(
                    transaction => string.Compare(transaction.PurchaseReturnTransactionID, newEntryID, StringComparison.Ordinal) >= 0)
                    .OrderByDescending(transaction => transaction.PurchaseReturnTransactionID)
                    .FirstOrDefault();
                if (latestPurchaseReturnTransaction != null) lastEntryID = latestPurchaseReturnTransaction.PurchaseReturnTransactionID;
            }

            if (lastEntryID != null) newEntryID = "PR" + (Convert.ToInt64(lastEntryID.Substring(2)) + 1);

            Model.PurchaseReturnTransactionID = newEntryID;
            _purchaseReturnTransactionID = newEntryID;
            OnPropertyChanged("PurchaseReturnTransactionID");
        }

        private static bool IsPurchaseReturnTransactionInDatabase(PurchaseReturnTransaction purchaseReturnTransaction)
        {
            if (purchaseReturnTransaction != null) return true;
            MessageBox.Show("Purchase return transaction does not exists.", "Invalid ID", MessageBoxButton.OK);
            return false;
        }

        private void SetEditTransactionMode(PurchaseReturnTransaction purchaseReturnTransaction)
        {
            Model = purchaseReturnTransaction;
            SelectedPurchaseTransactionID = Model.PurchaseTransaction.PurchaseID;
            PurchaseReturnTransactionDate = Model.Date;
            _purchaseReturnTransactionNetTotal = 0;
            foreach (var line in Model.PurchaseReturnTransactionLines)
            {
                PurchaseReturnTransactionLines.Add(new PurchaseReturnTransactionLineVM { Model = line });
                _purchaseReturnTransactionNetTotal += line.Total;
            }
            UpdateUINetTotal();
            NotEditing = false;
        }

        private static bool IsPurchaseTransactionInDatabase(PurchaseTransaction purchaseTransaction)
        {
            if (purchaseTransaction != null) return true;
            MessageBox.Show("Please check if the transaction has been issued or exists.", "Invalid Sales Transaction", MessageBoxButton.OK);
            return false;
        }

        private void SetNewTransactionMode(PurchaseTransaction purchaseTransaction)
        {
            NewEntryVM.ResetEntryFields();
            Model.PurchaseTransaction = purchaseTransaction;
            UpdatePurchaseTransactionLines(purchaseTransaction);
            SelectedPurchaseTransactionSupplier = new SupplierVM { Model = Model.PurchaseTransaction.Supplier };
            SelectedPurchaseTransactionLine = null;
            NotEditing = true;
        }

        private void UpdatePurchaseTransactionLines(PurchaseTransaction purchaseTransaction)
        {
            PurchaseTransactionLines.Clear();
            foreach (var line in purchaseTransaction.PurchaseTransactionLines.ToList())
                PurchaseTransactionLines.Add(new PurchaseTransactionLineVM { Model = line });
        }

        private void UpdateReturnEntryProperties()
        {
            NewEntryVM.PurchaseReturnEntryWarehouse = Warehouses.Single(
                warehouse => warehouse.ID.Equals(_selectedPurchaseTransactionLine.Warehouse.ID));
            NewEntryVM.PurchaseReturnEntryProduct = _selectedPurchaseTransactionLine.Item.Name;
            NewEntryVM.PurchaseReturnEntryUnits = 0;
            NewEntryVM.PurchaseReturnEntryPieces = 0;
        }

        private void SetPurchaseReturnTransactionModelPropertiesToVMProperties()
        {
            Model.Date = _purchaseReturnTransactionDate;
            Model.NetTotal = _purchaseReturnTransactionNetTotal;
            AddPurchaseReturnTransactionLinesModelsIntoPurchaseReturnTransactionModel();
        }

        private void AddPurchaseReturnTransactionLinesModelsIntoPurchaseReturnTransactionModel()
        {
            foreach (var purchaseReturnTransactionLine in PurchaseReturnTransactionLines)
                Model.PurchaseReturnTransactionLines.Add(purchaseReturnTransactionLine.Model);
        }

        private void ResetTransaction()
        {
            PurchaseReturnTransactionLines.Clear();
            PurchaseTransactionLines.Clear();

            Model = new PurchaseReturnTransaction();
            NotEditing = true;

            SetPurchaseReturnTransactionID();
            PurchaseReturnTransactionDate = UtilityMethods.GetCurrentDate().Date;
            PurchaseReturnTransactionNetTotal = 0;

            SelectedPurchaseTransactionID = null;
            SelectedPurchaseTransactionSupplier = null;
            SelectedPurchaseTransactionLine = null;

            NewEntryVM.ResetEntryFields();

            UpdateWarehouses();
        }

        public void UpdateUINetTotal()
        {
            OnPropertyChanged("PurchaseReturnTransactionNetTotal");
        }
        #endregion
    }
}
