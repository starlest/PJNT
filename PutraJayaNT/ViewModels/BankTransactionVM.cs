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
    class BankTransactionVM : ViewModelBase
    {
        DateTime _fromDate;
        DateTime _toDate;

        ObservableCollection<LedgerAccountVM> _banks;
        ObservableCollection<LedgerTransactionLineVM> _displayLines;

        LedgerAccountVM _selectedBank;
        int _selectedBankID;

        DateTime _newEntryDate;
        ObservableCollection<LedgerAccountVM> _accounts;
        LedgerAccountVM _newEntryAccount;
        decimal? _newEntryAmount;
        string _newEntryDescription;
        ObservableCollection<string> _sequences;
        string _newEntrySequence;
        ICommand _newEntryConfirmCommand;
        ICommand _newEntryCancelCommand;

        public BankTransactionVM()
        {
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;

            _banks = new ObservableCollection<LedgerAccountVM>();
            _displayLines = new ObservableCollection<LedgerTransactionLineVM>();
            _sequences = new ObservableCollection<string> { "Debit", "Credit" };

            _newEntryDate = DateTime.Now.Date;
            _accounts = new ObservableCollection<LedgerAccountVM>();
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set { SetProperty(ref _fromDate, value, "FromDate"); }
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set { SetProperty(ref _toDate, value, "ToDate"); }
        }

        public ObservableCollection<LedgerAccountVM> Banks
        {
            get
            {
                UpdateBanks();
                return _banks;
            }
        }

        public ObservableCollection<LedgerTransactionLineVM> DisplayLines
        {
            get
            {
                return _displayLines;
            }
        }

        public LedgerAccountVM SelectedBank
        {
            get { return _selectedBank; }
            set
            {
                SetProperty(ref _selectedBank, value, "SelectedBank");

                if (_selectedBank == null)
                {
                    _selectedBankID = -1;
                    return;
                }

                _selectedBankID = _selectedBank.ID;
                UpdateBanks();
                UpdateDisplayLines();
                UpdateAccounts();
            }
        }

        // ------------------------ New Entry Properties ------------------------ //

        public DateTime NewEntryDate
        {
            get { return _newEntryDate; }
            set { SetProperty(ref _newEntryDate, value, "NewEntryDate"); }
        }

        public ObservableCollection<LedgerAccountVM> Accounts
        {
            get
            {
                return _accounts;
            }
        }

        public LedgerAccountVM NewEntryAccount
        {
            get { return _newEntryAccount; }
            set { SetProperty(ref _newEntryAccount, value, "NewEntryAccount"); }
        }

        public decimal? NewEntryAmount
        {
            get { return _newEntryAmount; }
            set { SetProperty(ref _newEntryAmount, value, "NewEntryAmount"); }
        }

        public string NewEntryDescription
        {
            get { return _newEntryDescription; }
            set { SetProperty(ref _newEntryDescription, value, "NewEntryDescription"); }
        }

        public ObservableCollection<string> Sequences
        {
            get { return _sequences; }
        }

        public string NewEntrySequence
        {
            get { return _newEntrySequence; }
            set { SetProperty(ref _newEntrySequence, value, "NewEntrySequence"); }
        }

        public ICommand NewEntryConfirmCommand
        {
            get
            {
                return _newEntryConfirmCommand ?? (_newEntryConfirmCommand = new RelayCommand(() =>
                {
                    if (_newEntryAccount == null || _newEntryAmount == null ||
                    _newEntryDescription == null || _newEntrySequence == null)
                    {
                        MessageBox.Show("Please fill in all fields!", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm adding this entry?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required))
                        {
                            var context1 = new ERPContext();

                            var transaction = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context1, transaction, _newEntryDate, _newEntryDescription, _newEntryDescription);
                            context1.SaveChanges();

                            LedgerDBHelper.AddTransactionLine(context1, transaction, _newEntryAccount.Name, _newEntrySequence, (decimal)_newEntryAmount);
                            LedgerDBHelper.AddTransactionLine(context1, transaction, _selectedBank.Name, _newEntrySequence == "Debit" ? "Credit" : "Debit", (decimal)_newEntryAmount);
                            context1.SaveChanges();

                            ts.Complete();
                        }

                        ResetEntryFields();
                        UpdateDisplayLines();
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

        private void UpdateAccounts()
        {
            _accounts.Clear();

            using (var context = new ERPContext())
            {
                var accounts = context.Ledger_Accounts
                    .Where(e => !e.Name.Equals(_selectedBank.Name) &&
                    !e.Name.Contains("Payable") && !e.Name.Equals("Cost of Goods Sold")
                    && !e.Name.Equals("Inventory") && !e.Name.Equals("Retained Earnings")
                    && !e.Notes.Equals("Operating Expense") && !e.Name.Contains("Revenue"));
      
                foreach (var account in accounts)
                    _accounts.Add(new LedgerAccountVM { Model = account });
            }
        }

        private void ResetEntryFields()
        {
            NewEntryDate = DateTime.Now.Date;
            NewEntryAccount = null;
            NewEntryAmount = null;
            NewEntryDescription = null;
            NewEntrySequence = null;
            UpdateAccounts();
        }

        // ---------------------------------------------------------------------- //

        private void UpdateBanks()
        {
            _banks.Clear();

            using (var context = new ERPContext())
            {
                var banks = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank"))
                    .Include("TransactionLines");

                foreach (var bank in banks)
                {
                    var bankVM = new LedgerAccountVM { Model = bank };
                    _banks.Add(bankVM);
                }
            }
        }

        private void UpdateDisplayLines()
        {
            _displayLines.Clear();

            using (var context = new ERPContext())
            {
                var transactionLines = context.Ledger_Transaction_Lines
                    .Include("LedgerTransaction")
                    .Include("LedgerTransaction.LedgerTransactionLines")
                    .Where(e => e.LedgerAccount.ID == _selectedBankID && _fromDate <= e.LedgerTransaction.Date && _toDate >= e.LedgerTransaction.Date)
                    .OrderBy(e => e.LedgerTransactionID);

                foreach (var line in transactionLines)
                {
                    // Find the opposing line(s) of the line
                    foreach (var l in line.LedgerTransaction.LedgerTransactionLines)
                    {
                        if (l.LedgerAccountID != _selectedBankID)
                        {
                            var transactionLine = context.Ledger_Transaction_Lines
                                .Include("LedgerAccount")
                                .Include("LedgerTransaction")
                                .Where(e => e.LedgerTransactionID == l.LedgerTransactionID)
                                .FirstOrDefault();

                            _displayLines.Add(new LedgerTransactionLineVM { Model = transactionLine });
                        }
                    }
                }
            }
        }
    }
}
