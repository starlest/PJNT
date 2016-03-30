﻿using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Accounting
{
    using Utilities.ModelHelpers;

    class CashBankTransactionVM : ViewModelBase
    {
        DateTime _fromDate;
        DateTime _toDate;

        ObservableCollection<LedgerAccountVM> _banks; // inclusive of Cash Account
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

        LedgerTransactionLineVM _selectedLine;
        ICommand _deleteLineCommand;

        public CashBankTransactionVM()
        {
            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;

            _banks = new ObservableCollection<LedgerAccountVM>();
            _displayLines = new ObservableCollection<LedgerTransactionLineVM>();
            _displayLines.CollectionChanged += OnCollectionChanged;
            _sequences = new ObservableCollection<string> { "Debit", "Credit" };

            _newEntryDate = UtilityMethods.GetCurrentDate().Date;
            _accounts = new ObservableCollection<LedgerAccountVM>();

            UpdateAccounts();
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

                if (_fromDate != null && _toDate != null && _selectedBank != null) UpdateDisplayLines();
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

                if (_fromDate != null && _toDate != null && _selectedBank != null) UpdateDisplayLines();
            }
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
                if (value == null)
                {
                    _selectedBankID = -1;
                    return;
                }

                _selectedBankID = value.ID;
                SetProperty(ref _selectedBank, value, "SelectedBank");

                UpdateAccounts();
                UpdateDisplayLines();
            }
        }

        public LedgerTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (_selectedLine == null)
                    {
                        MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (_selectedLine.Description.Equals("Purchase Payment") || _selectedLine.Description.Equals("Sales Transaction Receipt"))
                    {
                        MessageBox.Show("Cannot delete this line.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (!_selectedLine.Date.Equals(UtilityMethods.GetCurrentDate()))
                    {
                        MessageBox.Show("Cannot delete a line from another date.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm deletion?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        _displayLines.Remove(_selectedLine);                  
                }));
            }
        }

        #region New Entry Propeties
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
                    if (_selectedBank == null)
                    {
                        MessageBox.Show("Please select a bank.", "Invalid Selection", MessageBoxButton.OK);
                        return;
                    }

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
                            var context1 = new ERPContext(UtilityMethods.GetDBName());

                            var transaction = new LedgerTransaction();

                            if (!LedgerTransactionHelper.AddTransactionToDatabase(context1, transaction, _newEntryDate, _newEntryDescription, _newEntryDescription)) return;
                            context1.SaveChanges();

                            LedgerTransactionHelper.AddTransactionLineToDatabase(context1, transaction, _newEntryAccount.Name, _newEntrySequence, (decimal)_newEntryAmount);
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context1, transaction, _selectedBank.Name, _newEntrySequence == "Debit" ? "Credit" : "Debit", (decimal)_newEntryAmount);
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
        #endregion

        #region Helper Methods
        private void UpdateAccounts()
        {
            _accounts.Clear();

            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var accounts = context.Ledger_Accounts
                    .Where(e => !e.Name.Equals("Cost of Goods Sold") && !e.Name.Equals("- Accounts Payable")
                    && !e.Name.Equals("Inventory") && !e.Class.Equals("Equity")
                    && !e.Name.Contains("Revenue"))
                    .OrderBy(e => e.Name)
                    .ToList();

                foreach (var account in accounts)
                {
                    if (_selectedBank != null && account.Name.Equals(_selectedBank.Name)) continue;
                    _accounts.Add(new LedgerAccountVM { Model = account });
                }
            }
        }

        private void ResetEntryFields()
        {
            NewEntryDate = UtilityMethods.GetCurrentDate().Date;
            NewEntryAccount = null;
            NewEntryAmount = null;
            NewEntryDescription = null;
            NewEntrySequence = null;
            UpdateAccounts();
        }

        private void UpdateBanks()
        {
            _banks.Clear();

            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var banks = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank") && !e.Name.Contains("Expense") || e.Name.Equals("Cash"))
                    .Include("LedgerTransactionLines");

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

            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var transactionLines = context.Ledger_Transaction_Lines
                    .Include("LedgerAccount")
                    .Include("LedgerTransaction")
                    .Include("LedgerTransaction.LedgerTransactionLines")
                            .Include("LedgerTransaction.LedgerTransactionLines.LedgerAccount")
                    .Where(e => e.LedgerAccount.ID == _selectedBankID && _fromDate <= e.LedgerTransaction.Date && _toDate >= e.LedgerTransaction.Date)
                    .OrderBy(e => e.LedgerTransaction.Date);

                foreach (var line in transactionLines)
                {
                    // Find the opposing line(s) of the line
                    foreach (var l in line.LedgerTransaction.LedgerTransactionLines)
                    {
                        if (l.LedgerAccountID != _selectedBankID)
                        {
                            l.Seq = l.Seq == "Debit" ? "Credit" : "Debit";
                            _displayLines.Add(new LedgerTransactionLineVM { Model = l });
                        }
                    }
                }
            }
        }
        #endregion

        #region Collection Event Handlers
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (LedgerTransactionLineVM line in e.OldItems)
                {
                    using (var context = new ERPContext(UtilityMethods.GetDBName()))
                    {
                        var firstLine = context.Ledger_Transaction_Lines
                            .Include("LedgerAccount")
                            .Where(l => l.LedgerTransactionID.Equals(line.LedgerTransaction.ID) &&
                            l.LedgerAccountID.Equals(line.LedgerAccount.ID))
                            .FirstOrDefault();
                        var oppostieLine = context.Ledger_Transaction_Lines
                            .Include("LedgerAccount")
                            .Where(l => l.LedgerTransactionID.Equals(line.LedgerTransaction.ID) &&
                            !l.LedgerAccountID.Equals(line.LedgerAccount.ID))
                            .FirstOrDefault();
                        var transaction = context.Ledger_Transactions
                            .Where(t => t.ID.Equals(line.LedgerTransaction.ID))
                            .FirstOrDefault();
                        var ledgerGeneral = context.Ledger_General
                            .Where(g => g.ID.Equals(line.LedgerAccount.ID))
                            .FirstOrDefault();
                        var oppositeLedgerGeneral = context.Ledger_General
                            .Where(g => g.ID.Equals(oppostieLine.LedgerAccount.ID))
                            .FirstOrDefault();
                        context.Ledger_Transactions.Remove(transaction);
                        context.Ledger_Transaction_Lines.Remove(firstLine);
                        context.Ledger_Transaction_Lines.Remove(oppostieLine);
                        if (line.Seq.Equals("Credit"))
                        {
                            ledgerGeneral.Debit -= line.Amount;
                            oppositeLedgerGeneral.Credit -= line.Amount;
                        }
                        else
                        {
                            ledgerGeneral.Credit -= line.Amount;
                            oppositeLedgerGeneral.Debit -= line.Amount;
                        }

                        context.SaveChanges();
                    }
                }
            }
        }
        #endregion
    }
}
