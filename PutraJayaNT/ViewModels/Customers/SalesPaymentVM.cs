﻿using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Customers
{
    class SalesPaymentVM : ViewModelBase
    {
        ObservableCollection<CustomerVM> _customers;
        ObservableCollection<string> _paymentModes;
        ObservableCollection<SalesTransaction> _customerUnpaidSalesTransactions;
        ObservableCollection<SalesTransactionLineVM> _selectedSalesTransactionLines;

        CustomerVM _selectedCustomer;
        SalesTransaction _selectedSalesTransaction;
        string _selectedPaymentMode;
        decimal _salesReturnCredits;

        decimal _total;
        decimal _useCredits;
        decimal _remaining;
        decimal? _pay;

        ICommand _confirmPaymentCommand;

        public SalesPaymentVM()
        {
            _customers = new ObservableCollection<CustomerVM>();
            _customerUnpaidSalesTransactions = new ObservableCollection<SalesTransaction>();
            _selectedSalesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _paymentModes = new ObservableCollection<string>();

            UpdatePaymentModes();
            UpdateCustomers();
        }

        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
        }

        public ObservableCollection<string> PaymentModes
        {
            get { return _paymentModes; }
        }

        public ObservableCollection<SalesTransaction> CustomerUnpaidSalesTransactions
        {
            get { return _customerUnpaidSalesTransactions; }
        }

        public ObservableCollection<SalesTransactionLineVM> SelectedSalesTransactionLines
        {
            get { return _selectedSalesTransactionLines; }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                SalesReturnCredits = _selectedCustomer.SalesReturnCredits;
                UpdateCustomerUnpaidSalesTransactions();
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
            _paymentModes.Clear();

            _paymentModes.Add("Cash");

            using (var context = new ERPContext())
            {
                var banks = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank"))
                    .ToList();

                foreach (var bank in banks)
                    _paymentModes.Add(bank.Name);
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
                    .Where(e => e.Customer.ID.Equals(_selectedCustomer.ID) && (e.Paid < e.Total))
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

        #region Payment Properties
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

                Remaining = _total - _selectedSalesTransaction.Paid - UseCredits;
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

        public decimal? Pay
        {
            get { return _pay; }
            set
            {
                if (value < 0 || value > _remaining)
                {
                    MessageBox.Show(string.Format("The valid range is 0 - {0}", _remaining), "Invalid Input", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _pay, value, "Pay");
            }
        }
        #endregion

        public ICommand ConfirmPaymentCommand
        {
            get
            {
                return _confirmPaymentCommand ?? (_confirmPaymentCommand = new RelayCommand(() =>
                {
                    if (_pay == null)
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

                            _selectedSalesTransaction.Paid += (decimal)_pay + _useCredits;
                            _selectedSalesTransaction.Customer.SalesReturnCredits -= _useCredits;

                            var accountsReceivableName = _selectedCustomer.Name + " Accounts Receivable";
                            var transaction = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, _selectedSalesTransaction.SalesTransactionID, "Sales Transaction Payment");
                            context.SaveChanges();

                            LedgerDBHelper.AddTransactionLine(context, transaction, _selectedPaymentMode, "Debit", (decimal)_pay);
                            LedgerDBHelper.AddTransactionLine(context, transaction, accountsReceivableName, "Credit", (decimal)_pay);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        Total = 0;
                        Pay = null;
                        SelectedCustomer = null;
                        SelectedPaymentMode = null;
                        SelectedSalesTransaction = null;
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

            Total = 0;
            UseCredits = 0;
            Remaining = 0;
            Pay = 0;
        }
    }
}