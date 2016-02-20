using MVVMFramework;
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

        LedgerTransactionLineVM _selectedLine;
        ICommand _deleteLineCommand;

        public OperatingExpenseVM()
        {
            _accounts = new ObservableCollection<LedgerAccountVM>();
            _displayTransactions = new ObservableCollection<LedgerTransactionLineVM>();
            _displayTransactions.CollectionChanged += OnCollectionChanged;
            _paymentModes = new ObservableCollection<string>();

            _paymentModes.Add("Cash");
            using (var context = new ERPContext())
            {
                var bankAccounts = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank") && e.Notes.Equals("Current Asset"));
                foreach (var bank in bankAccounts)
                    _paymentModes.Add(bank.Name);
            }

            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;

            _newEntryDate = UtilityMethods.GetCurrentDate().Date;
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

        #region New Entry Properties
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

                            if (!LedgerDBHelper.AddTransaction(context, transaction, _newEntryDate, "Operating Expense", _newEntryDescription)) return;
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
        #endregion

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
                       _displayTransactions.Remove(_selectedLine);
                }));
            }
        }

        #region Helper Methods
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

        private void ResetEntryFields()
        {
            UpdateAccounts();
            NewEntryDate = UtilityMethods.GetCurrentDate().Date;
            NewEntryAccount = null;
            NewEntryDescription = null;
            NewEntryAmount = null;
            NewEntryPaymentMode = null;
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
        #endregion

        #region Collection Event Handlers
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (LedgerTransactionLineVM line in e.OldItems)
                {
                    using (var context = new ERPContext())
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
                        if (line.Seq.Equals("Debit"))
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
