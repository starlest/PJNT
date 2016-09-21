namespace ECRP.ViewModels.Suppliers
{
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Purchase;
    using Models.Supplier;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;
    using ViewModels.Purchase;

    internal class PurchasePaymentVM : ViewModelBase
    {
        #region Purchase Transaction Backing Fields
        private decimal _purchaseTransactionGrossTotal;
        private decimal _purchaseTransactionDiscount;
        private decimal _purchaseTransactionTax;
        private decimal _purchaseTransactionTotal;
        #endregion

        #region Backing Fields
        private decimal _purchaseReturnCredits;

        private decimal _remaining;
        private decimal _useCredits;
        private decimal _pay;

        private Supplier _selectedSupplier;
        private PurchaseTransaction _selectedPurchaseTransaction;
        private string _selectedPaymentMode;

        private ICommand _confirmPaymentCommand;

        private bool _isPaymentButtonPressed;
        #endregion

        public PurchasePaymentVM()
        {
            Suppliers = new ObservableCollection<Supplier>();
            PaymentModes = new ObservableCollection<string>();
            SupplierUnpaidPurchases = new ObservableCollection<PurchaseTransaction>();
            SelectedPurchaseLines = new ObservableCollection<PurchaseTransactionLineVM>();

            UpdateSuppliers();
            UpdatePaymentMethods();
        }

        public bool IsPaymentButtonPressed
        {
            get { return _isPaymentButtonPressed; }
            set { SetProperty(ref _isPaymentButtonPressed, value, () => IsPaymentButtonPressed); }
        }

        #region Collections
        public ObservableCollection<Supplier> Suppliers { get; }

        public ObservableCollection<PurchaseTransaction> SupplierUnpaidPurchases { get; }

        public ObservableCollection<PurchaseTransactionLineVM> SelectedPurchaseLines { get; }

        public ObservableCollection<string> PaymentModes { get; }
        #endregion

        #region Purchase Transaction Properties
        public decimal PurchaseTransactionGrossTotal
        {
            get { return _purchaseTransactionGrossTotal; }
            set { SetProperty(ref _purchaseTransactionGrossTotal, value, () => PurchaseTransactionGrossTotal); }
        }

        public decimal PurchaseTransactionDiscount
        {
            get { return _purchaseTransactionDiscount; }
            set { SetProperty(ref _purchaseTransactionDiscount, value, () => PurchaseTransactionDiscount); }
        }

        public decimal PurchaseTransactionTax
        {
            get { return _purchaseTransactionTax; }
            set { SetProperty(ref _purchaseTransactionTax, value, () => PurchaseTransactionTax); }
        }

        public decimal PurchaseTransactionTotal
        {
            get { return _purchaseTransactionTotal; }
            set { SetProperty(ref _purchaseTransactionTotal, value, () => PurchaseTransactionTotal); }
        }
        #endregion

        #region Properties
        public decimal PurchaseReturnCredits
        {
            get { return _purchaseReturnCredits; }
            set { SetProperty(ref _purchaseReturnCredits, value, () => PurchaseReturnCredits); }
        }

        public decimal UseCredits
        {
            get { return _useCredits; }
            set
            {
                if (value > _purchaseReturnCredits)
                {
                    MessageBox.Show("There is not enough credits.", "Insufficient Credits", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _useCredits, value, () => UseCredits);
            }
        }

        public decimal Remaining
        {
            get { return _remaining; }
            set { SetProperty(ref _remaining, value, () => Remaining); }
        }

        public decimal Pay
        {
            get { return _pay; }
            set
            {
                if (_selectedPurchaseTransaction != null && (value <= 0 || value > _remaining))
                {
                    MessageBox.Show($"Please input a valid amount. {0} - {_remaining}", "Invalid Value", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _pay, value, () => Pay);
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                SelectedPurchaseLines.Clear();
                SetProperty(ref _selectedSupplier, value, () => SelectedSupplier);
                if (_selectedSupplier == null) return;
                UpdateSuppliers();
                ResetUI();
                UpdateSupplierUnpaidPurchases();
                PurchaseReturnCredits = value.PurchaseReturnCredits;
            }
        }

        public PurchaseTransaction SelectedPurchaseTransaction
        {
            get { return _selectedPurchaseTransaction; }
            set
            {
                if (value == null)
                    SelectedPurchaseLines.Clear();
                else
                    AssignSelectedPurchaseTransactionPropertiesToUI(value);
                SetProperty(ref _selectedPurchaseTransaction, value, () => SelectedPurchaseTransaction);
            }
        }

        public string SelectedPaymentMode
        {
            get { return _selectedPaymentMode; }
            set { SetProperty(ref _selectedPaymentMode, value, () =>SelectedPaymentMode); }
        }
        #endregion

        public ICommand ConfirmPaymentCommand
        {
            get
            {
                return _confirmPaymentCommand ?? (_confirmPaymentCommand = new RelayCommand(() =>
                {
                    if (!IsPaymentModeSelected() || !IsPaymentConfirmationYes()) return;
                    PurchaseTransactionHelper.MakePayment(_selectedPurchaseTransaction, _pay, _useCredits, _selectedPaymentMode);
                    ResetTransaction();
                    TriggerPaymentButtonStyle();
                    MessageBox.Show("Payment successfully made!", "Success", MessageBoxButton.OK);

                }));
            }
        }

        #region Helper Methods
        public void UpdateSuppliers()
        {
            var oldSelectedSupplier = _selectedSupplier;

            Suppliers.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var suppliers = context.Suppliers.Where(e => !e.Name.Equals("-"));
                foreach (var supplier in suppliers)
                    Suppliers.Add(supplier);
            }

            UpdateSelectedSupplier(oldSelectedSupplier);
        }

        private void UpdateSelectedSupplier(Supplier oldSelectedSupplier)
        {
            if (oldSelectedSupplier == null) return;
            _selectedSupplier = Suppliers.Single(supplier => supplier.ID.Equals(oldSelectedSupplier.ID));
        }

        private void UpdatePaymentMethods()
        {
            PaymentModes.Add("Cash");
            using (var context = UtilityMethods.createContext())
            {
                var bankAccounts = context.Ledger_Accounts
                    .Where(account => account.Name.Contains("Bank") && !account.Name.Contains("Expense"));
                foreach (var bank in bankAccounts)
                    PaymentModes.Add(bank.Name);
            }
        }

        private void UpdateSupplierUnpaidPurchases()
        {
            SupplierUnpaidPurchases.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var unpaidPurchases = context.PurchaseTransactions
                    .Where(e => e.Supplier.ID == _selectedSupplier.ID && e.Paid < e.Total)
                    .Include("Supplier").Include("PurchaseTransactionLines.Item");

                foreach (var purchase in unpaidPurchases)
                    SupplierUnpaidPurchases.Add(purchase);
            }
        }

        private void AssignSelectedPurchaseTransactionPropertiesToUI(PurchaseTransaction purchaseTransaction)
        {
            SelectedPurchaseLines.Clear();
            foreach (var lineVM in purchaseTransaction.PurchaseTransactionLines.Select(line => new PurchaseTransactionLineVM { Model = line }))
                SelectedPurchaseLines.Add(lineVM);   
            PurchaseTransactionGrossTotal = purchaseTransaction.GrossTotal;
            PurchaseTransactionDiscount = purchaseTransaction.Discount;
            PurchaseTransactionTax = purchaseTransaction.Tax;
            PurchaseTransactionTotal = purchaseTransaction.Total;
            Remaining = purchaseTransaction.Total - purchaseTransaction.Paid;
        }

        private bool IsPaymentModeSelected()
        {
            if (_selectedPaymentMode != null) return true;
            MessageBox.Show("Please select a payment mode.", "Invalid Selection", MessageBoxButton.OK);
            return false;
        }

        private static bool IsPaymentConfirmationYes()
        {
            return MessageBox.Show("Confirm Payment?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private void ResetTransaction()
        {
            SelectedSupplier = null;
            PurchaseReturnCredits = 0;
            
            SelectedPaymentMode = null;
            SelectedPurchaseTransaction = null;

            PurchaseTransactionGrossTotal = 0;
            PurchaseTransactionDiscount = 0;
            PurchaseTransactionTax = 0;
            PurchaseTransactionTotal = 0;

            UseCredits = 0;
            Remaining = 0;
            Pay = 0;

            UpdateSuppliers();
            UpdatePaymentMethods();
            SelectedPurchaseLines.Clear();
            SupplierUnpaidPurchases.Clear();
        }

        private void ResetUI()
        {
            SelectedPurchaseTransaction = null;
            PurchaseReturnCredits = 0;
            PurchaseTransactionGrossTotal = 0;
            PurchaseTransactionTotal = 0;
            Remaining = 0;
            UseCredits = 0;
            Pay = 0;
        }

        private void TriggerPaymentButtonStyle()
        {
            IsPaymentButtonPressed = true;
            IsPaymentButtonPressed = false;
        }
        #endregion
    }
}
