using MVVMFramework;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace PutraJayaNT.ViewModels.Accounting
{
    class GeneralLedgerVM : ViewModelBase
    {
        ObservableCollection<string> _accounts;
        ObservableCollection<LedgerTransactionLineVM> _displayTransactions;

        ObservableCollection<int> _months;
        int _selectedMonth;
        int _selectedYear;

        string _selectedClass;
        decimal? _selectedBeginningBalance;
        string _selectedAccount;
        LedgerAccountVM _selectedAccountVM;

        decimal _totalDebit;
        decimal _totalCredit;
        decimal _endingBalance;

        string _normalSeq;

        public GeneralLedgerVM()
        {
            _accounts = new ObservableCollection<string>();
            _displayTransactions = new ObservableCollection<LedgerTransactionLineVM>();

            _months = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            _selectedMonth = DateTime.Now.Month;              
            _selectedYear = DateTime.Now.Year;

            RefreshAccounts();
        }

        public ObservableCollection<string> Accounts
        {
            get { return _accounts; }
        }

        public ObservableCollection<LedgerTransactionLineVM> DisplayTransactions
        {
            get { return _displayTransactions; }
        }

        public ObservableCollection<int> Months
        {
            get { return _months; }
        }

        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                SetProperty(ref _selectedMonth, value, "SelectedMonth");
                if (_selectedAccount != null) RefreshDisplayLines();
            }
        }

        public int SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value, "SelectedYear"); }
        }

        public string SelectedClass
        {
            get { return _selectedClass; }
            set { SetProperty(ref _selectedClass, value, "SelectedClass"); }
        }

        public decimal? SelectedBeginningBalance
        {
            get { return _selectedBeginningBalance; }
            set
            {
                SetProperty(ref _selectedBeginningBalance, value, "SelectedBeginningBalance");
            }
        }

        public decimal TotalDebit
        {
            get { return _totalDebit; }
            set { SetProperty(ref _totalDebit, value, "TotalDebit"); }
        }

        public decimal TotalCredit
        {
            get { return _totalCredit; }
            set { SetProperty(ref _totalCredit, value, "TotalCredit"); }
        }

        public decimal EndingBalance
        {
            get { return _endingBalance; }
            set { SetProperty(ref _endingBalance, value, "EndingBalance"); }
        }

        public string SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                TotalDebit = 0;
                TotalCredit = 0;
                _displayTransactions.Clear();
                SetProperty(ref _selectedAccount, value, "SelectedAccount");

                if (_selectedAccount == null) return;

                RefreshAccounts();
                RefreshDisplayLines();
            }
        }

        public void RefreshAccounts()
        {
            _accounts.Clear();

            using (var context = new ERPContext())
            {
                var accounts = context.Ledger_Accounts
                    .Where(e => !e.Name.Equals("- Accounts Payable"))
                    .OrderBy(e => e.Class)
                    .ThenBy(e => e.Name);

                foreach (var account in accounts)
                    _accounts.Add(account.Name);
            }
        }

        public void RefreshDisplayLines()
        {
            _displayTransactions.Clear();

            using (var context = new ERPContext())
            {
                var selectedAccount = context.Ledger_Accounts
                    .Where(e => e.Name == _selectedAccount)
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalances")
                    .FirstOrDefault();

                _selectedAccountVM = new LedgerAccountVM { Model = selectedAccount };

                SelectedClass = _selectedAccountVM.Class;

                if (_selectedClass == "Asset" || _selectedClass == "Expense") _normalSeq = "Debit";
                else _normalSeq = "Credit";

                var periodYearBalances = selectedAccount.LedgerAccountBalances.Where(e => e.PeriodYear.Equals(_selectedYear)).FirstOrDefault();

                // Determine the beginning balance of the account
                switch (_selectedMonth)
                {
                    case 1:
                        SelectedBeginningBalance = periodYearBalances.BeginningBalance;
                        break;
                    case 2:
                        SelectedBeginningBalance = periodYearBalances.Balance1;
                        break;
                    case 3:
                        SelectedBeginningBalance = periodYearBalances.Balance2;
                        break;
                    case 4:
                        SelectedBeginningBalance = periodYearBalances.Balance3;
                        break;
                    case 5:
                        SelectedBeginningBalance = periodYearBalances.Balance4;
                        break;
                    case 6:
                        SelectedBeginningBalance = periodYearBalances.Balance5;
                        break;
                    case 7:
                        SelectedBeginningBalance = periodYearBalances.Balance6;
                        break;
                    case 8:
                        SelectedBeginningBalance = periodYearBalances.Balance7;
                        break;
                    case 9:
                        SelectedBeginningBalance = periodYearBalances.Balance8;
                        break;
                    case 10:
                        SelectedBeginningBalance = periodYearBalances.Balance9;
                        break;
                    case 11:
                        SelectedBeginningBalance = periodYearBalances.Balance10;
                        break;
                    case 12:
                        SelectedBeginningBalance = periodYearBalances.Balance11;
                        break;
                    default:
                        break;
                }

                decimal balanceTracker = (decimal)_selectedBeginningBalance;

                var transactionLines = context.Ledger_Transaction_Lines
                    .Where(e => e.LedgerAccountID == _selectedAccountVM.ID && e.LedgerTransaction.Date.Month == _selectedMonth)
                    .Include("LedgerTransaction")
                    .Include("LedgerAccount")
                    .OrderBy(e => e.LedgerTransaction.Date);

                foreach (var line in transactionLines)
                {
                    var lineVM = new LedgerTransactionLineVM { Model = line };

                    foreach (var l in lineVM.OpposingLines)
                    {
                        l.Seq = l.Seq == "Debit" ? "Credit" : "Debit";

                        if (_normalSeq == l.Seq) balanceTracker += l.Amount;
                        else balanceTracker -= l.Amount;

                        if (l.Seq == "Debit") _totalDebit += l.Amount;
                        else _totalCredit += l.Amount;

                        l.Balance = balanceTracker;
                        _displayTransactions.Add(l);
                    }
                }
                EndingBalance = balanceTracker;

                OnPropertyChanged("TotalDebit");
                OnPropertyChanged("TotalCredit");
            }
        }
    }
}
