namespace PutraJayaNT.ViewModels.Accounting
{
    using Ledger;
    using MVVMFramework;
    using Utilities;
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Accounting;

    internal class CashBankTransactionVM : ViewModelBase
    {
        #region Backing Fields

        private DateTime _fromDate;
        private DateTime _toDate;
        private LedgerAccountVM _selectedBank;
        private LedgerTransactionLineVM _selectedLine;
        private ICommand _deleteLineCommand;
        #endregion

        public CashBankTransactionVM()
        {
            NewEntryVM = new CashBankTransactionNewEntryVM(this);
            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;
            Banks = new ObservableCollection<LedgerAccountVM>();
            DisplayLines = new ObservableCollection<LedgerTransactionLineVM>();
            DisplayLines.CollectionChanged += OnCollectionChanged;
            UpdateBanks();
        }

        public CashBankTransactionNewEntryVM NewEntryVM { get; }

        public ObservableCollection<LedgerTransactionLineVM> DisplayLines { get; }

        #region Properties
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
                SetProperty(ref _fromDate, value, () => FromDate);
                if (_fromDate != null && _selectedBank != null) UpdateDisplayedLines();
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
                SetProperty(ref _toDate, value, () => ToDate);
                if (_toDate != null && _selectedBank != null) UpdateDisplayedLines();
            }
        }

        public ObservableCollection<LedgerAccountVM> Banks { get; }

        public LedgerAccountVM SelectedBank
        {
            get { return _selectedBank; }
            set
            {
                SetProperty(ref _selectedBank, value, () => SelectedBank);
                if (_selectedBank == null) return;
                UpdateDisplayedLines();
            }
        }

        public LedgerTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }
        #endregion

        #region Commands
        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected() || !IsLineAllowedToBeDeleted() || !IsLineDateValid() || !IsConfirmationYes()) return;
                    DisplayLines.Remove(_selectedLine);
                }));
            }
        }
        #endregion

        #region Helper Methods
        public void UpdateBanks()
        {
            var oldSelectedBank = _selectedBank;

            using (var context = UtilityMethods.createContext())
            {
                Banks.Clear();
                var banks = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank") &&
                    !e.Name.Contains("Expense") || e.Name.Equals("Cash"))
                    .Include("LedgerTransactionLines");
                foreach (var bank in banks)
                    Banks.Add(new LedgerAccountVM { Model = bank });         
            }

            UpdateSelectedBank(oldSelectedBank);
        }

        private void UpdateSelectedBank(LedgerAccountVM oldSelectedBank)
        {
            if (oldSelectedBank == null) return;
            SelectedBank = Banks.SingleOrDefault(bank => bank.ID.Equals(oldSelectedBank.ID));
        }

        public void UpdateDisplayedLines()
        {
            DisplayLines.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var transactionLines = context.Ledger_Transaction_Lines
                    .Include("LedgerAccount")
                    .Include("LedgerTransaction")
                    .Include("LedgerTransaction.LedgerTransactionLines")
                    .Include("LedgerTransaction.LedgerTransactionLines.LedgerAccount")
                    .Where(line => line.LedgerAccount.ID.Equals(_selectedBank.ID) && _fromDate <= line.LedgerTransaction.Date && _toDate >= line.LedgerTransaction.Date)
                    .OrderBy(line => line.LedgerTransaction.Date)
                    .ToList();
                foreach (var oppositeLine in from line in transactionLines
                                             from oppositeLine in line.LedgerTransaction.LedgerTransactionLines
                                             where oppositeLine.LedgerAccountID != _selectedBank.ID select oppositeLine)
                {
                    oppositeLine.Seq = oppositeLine.Seq == "Debit" ? "Credit" : "Debit";
                    DisplayLines.Add(new LedgerTransactionLineVM { Model = oppositeLine });
                }
            }
        }

        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private bool IsLineAllowedToBeDeleted()
        {
            if (!_selectedLine.Description.Equals("Purchase Payment") &&
                !_selectedLine.Description.Equals("Sales Transaction Receipt")) return true;
            MessageBox.Show("Cannot delete this line.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private bool IsLineDateValid()
        {
            if (_selectedLine.Date.Equals(UtilityMethods.GetCurrentDate())) return true;
            MessageBox.Show("Cannot delete a line from another date.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private static bool IsConfirmationYes()
        {
            return
                MessageBox.Show("Confirm deletion?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes;
        }
        #endregion

        #region Collection Event Handlers
        private static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems == null) return;
            foreach (LedgerTransactionLineVM deletedLine in e.OldItems)
                RemoveLineFromDatabase(deletedLine.Model);
        }

        private static void RemoveLineFromDatabase(LedgerTransactionLine deletedLine)
        {
            using (var context = UtilityMethods.createContext())
            {
                var firstLine = context.Ledger_Transaction_Lines
                    .Include("LedgerAccount")
                    .Single(line => line.LedgerTransactionID.Equals(deletedLine.LedgerTransaction.ID) &&
                                    line.LedgerAccountID.Equals(deletedLine.LedgerAccount.ID));
                var oppostieLine = context.Ledger_Transaction_Lines
                    .Include("LedgerAccount")
                    .Single(line => line.LedgerTransactionID.Equals(deletedLine.LedgerTransaction.ID) &&
                                         !line.LedgerAccountID.Equals(deletedLine.LedgerAccount.ID));
                var transactionFromDatabase = context.Ledger_Transactions
                    .Single(transaction => transaction.ID.Equals(deletedLine.LedgerTransaction.ID));
                var ledgerGeneralFromDatabase = context.Ledger_General
                    .Single(ledgerGeneral => ledgerGeneral.ID.Equals(deletedLine.LedgerAccount.ID));
                var oppositeLedgerGeneralFromDatabase = context.Ledger_General
                    .Single(ledgerGeneral => ledgerGeneral.ID.Equals(oppostieLine.LedgerAccount.ID));

                if (!transactionFromDatabase.Date.Month.Equals(context.Ledger_General.First().Period))
                {
                    MessageBox.Show("This line cannot be deleted as the period has been closed.", "Invalid Command",
                        MessageBoxButton.OK);
                }

                context.Ledger_Transactions.Remove(transactionFromDatabase);
                context.Ledger_Transaction_Lines.Remove(firstLine);
                context.Ledger_Transaction_Lines.Remove(oppostieLine);
                if (deletedLine.Seq.Equals("Credit"))
                {
                    ledgerGeneralFromDatabase.Debit -= deletedLine.Amount;
                    oppositeLedgerGeneralFromDatabase.Credit -= deletedLine.Amount;
                }
                else
                {
                    ledgerGeneralFromDatabase.Credit -= deletedLine.Amount;
                    oppositeLedgerGeneralFromDatabase.Debit -= deletedLine.Amount;
                }
                context.SaveChanges();
            }
        }
        #endregion
    }
}
