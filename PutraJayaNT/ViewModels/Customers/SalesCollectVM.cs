using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Customers
{
    class SalesCollectVM : ViewModelBase
    {
        ObservableCollection<CustomerVM> _customers;
        ObservableCollection<string> _collectmentModes;
        ObservableCollection<SalesTransaction> _customerUnpaidSalesTransactions;
        ObservableCollection<SalesTransactionLineVM> _selectedSalesTransactionLines;

        CustomerVM _selectedCustomer;
        DateTime _date;
        SalesTransaction _selectedSalesTransaction;
        string _selectedPaymentMode;
        decimal _salesReturnCredits;

        decimal _grossTotal;
        decimal _discount;
        decimal _salesExpense;
        decimal _total;

        decimal _useCredits;
        decimal _remaining;
        decimal? _collect;

        ICommand _confirmPaymentCommand;

        public SalesCollectVM()
        {
            _customers = new ObservableCollection<CustomerVM>();
            _customerUnpaidSalesTransactions = new ObservableCollection<SalesTransaction>();
            _selectedSalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _collectmentModes = new ObservableCollection<string>();
            _date = UtilityMethods.GetCurrentDate().Date;
            UpdatePaymentModes();
            UpdateCustomers();
        }

        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
        }

        public ObservableCollection<string> PaymentModes
        {
            get { return _collectmentModes; }
        }

        public ObservableCollection<SalesTransaction> CustomerUnpaidSalesTransactions
        {
            get { return _customerUnpaidSalesTransactions; }
        }

        public ObservableCollection<SalesTransactionLineVM> SelectedSalesTransactionLines
        {
            get { return _selectedSalesTransactionLines; }
        }

        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, "Date"); }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                UpdateCustomers();
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                SalesReturnCredits = _selectedCustomer.SalesReturnCredits;
                UpdateCustomerUnpaidSalesTransactions();
                _selectedSalesTransactionLines.Clear();
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
                GrossTotal = _selectedSalesTransaction.GrossTotal;
                Discount = _selectedSalesTransaction.Discount;
                SalesExpense = _selectedSalesTransaction.SalesExpense;
                Total = _selectedSalesTransaction.Total;
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

        private void UpdatePaymentModes()
        {
            _collectmentModes.Clear();

            _collectmentModes.Add("Cash");

            using (var context = new ERPContext())
            {
                var banks = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank") && !e.Name.Contains("Expense"))
                    .ToList();

                foreach (var bank in banks)
                    _collectmentModes.Add(bank.Name);
            }
        }

        private void UpdateCustomers()
        {
            _customers.Clear();

            using (var context = new ERPContext())
            {
                var customers = context.Customers.OrderBy(e => e.Name).ToList();

                foreach (var customer in customers)
                    _customers.Add(new CustomerVM { Model = customer });
            }
        }

        private void UpdateCustomerUnpaidSalesTransactions()
        {
            _customerUnpaidSalesTransactions.Clear();

            using (var context = new ERPContext())
            {
                var transactions = context.SalesTransactions
                    .Include("TransactionLines")
                    .Include("TransactionLines.Item")
                    .Include("TransactionLines.Warehouse")
                    .Where(e => e.InvoiceIssued != null && e.Customer.ID.Equals(_selectedCustomer.ID) && (e.Paid < e.Total))
                    .ToList();

                foreach (var transaction in transactions)
                    _customerUnpaidSalesTransactions.Add(transaction);
            }
        }

        private void UpdateSelectedSalesTransactionLines()
        {
            _selectedSalesTransactionLines.Clear();

            foreach (var line in _selectedSalesTransaction.TransactionLines.ToList())
                _selectedSalesTransactionLines.Add(new SalesTransactionLineVM { Model = line });
        }

        #region Sales Transaction Properties
        public decimal GrossTotal
        {
            get { return _grossTotal; }
            set { SetProperty(ref _grossTotal, value, "GrossTotal"); }
        }

        public decimal Discount
        {
            get { return _discount; }
            set { SetProperty(ref _discount, value, "Discount"); }
        }

        public decimal SalesExpense
        {
            get { return _salesExpense; }
            set { SetProperty(ref _salesExpense, value, "SalesExpense"); }
        }

        public decimal Total
        {
            get { return _total; }
            set
            {
                SetProperty(ref _total, value, "Total");

                if (_total == 0) return;
                Remaining = _total - _selectedSalesTransaction.Paid - _useCredits;
            }
        }
        #endregion

        #region Payment Properties
        public decimal UseCredits
        {
            get { return _useCredits; }
            set
            {          
                if (value < 0 || value > _salesReturnCredits )
                {
                    MessageBox.Show(string.Format("The available number of credits is {0}", _salesReturnCredits), "Invalid Input", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _useCredits, value, "UseCredits");

                if (_useCredits == 0) return;

                Remaining = _total - _selectedSalesTransaction.Paid - _useCredits;
            }
        }

        public decimal Remaining
        {
            get { return _remaining; }
            set
            {
                SetProperty(ref _remaining, value, "Remaining");
            }
        }

        public decimal? Collect
        {
            get { return _collect; }
            set
            {
                if (value < 0 || value > _remaining)
                {
                    MessageBox.Show(string.Format("The valid range is 0 - {0}", _remaining), "Invalid Input", MessageBoxButton.OK);
                    return;
                }

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

                        Total = 0;
                        Collect = null;
                        SelectedCustomer = null;
                        SelectedPaymentMode = null;
                        SelectedSalesTransaction = null;
                        SalesReturnCredits = 0;
                        UseCredits = 0;
                        Remaining = 0;
                        _selectedSalesTransactionLines.Clear();
                        _customerUnpaidSalesTransactions.Clear();
                    }

                }));
            }
        }

        private void ResetTransaction()
        {
            SalesReturnCredits = 0;
            SelectedCustomer = null;
            SelectedSalesTransaction = null;
            UpdateCustomers();
            _customerUnpaidSalesTransactions.Clear();
            _selectedSalesTransactionLines.Clear();

            GrossTotal = 0;
            Discount = 0;
            SalesExpense = 0;
            Total = 0;
            UseCredits = 0;
            Remaining = 0;
            Collect = null;
        }
    }
}
