namespace ECRP.ViewModels.Accounting
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Input;
    using Ledger;
    using Models.Accounting;
    using MVVMFramework;
    using Services;
    using Utilities;
    using Utilities.ModelHelpers;

    internal class CashBankTransactionNewEntryVM : ViewModelBase
    {
        private readonly CashBankTransactionVM _parentVM;

        #region Backing Fields

        private DateTime _newEntryDate;
        private LedgerAccountVM _newEntryAccount;
        private decimal _newEntryAmount;
        private string _newEntryDescription;
        private string _newEntrySequence;
        private ICommand _newEntryConfirmCommand;
        private ICommand _newEntryCancelCommand;

        #endregion

        public CashBankTransactionNewEntryVM(CashBankTransactionVM parentVM)
        {
            _parentVM = parentVM;
            _newEntryDate = UtilityMethods.GetCurrentDate().Date;
            Accounts = new ObservableCollection<LedgerAccountVM>();
            Sequences = new ObservableCollection<string> { "Debit", "Credit" };
            UpdateAccounts();
        }

        #region Collections

        public ObservableCollection<LedgerAccountVM> Accounts { get; }

        public ObservableCollection<string> Sequences { get; }

        #endregion

        #region New Entry Propeties

        public DateTime NewEntryDate
        {
            get { return _newEntryDate; }
            set { SetProperty(ref _newEntryDate, value, () => NewEntryDate); }
        }

        public LedgerAccountVM NewEntryAccount
        {
            get { return _newEntryAccount; }
            set { SetProperty(ref _newEntryAccount, value, () => NewEntryAccount); }
        }

        public decimal NewEntryAmount
        {
            get { return _newEntryAmount; }
            set { SetProperty(ref _newEntryAmount, value, () => NewEntryAmount); }
        }

        public string NewEntryDescription
        {
            get { return _newEntryDescription; }
            set { SetProperty(ref _newEntryDescription, value, () => NewEntryDescription); }
        }

        public string NewEntrySequence
        {
            get { return _newEntrySequence; }
            set { SetProperty(ref _newEntrySequence, value, () => NewEntrySequence); }
        }

        #endregion

        #region Commands

        public ICommand NewEntryConfirmCommand
        {
            get
            {
                return _newEntryConfirmCommand ?? (_newEntryConfirmCommand = new RelayCommand(() =>
                {
                    if (!IsBankSelected() || !AreAllFieldsFilled() || !IsConfirmationYes()) return;
                    AddEntryToDatabase();
                    ResetEntryFields();
                    _parentVM.UpdateDisplayedLines();
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
                    UpdateAccounts();
                    _parentVM.UpdateBanks();
                }));
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateAccounts()
        {
            var protectedAccounts = AccountingService.GetProtectedAccounts();

            using (var context = UtilityMethods.createContext())
            {
                Accounts.Clear();
                var accounts = context.Ledger_Accounts
                    .Where(
                        account =>
                            !protectedAccounts.Contains(account.Name) &&
                            !account.LedgerAccountClass.Name.Equals(Constants.EQUITY)
                            && !account.Name.Contains(Constants.REVENUE) &&
                            !account.Name.Contains(Constants.ACCOUNTS_RECEIVABLE) &&
                            !account.Name.Contains(Constants.ACCOUNTS_PAYABLE))
                    .OrderBy(account => account.Name)
                    .ToList();
                foreach (var account in accounts.Where(
                    account => _parentVM.SelectedBank == null || !account.Name.Equals(_parentVM.SelectedBank.Name)))
                    Accounts.Add(new LedgerAccountVM { Model = account });
            }
        }

        private bool IsBankSelected()
        {
            if (_parentVM.SelectedBank != null) return true;
            MessageBox.Show("Please select a bank.", "Missing Selection", MessageBoxButton.OK);
            return false;
        }

        private bool AreAllFieldsFilled()
        {
            if (_newEntryAccount != null && _newEntryAmount > 0 && _newEntryDescription != null &&
                _newEntrySequence != null) return true;
            MessageBox.Show("Please fill in all fields!", "Missing Field(s)", MessageBoxButton.OK);
            return false;
        }

        private static bool IsConfirmationYes()
        {
            return MessageBox.Show("Confirm adding this entry?", "Confirmation", MessageBoxButton.YesNo) ==
                   MessageBoxResult.Yes;
        }

        private void AddEntryToDatabase()
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required))
            {
                var context = UtilityMethods.createContext();
                var transaction = new LedgerTransaction();
                if (
                    !LedgerTransactionHelper.AddTransactionToDatabase(context, transaction, _newEntryDate,
                        _newEntryDescription, _newEntryDescription)) return;
                context.SaveChanges();
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, _newEntryAccount.Name,
                    _newEntrySequence, _newEntryAmount);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, _parentVM.SelectedBank.Name,
                    _newEntrySequence == "Debit" ? "Credit" : "Debit", _newEntryAmount);
                context.SaveChanges();
                ts.Complete();
            }
        }

        private void ResetEntryFields()
        {
            NewEntryDate = UtilityMethods.GetCurrentDate().Date;
            NewEntryAccount = null;
            NewEntryAmount = 0;
            NewEntryDescription = null;
            NewEntrySequence = null;
            UpdateAccounts();
        }

        #endregion
    }
}