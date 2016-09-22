namespace ECRP.ViewModels.Customers
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using Models.Sales;
    using MVVMFramework;
    using Services;
    using Utilities;
    using Utilities.ModelHelpers;
    using ViewModels.Sales;

    public class SalesCollectVM : ViewModelBase
    {
        #region Backing Fields

        private CustomerVM _selectedCustomer;
        private SalesTransaction _selectedSalesTransaction;
        private string _selectedPaymentMode;

        private decimal _salesTransactionGrossTotal;
        private decimal _salesTransactionDiscount;
        private decimal _salesTransactionSalesExpense;
        private decimal _salesTransactionTotal;

        private decimal _salesReturnCredits;
        private decimal _useCredits;
        private decimal _remaining;
        private decimal _collectionAmount;

        private bool _isCollectionSuccess;

        private ICommand _confirmCollectionCommand;

        #endregion

        public SalesCollectVM()
        {
            Customers = new ObservableCollection<CustomerVM>();
            CustomerUnpaidSalesTransactions = new ObservableCollection<SalesTransaction>();
            SelectedSalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            PaymentModes = new ObservableCollection<string>();

            UpdateCustomers();
            UpdatePaymentModes();
        }

        #region Collections

        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<string> PaymentModes { get; }

        public ObservableCollection<SalesTransaction> CustomerUnpaidSalesTransactions { get; }

        public ObservableCollection<SalesTransactionLineVM> SelectedSalesTransactionLines { get; }

        #endregion

        #region Properties

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value, () => SelectedCustomer);
                if (_selectedCustomer == null) return;
                UpdatePropertiesAccordingToSelectedCustomer();
                UpdateCustomers();
            }
        }

        public SalesTransaction SelectedSalesTransaction
        {
            get { return _selectedSalesTransaction; }
            set
            {
                SetProperty(ref _selectedSalesTransaction, value, () => SelectedSalesTransaction);

                if (_selectedSalesTransaction == null) return;

                UpdatePaymentModes();
                ResetSalesTransactionAndCollectionProperties();
                UpdateSalesTransactionProperties();
            }
        }

        public decimal SalesReturnCredits
        {
            get { return _salesReturnCredits; }
            set { SetProperty(ref _salesReturnCredits, value, () => SalesReturnCredits); }
        }

        public bool IsCollectionSuccess
        {
            get { return _isCollectionSuccess; }
            set { SetProperty(ref _isCollectionSuccess, value, () => IsCollectionSuccess); }
        }

        public ICommand ConfirmCollectionCommand
        {
            get
            {
                return _confirmCollectionCommand ?? (_confirmCollectionCommand = new RelayCommand(() =>
                {
                    if (!IsThereAmountCollected() || !IsPaymentModeSelected() || !IsCollectionConfirmationYes()) return;

                    IsCollectionSuccess = SalesTransactionService.Collect(_selectedSalesTransaction, _useCredits,
                        _collectionAmount, _selectedPaymentMode);

                    if (_isCollectionSuccess)
                    {
                        MessageBox.Show("Succesfully collected!", "Success", MessageBoxButton.OK);
                        ResetTransaction();
                    }
                    else
                        MessageBox.Show("There was a problem making the collection. Please contact Edwin.",
                            "Invalid Amounts",
                            MessageBoxButton.OK);
                }));
            }
        }

        #endregion

        #region Sales Transaction Properties

        public decimal SalesTransactionGrossTotal
        {
            get { return _salesTransactionGrossTotal; }
            set { SetProperty(ref _salesTransactionGrossTotal, value, () => SalesTransactionGrossTotal); }
        }

        public decimal SalesTransactionDiscount
        {
            get { return _salesTransactionDiscount; }
            set { SetProperty(ref _salesTransactionDiscount, value, () => SalesTransactionDiscount); }
        }

        public decimal SalesTransactionSalesExpense
        {
            get { return _salesTransactionSalesExpense; }
            set { SetProperty(ref _salesTransactionSalesExpense, value, () => SalesTransactionSalesExpense); }
        }

        public decimal SalesTransactionTotal
        {
            get { return _salesTransactionTotal; }
            set
            {
                SetProperty(ref _salesTransactionTotal, value, () => SalesTransactionTotal);
                if (_salesTransactionTotal == 0) return;
                Remaining = _salesTransactionTotal - _selectedSalesTransaction.Paid - _useCredits;
            }
        }

        #endregion

        #region Collection Properties

        public decimal UseCredits
        {
            get { return _useCredits; }
            set
            {
                if (!IsCreditsValueValid(value)) return;
                SetProperty(ref _useCredits, value, () => UseCredits);
                if (_useCredits <= 0) return;
                UpdateRemaining();
            }
        }

        public decimal Remaining
        {
            get { return _remaining; }
            set { SetProperty(ref _remaining, value, () => Remaining); }
        }

        public decimal CollectionAmount
        {
            get { return _collectionAmount; }
            set
            {
                if (!IsCollectValueValid(value)) return;
                SetProperty(ref _collectionAmount, value, () => CollectionAmount);
            }
        }

        public string SelectedPaymentMode
        {
            get { return _selectedPaymentMode; }
            set { SetProperty(ref _selectedPaymentMode, value, () => SelectedPaymentMode); }
        }

        #endregion

        #region Helper Methods

        private void UpdateCustomers()
        {
            var oldSelectedCustomer = _selectedCustomer;

            Customers.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var customersFromDatabase = context.Customers.OrderBy(customer => customer.Name);
                foreach (var customer in customersFromDatabase)
                    Customers.Add(new CustomerVM { Model = customer });
            }

            UpdateSelectedCustomer(oldSelectedCustomer);
        }

        private void UpdateSelectedCustomer(CustomerVM oldSelectedCustomer)
        {
            if (oldSelectedCustomer == null) return;
            _selectedCustomer = Customers.FirstOrDefault(customer => customer.ID.Equals(oldSelectedCustomer.ID));
            if (_selectedCustomer != null) SalesReturnCredits = _selectedCustomer.SalesReturnCredits;
        }

        private void UpdatePaymentModes()
        {
            var oldSelectedPaymentMode = _selectedPaymentMode;

            PaymentModes.Clear();
            PaymentModes.Add("Cash");

            using (var context = UtilityMethods.createContext())
            {
                var banksFromDatabase =
                    context.Ledger_Accounts.Where(
                        ledgerAccount => ledgerAccount.Name.Contains("Bank") && !ledgerAccount.Name.Contains("Expense"))
                        .OrderBy(account => account.Name);

                foreach (var bank in banksFromDatabase)
                    PaymentModes.Add(bank.Name);
            }

            UpdateSelectedPaymentMode(oldSelectedPaymentMode);
        }

        private void UpdateSelectedPaymentMode(string oldSelectedPaymentMode)
        {
            if (oldSelectedPaymentMode == null) return;
            SelectedPaymentMode = PaymentModes.FirstOrDefault(paymentMode => paymentMode.Equals(oldSelectedPaymentMode));
        }

        private void UpdateSalesTransactionProperties()
        {
            UpdateSelectedSalesTransactionLines();
            SalesTransactionGrossTotal = _selectedSalesTransaction.GrossTotal;
            SalesTransactionDiscount = _selectedSalesTransaction.Discount;
            SalesTransactionSalesExpense = _selectedSalesTransaction.SalesExpense;
            SalesTransactionTotal = _selectedSalesTransaction.NetTotal;
            Remaining = _selectedSalesTransaction.NetTotal - _selectedSalesTransaction.Paid;
        }

        private void UpdatePropertiesAccordingToSelectedCustomer()
        {
            ResetSalesTransactionAndCollectionProperties();
            SalesReturnCredits = _selectedCustomer.SalesReturnCredits;
            UpdateCustomerUnpaidSalesTransactions();
            SelectedSalesTransactionLines.Clear();
        }

        private void UpdateCustomerUnpaidSalesTransactions()
        {
            CustomerUnpaidSalesTransactions.Clear();

            using (var context = UtilityMethods.createContext())
            {
                var transactions = context.SalesTransactions
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Item")
                    .Include("SalesTransactionLines.Warehouse")
                    .Where(
                        e =>
                            e.InvoiceIssued != null && e.Customer.ID.Equals(_selectedCustomer.ID) &&
                            (e.Paid < e.NetTotal))
                    .ToList();

                foreach (var transaction in transactions)
                    CustomerUnpaidSalesTransactions.Add(transaction);
            }
        }

        private void UpdateSelectedSalesTransactionLines()
        {
            SelectedSalesTransactionLines.Clear();

            foreach (var line in _selectedSalesTransaction.SalesTransactionLines.ToList())
                SelectedSalesTransactionLines.Add(new SalesTransactionLineVM { Model = line });
        }

        private void ResetSalesTransactionAndCollectionProperties()
        {
            SelectedPaymentMode = null;
            SalesTransactionTotal = 0;
            SalesReturnCredits = 0;
            SalesTransactionGrossTotal = 0;
            UseCredits = 0;
            Remaining = 0;
            CollectionAmount = 0;
        }

        private void ResetTransaction()
        {
            SelectedCustomer = null;
            SelectedSalesTransaction = null;
            ResetSalesTransactionAndCollectionProperties();
            SelectedSalesTransactionLines.Clear();
            CustomerUnpaidSalesTransactions.Clear();
        }

        private void UpdateRemaining()
        {
            CollectionAmount = 0;
            Remaining = _salesTransactionTotal - _selectedSalesTransaction.Paid - _useCredits;
        }

        #endregion

        #region Checker Methods

        private bool IsThereAmountCollected()
        {
            if (_useCredits != 0 || _collectionAmount != 0) return true;
            MessageBox.Show("Please input a collection amount.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private bool IsCreditsValueValid(decimal value)
        {
            if (value >= 0 && value <= _salesReturnCredits) return true;
            MessageBox.Show($"The available number of credits is {_salesReturnCredits}", "Invalid Value",
                MessageBoxButton.OK);
            return false;
        }

        private bool IsCollectValueValid(decimal value)
        {
            if (value >= 0 && value <= _remaining) return true;
            MessageBox.Show($"The valid range is 0 - {_remaining}", "Invalid Value", MessageBoxButton.OK);
            return false;
        }

        private bool IsPaymentModeSelected()
        {
            if (_selectedPaymentMode != null) return true;
            MessageBox.Show("Please select a payment mode.", "Incomplete Selections", MessageBoxButton.OK);
            return false;
        }

        private static bool IsCollectionConfirmationYes()
        {
            return MessageBox.Show("Confirm collection?", "Confirmation", MessageBoxButton.YesNo) ==
                   MessageBoxResult.Yes;
        }

        #endregion
    }
}