using MVVMFramework;
using PutraJayaNT.Utilities;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace PutraJayaNT.ViewModels.Accounting
{
    using System.Windows.Input;

    internal class GeneralLedgerVM : ViewModelBase
    {
        #region Backing Fields
        private int _selectedMonth;
        private int _selectedYear;

        private string _selectedClass;
        private LedgerAccountVM _selectedAccount;
        private string _normalSeq;
        private decimal _selectedBeginningBalance;
        private decimal _selectedEndingBalance;
        private decimal _totalDebit;
        private decimal _totalCredit;

        private ICommand _displayCommand;

        #endregion

        public GeneralLedgerVM()
        {
            Accounts = new ObservableCollection<LedgerAccountVM>();
            DisplayedTransactionLines = new ObservableCollection<LedgerTransactionLineVM>();
            Months = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            _selectedMonth = UtilityMethods.GetCurrentDate().Month;              
            _selectedYear = UtilityMethods.GetCurrentDate().Year;

            UpdateAccounts();
        }

        #region Collections
        public ObservableCollection<LedgerAccountVM> Accounts { get; }

        public ObservableCollection<LedgerTransactionLineVM> DisplayedTransactionLines { get; }

        public ObservableCollection<int> Months { get; }
        #endregion

        #region Properties
        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set {  SetProperty(ref _selectedMonth, value, () => SelectedMonth); }
        }

        public int SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value, () => SelectedYear); }
        }

        public LedgerAccountVM SelectedAccount
        {
            get { return _selectedAccount; }
            set  { SetProperty(ref _selectedAccount, value, () => SelectedAccount); }
        }

        public string SelectedClass
        {
            get { return _selectedClass; }
            set { SetProperty(ref _selectedClass, value, () => SelectedClass); }
        }

        public decimal SelectedBeginningBalance
        {
            get { return _selectedBeginningBalance; }
            set {  SetProperty(ref _selectedBeginningBalance, value, () => SelectedBeginningBalance); }
        }

        public decimal SelectedEndingBalance
        {
            get { return _selectedEndingBalance; }
            set { SetProperty(ref _selectedEndingBalance, value, () => SelectedEndingBalance); }
        }

        public decimal TotalDebit
        {
            get { return _totalDebit; }
            set { SetProperty(ref _totalDebit, value, () => TotalDebit); }
        }

        public decimal TotalCredit
        {
            get { return _totalCredit; }
            set { SetProperty(ref _totalCredit, value, () => TotalCredit); }
        }
        #endregion

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    ResetUI();
                    if (_selectedAccount == null) return;
                    RefreshDisplayLines();
                    UpdateUI();
                    UpdateAccounts();
                }));
            }
        }

        #region Helper Methods
        public void UpdateAccounts()
        {
            var oldSelectedAccount = _selectedAccount;

            Accounts.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var accounts = context.Ledger_Accounts
                    .Where(e => !e.Name.Equals("- Accounts Payable"))
                    .OrderBy(e => e.Name);
                foreach (var account in accounts)
                    Accounts.Add(new LedgerAccountVM { Model = account });
            }

            UpdateSelectedAccount(oldSelectedAccount);
        }

        private void UpdateSelectedAccount(LedgerAccountVM oldSelectedAccount)
        {
            if (oldSelectedAccount == null) return;
            SelectedAccount = Accounts.FirstOrDefault(account => account.ID.Equals(oldSelectedAccount.ID));
        }

        public void RefreshDisplayLines()
        {
            DisplayedTransactionLines.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                SetBeginningBalanceFromDatabaseContext(context);
                var balanceTracker = _selectedBeginningBalance;

                var transactionLines = context.Ledger_Transaction_Lines
                    .Where(e => e.LedgerAccountID == _selectedAccount.ID && e.LedgerTransaction.Date.Month == _selectedMonth)
                    .Include("LedgerTransaction")
                    .Include("LedgerAccount")
                    .OrderBy(e => e.LedgerTransaction.Date);

                if (_selectedClass == "Asset" || _selectedClass == "Expense") _normalSeq = "Debit";
                else _normalSeq = "Credit";

                foreach (var opposingLine in transactionLines.ToList().Select(
                    line => new LedgerTransactionLineVM { Model = line }).SelectMany(lineVM => lineVM.OpposingLines))
                {
                    opposingLine.Seq = opposingLine.Seq == "Debit" ? "Credit" : "Debit";

                    if (_normalSeq == opposingLine.Seq) balanceTracker += opposingLine.Amount;
                    else balanceTracker -= opposingLine.Amount;

                    if (opposingLine.Seq == "Debit") _totalDebit += opposingLine.Amount;
                    else _totalCredit += opposingLine.Amount;

                    opposingLine.Balance = balanceTracker;
                    DisplayedTransactionLines.Add(opposingLine);
                }
                _selectedEndingBalance = balanceTracker;
            }
        }

        private void SetBeginningBalanceFromDatabaseContext(ERPContext context)
        {
            var periodYearBalances =
                context.Ledger_Account_Balances.Single(
                    balance => balance.ID.Equals(_selectedAccount.ID) && balance.PeriodYear.Equals(_selectedYear));

            switch (_selectedMonth)
            {
                case 1:
                    _selectedBeginningBalance = periodYearBalances.BeginningBalance;
                    break;
                case 2:
                    _selectedBeginningBalance = periodYearBalances.Balance1;
                    break;
                case 3:
                    _selectedBeginningBalance = periodYearBalances.Balance2;
                    break;
                case 4:
                    _selectedBeginningBalance = periodYearBalances.Balance3;
                    break;
                case 5:
                    _selectedBeginningBalance = periodYearBalances.Balance4;
                    break;
                case 6:
                    _selectedBeginningBalance = periodYearBalances.Balance5;
                    break;
                case 7:
                    _selectedBeginningBalance = periodYearBalances.Balance6;
                    break;
                case 8:
                    _selectedBeginningBalance = periodYearBalances.Balance7;
                    break;
                case 9:
                    _selectedBeginningBalance = periodYearBalances.Balance8;
                    break;
                case 10:
                    _selectedBeginningBalance = periodYearBalances.Balance9;
                    break;
                case 11:
                    _selectedBeginningBalance = periodYearBalances.Balance10;
                    break;
                case 12:
                    _selectedBeginningBalance = periodYearBalances.Balance11;
                    break;
            }
        }

        private void UpdateUI()
        {
            SelectedClass = _selectedAccount.Class;
            OnPropertyChanged("SelectedBeginningBalance");
            OnPropertyChanged("SelectedEndingBalance");
            OnPropertyChanged("TotalDebit");
            OnPropertyChanged("TotalCredit");
        }

        private void ResetUI()
        {
            TotalDebit = 0;
            TotalCredit = 0;
            DisplayedTransactionLines.Clear();
        }
        #endregion
    }
}
