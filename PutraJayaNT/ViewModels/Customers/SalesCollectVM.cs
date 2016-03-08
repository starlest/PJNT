using System;
using PutraJayaNT.Utilities.Database.Customer;
using PutraJayaNT.Utilities.Database.Ledger;

namespace PutraJayaNT.ViewModels.Customers
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models.Accounting;
    using Models.Sales;
    using Utilities;
    using Customer;
    using Sales;

    internal class SalesCollectVM : ViewModelBase
    {
        private readonly DateTime _date;

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
        private decimal _collect;

        private ICommand _confirmPaymentCommand;

        public SalesCollectVM()
        {
            Customers = new ObservableCollection<CustomerVM>();
            CustomerUnpaidSalesTransactions = new ObservableCollection<SalesTransaction>();
            SelectedSalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            PaymentModes = new ObservableCollection<string>();

            UpdateCustomers();
            UpdatePaymentModes();

            _date = UtilityMethods.GetCurrentDate().Date;
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
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");
                if (_selectedCustomer == null) return;
                UpdateUIAccordingToSelectedCustomer();
            }
        }

        public SalesTransaction SelectedSalesTransaction
        {
            get { return _selectedSalesTransaction; }
            set
            {
                SetProperty(ref _selectedSalesTransaction, value, "SelectedSalesTransaction");

                if (_selectedSalesTransaction == null) return;

                UpdateSelectedSalesTransactionLines();
                SalesTransactionGrossTotal = _selectedSalesTransaction.GrossTotal;
                SalesTransactionDiscount = _selectedSalesTransaction.Discount;
                SalesTransactionSalesExpense = _selectedSalesTransaction.SalesExpense;
                SalesTransactionTotal = _selectedSalesTransaction.Total;
                Remaining = _selectedSalesTransaction.Total - _selectedSalesTransaction.Paid;
            }
        }

        public string SelectedPaymentMode
        {
            get { return _selectedPaymentMode; }
            set { SetProperty(ref _selectedPaymentMode, value, "SelectedPaymentMode"); }
        }

        public decimal SalesReturnCredits
        {
            get { return _salesReturnCredits; }
            set { SetProperty(ref _salesReturnCredits, value, "SalesReturnCredits"); }
        }
        #endregion

        #region Sales Transaction Properties
        public decimal SalesTransactionGrossTotal
        {
            get { return _salesTransactionGrossTotal; }
            set { SetProperty(ref _salesTransactionGrossTotal, value, "SalesTransactionGrossTotal"); }
        }

        public decimal SalesTransactionDiscount
        {
            get { return _salesTransactionDiscount; }
            set { SetProperty(ref _salesTransactionDiscount, value, "SalesTransactionDiscount"); }
        }

        public decimal SalesTransactionSalesExpense
        {
            get { return _salesTransactionSalesExpense; }
            set { SetProperty(ref _salesTransactionSalesExpense, value, "SalesTransactionSalesExpense"); }
        }

        public decimal SalesTransactionTotal
        {
            get { return _salesTransactionTotal; }
            set
            {
                SetProperty(ref _salesTransactionTotal, value, "SalesTransactionTotal");
                if (_salesTransactionTotal == 0) return;
                Remaining = _salesTransactionTotal - _selectedSalesTransaction.Paid - _useCredits;
            }
        }
        #endregion

        #region Payment Properties
        public decimal UseCredits
        {
            get { return _useCredits; }
            set
            {
                if (!IsCreditsValueValid(value)) return;
                SetProperty(ref _useCredits, value, "UseCredits");
                Remaining = _salesTransactionTotal - _selectedSalesTransaction.Paid - _useCredits;
            }
        }

        public decimal Remaining
        {
            get { return _remaining; }
            set {  SetProperty(ref _remaining, value, "Remaining");  }
        }

        public decimal Collect
        {
            get { return _collect; }
            set
            {
                if (!IsCollectValueValid(value)) return;
                SetProperty(ref _collect, value, "Collect");
            }
        }
        #endregion

        public ICommand ConfirmPaymentCommand
        {
            get
            {
                return _confirmPaymentCommand ?? (_confirmPaymentCommand = new RelayCommand(() =>
                {
                    if (_collect == null)
                    {
                        MessageBox.Show("Please enter payment amount", "Empty Field", MessageBoxButton.OK);
                        return;
                    }

                    if (_selectedPaymentMode == null)
                    {
                        MessageBox.Show("Please select a payment mode.", "No Selection", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm Payment?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            _selectedSalesTransaction = context.SalesTransactions
                            .Include("Customer")
                            .Where(e => e.SalesTransactionID.Equals(_selectedSalesTransaction.SalesTransactionID)).FirstOrDefault();

                            _selectedSalesTransaction.Paid += (decimal)_collect + _useCredits;
                            _selectedSalesTransaction.Customer.SalesReturnCredits -= _useCredits;

                            var accountsReceivableName = _selectedCustomer.Name + " Accounts Receivable";

                            if (_collect > 0)
                            {
                                var transaction = new LedgerTransaction();

                                if (!LedgerDBHelper.AddTransaction(context, transaction, _date, _selectedSalesTransaction.SalesTransactionID, "Sales Transaction Receipt")) return;
                                context.SaveChanges();

                                LedgerDBHelper.AddTransactionLine(context, transaction, _selectedPaymentMode, "Debit", (decimal)_collect);
                                LedgerDBHelper.AddTransactionLine(context, transaction, accountsReceivableName, "Credit", (decimal)_collect);
                            }
                            context.SaveChanges();

                            ts.Complete();
                        }

                        ResetTransaction();
                    }

                }));
            }
        }

        #region Helper Methods
        private void UpdateCustomers()
        {
            var oldSelectedCustomer = _selectedCustomer;

            Customers.Clear();
            var customersFromDatabase = DatabaseCustomerHelper.GetAll();
            foreach (var customer in customersFromDatabase)
                Customers.Add(new CustomerVM { Model = customer });

            UpdateSelectedCustomer(oldSelectedCustomer);
        }

        private void UpdateSelectedCustomer(CustomerVM oldSelectedCustomer)
        {
            if (oldSelectedCustomer == null) return;
            SelectedCustomer = Customers.FirstOrDefault(customer => customer.ID.Equals(oldSelectedCustomer.ID));
        }

        private void UpdatePaymentModes()
        {
            var oldSelectedPaymentMode = _selectedPaymentMode;

            PaymentModes.Clear();
            PaymentModes.Add("Cash");

            var banksFromDatabase =
                DatabaseLedgerAccountHelper.GetWithoutLines(ledgerAccount => ledgerAccount.Name.Contains("Bank") && !ledgerAccount.Name.Contains("Expense"));

            foreach (var bank in banksFromDatabase)
                PaymentModes.Add(bank.Name);

            UpdateSelectedPaymentMode(oldSelectedPaymentMode);
        }

        private void UpdateSelectedPaymentMode(string oldSelectedPaymentMode)
        {
            if (oldSelectedPaymentMode == null) return;
            SelectedPaymentMode = PaymentModes.FirstOrDefault(paymentMode => paymentMode.Equals(oldSelectedPaymentMode));
        }

        private void UpdateUIAccordingToSelectedCustomer()
        {
            SalesReturnCredits = _selectedCustomer.SalesReturnCredits;
            UpdateCustomerUnpaidSalesTransactions();
            SelectedSalesTransactionLines.Clear();
        }

        private void UpdateCustomerUnpaidSalesTransactions()
        {
            CustomerUnpaidSalesTransactions.Clear();

            using (var context = new ERPContext())
            {
                var transactions = context.SalesTransactions
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Item")
                    .Include("SalesTransactionLines.Warehouse")
                    .Where(e => e.InvoiceIssued != null && e.Customer.ID.Equals(_selectedCustomer.ID) && (e.Paid < e.Total))
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

        private bool IsCreditsValueValid(decimal value)
        {
            if (value >= 0 && value <= _salesReturnCredits) return true;
            MessageBox.Show($"The available number of credits is {_salesReturnCredits}", "Invalid Value", MessageBoxButton.OK);
            return false;
        }

        private bool IsCollectValueValid(decimal value)
        {
            if (value >= 0 && value <= _remaining) return true;
            MessageBox.Show($"The valid range is 0 - {_remaining}", "Invalid Value", MessageBoxButton.OK);
            return false;
        }

        private void ResetTransaction()
        {
            SelectedCustomer = null;
            SelectedPaymentMode = null;
            SelectedSalesTransaction = null;
            SalesTransactionTotal = 0;
            SalesReturnCredits = 0;
            UseCredits = 0;
            Remaining = 0;
            Collect = 0;
            SelectedSalesTransactionLines.Clear();
            CustomerUnpaidSalesTransactions.Clear();
        }
        #endregion
    }
}
