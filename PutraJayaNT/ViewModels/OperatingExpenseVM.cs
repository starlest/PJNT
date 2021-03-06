﻿using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    class OperatingExpenseVM : ViewModelBase
    {
        ObservableCollection<LedgerAccountVM> _accounts;
        ObservableCollection<LedgerTransactionLineVM> _displayTransactions;
        ObservableCollection<string> _paymentModes;

        DateTime _fromDate;
        DateTime _toDate;

        decimal _total;

        DateTime _newEntryDate;
        LedgerAccountVM _newEntryAccount;
        string _newEntryDescription;
        string _newEntryPaymentMode;
        decimal? _newEntryAmount;
        ICommand _newEntryConfirmCommand;
        ICommand _newEntryCancelCommand;

        public OperatingExpenseVM()
        {
            _accounts = new ObservableCollection<LedgerAccountVM>();
            _displayTransactions = new ObservableCollection<LedgerTransactionLineVM>();
            _paymentModes = new ObservableCollection<string>();

            _paymentModes.Add("Cash");
            using (var context = new ERPContext())
            {
                var bankAccounts = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank"));
                foreach (var bank in bankAccounts)
                    _paymentModes.Add(bank.Name);
            }

            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;

            _newEntryDate = DateTime.Now.Date;
        }

        public ObservableCollection<LedgerAccountVM> Accounts
        {
            get
            {
                UpdateAccounts();
                return _accounts;
            }
        }

        public ObservableCollection<LedgerTransactionLineVM> DisplayTransactions
        {
            get
            {
                UpdateDisplayTransactions();
                return _displayTransactions;
            }
        }

        public ObservableCollection<string> PaymentModes
        {
            get { return _paymentModes; }
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_toDate < value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _fromDate, value, "FromDate");
                UpdateDisplayTransactions();
            }
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (_fromDate > value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _toDate, value, "ToDate");
                UpdateDisplayTransactions();
            }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, "Total"); }
        }

        // ------------------------------- New Entry Properties ------------------------------- //

        public DateTime NewEntryDate
        {
            get { return _newEntryDate; }
            set { SetProperty(ref _newEntryDate, value, "NewEntryDate"); }
        }

        public LedgerAccountVM NewEntryAccount
        {
            get { return _newEntryAccount; }
            set
            {
                SetProperty(ref _newEntryAccount, value, "NewEntryAccount");
            }
        }

        public string NewEntryDescription
        {
            get { return _newEntryDescription; }
            set { SetProperty(ref _newEntryDescription, value, "NewEntryDescription"); }
        }

        public string NewEntryPaymentMode
        {
            get { return _newEntryPaymentMode; }
            set { SetProperty(ref _newEntryPaymentMode, value, "NewEntryPaymentMode"); }
        }

        public decimal? NewEntryAmount
        {
            get { return _newEntryAmount; }
            set { SetProperty(ref _newEntryAmount, value, "NewEntryAmount"); }
        }

        public ICommand NewEntryConfirmCommand
        {
            get
            {
                return _newEntryConfirmCommand ?? (_newEntryConfirmCommand = new RelayCommand(() =>
                {
                    if (_newEntryAccount == null || _newEntryAmount == null || _newEntryDescription == null || _newEntryPaymentMode == null)
                    {
                        MessageBox.Show("Please enter all fields.", "Missing Fields", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm entry?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            var transaction = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, transaction, _newEntryDate, "Operating Expense", _newEntryDescription);
                            context.SaveChanges();

                            LedgerDBHelper.AddTransactionLine(context, transaction, _newEntryAccount.Name, "Debit", (decimal) _newEntryAmount);
                            LedgerDBHelper.AddTransactionLine(context, transaction, _newEntryPaymentMode, "Credit", (decimal)_newEntryAmount);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        UpdateDisplayTransactions();
                        ResetEntryFields();
                    }
                }));
            }
        }

        public ICommand NewEntryCancelCommand
        {
            get
            {
                return _newEntryCancelCommand ?? (_newEntryCancelCommand = new RelayCommand(() =>
                {
                    ResetEntryFields();
                }));
            }
        }

        private void ResetEntryFields()
        {
            UpdateAccounts();
            NewEntryDate = DateTime.Now.Date;
            NewEntryAccount = null;
            NewEntryDescription = null;
            NewEntryAmount = null;
            NewEntryPaymentMode = null;
        }

        // ------------------------------------------------------------------------------------ //

        private void UpdateAccounts()
        {
            _accounts.Clear();

            using (var context = new ERPContext())
            {
                var operatingExpenseAccounts = context.Ledger_Accounts
                    .Where(e => e.Notes.Contains("Operating Expense") && !e.Name.Equals("Cost of Goods Sold"))
                    .OrderBy(e => e.Name);

                foreach (var account in operatingExpenseAccounts)
                    _accounts.Add(new LedgerAccountVM { Model = account });     
            }
        }

        private void UpdateDisplayTransactions()
        {
            _displayTransactions.Clear();
            Total = 0;

            using (var context = new ERPContext())
            {
                var operatingExpenseTransactions = context.Ledger_Transaction_Lines
                    .Include("LedgerTransaction")
                    .Where(e => e.LedgerAccount.Notes.Contains("Operating Expense") 
                    && !e.LedgerAccount.Name.Equals("Cost of Goods Sold")
                    && e.LedgerTransaction.Date >= _fromDate && e.LedgerTransaction.Date <= _toDate)
                    .OrderBy(e => e.LedgerTransaction.Date)
                    .Include("LedgerAccount");

                foreach (var transaction in operatingExpenseTransactions)
                {
                    _displayTransactions.Add(new LedgerTransactionLineVM { Model = transaction });
                    Total += transaction.Amount;
                }                  
            }
        }
    }
}
