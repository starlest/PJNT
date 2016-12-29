namespace ECERP.ViewModels.Accounting
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Models.Accounting;
    using MVVMFramework;
    using Utilities;

    internal class BalanceSheetVM : ViewModelBase
    {
        int _periodYear;
        int _periodMonth;

        decimal _cashAndCashEquivalents;
        decimal _inventory;
        decimal _accountsReceivable;
        decimal _totalCurrentAssets;
        decimal _totalAssets;

        decimal _accountsPayable;
        decimal _totalCurrentLiabilities;

        decimal _capital;
        decimal _retainedEarnings;
        decimal _totalEquity;
        decimal _totalLiabilitiesAndEquity;

        public BalanceSheetVM()
        {
            Months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            _periodYear = UtilityMethods.GetCurrentDate().Year;
            _periodMonth = UtilityMethods.GetCurrentDate().Month - 1;
        }

        public List<int> Months { get; }

        public int PeriodYear
        {
            get { return _periodYear; }
            set { SetProperty(ref _periodYear, value, () => PeriodYear); }
        }

        public int PeriodMonth
        {
            get { return _periodMonth; }
            set
            {
                SetProperty(ref _periodMonth, value, () => PeriodMonth);
                RefreshBalanceSheet();
            }
        }

        public string AsOfDate => "As of 31/" + _periodMonth + "/" + _periodYear;

        public decimal CashAndCashEquivalents
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _cashAndCashEquivalents = 0;

                    var equivalents = context.Ledger_Accounts
                        .Where(account => account.Name == "Cash" || account.Name.Equals("Bank BNI"))
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .ToList();

                    foreach (var equivalent in equivalents)
                        _cashAndCashEquivalents += FindCurrentBalance(equivalent);

                    return _cashAndCashEquivalents;
                }
            }
        }

        public decimal Inventory
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _inventory = 0;

                    var inventoryFromDatabase = context.Ledger_Accounts
                        .Where(account => account.Name.Equals(Constants.INVENTORY))
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .SingleOrDefault();

                    _inventory = FindCurrentBalance(inventoryFromDatabase);

                    return _inventory;
                }
            }
        }

        public decimal AccountsReceivable
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _accountsReceivable = 0;

                    var accountsReceivable = context.Ledger_Accounts
                        .Where(account => account.LedgerAccountGroup.Name.Equals("Accounts Receivable"))
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .ToList();

                    foreach (var account in accountsReceivable)
                        _accountsReceivable += FindCurrentBalance(account);

                    return _accountsReceivable;
                }
            }
        }

        public decimal TotalCurrentAssets
        {
            get
            {
                _totalCurrentAssets = 0;
                _totalCurrentAssets = _cashAndCashEquivalents + _inventory + _accountsReceivable;

                return _totalCurrentAssets;
            }
        }

        public decimal TotalAssets
        {
            get
            {
                _totalAssets = _totalCurrentAssets;
                return _totalAssets;
            }
        }

        public decimal Capital
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _capital = 0;

                    var capital = context.Ledger_Accounts
                        .Where(e => e.Name == "Capital")
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .FirstOrDefault();

                    _capital = FindCurrentBalance(capital);

                    return _capital;
                }
            }
        }

        public decimal RetainedEarnings
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _retainedEarnings = 0;

                    var retainedEarnings = context.Ledger_Accounts
                        .Where(e => e.Name == "Retained Earnings")
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .FirstOrDefault();

                    _retainedEarnings = FindCurrentBalance(retainedEarnings);

                    return _retainedEarnings;
                }
            }
        }

        public decimal TotalEquity
        {
            get
            {
                _totalEquity = _capital + _retainedEarnings;
                return _totalEquity;
            }
        }

        public decimal TotalLiabilitiesAndEquity
        {
            get
            {
                _totalLiabilitiesAndEquity = _totalCurrentLiabilities + _totalEquity;
                return _totalLiabilitiesAndEquity;
            }
        }

        public decimal AccountsPayable
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _accountsPayable = 0;

                    var accountsPayable = context.Ledger_Accounts
                        .Where(account => account.LedgerAccountGroup.Name.Equals("Accounts Payable"))
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .ToList();

                    foreach (var account in accountsPayable)
                        _accountsPayable += FindCurrentBalance(account);

                    return _accountsPayable;
                }
            }
        }

        public decimal TotalCurrentLiabilities
        {
            get
            {
                _totalCurrentLiabilities = _accountsPayable;
                return _totalCurrentLiabilities;
            }
        }

        private void RefreshBalanceSheet()
        {
            OnPropertyChanged("AsOfDate");
            OnPropertyChanged("CashAndCashEquivalents");
            OnPropertyChanged("Inventory");
            OnPropertyChanged("AccountsReceivable");
            OnPropertyChanged("TotalCurrentAssets");
            OnPropertyChanged("TotalAssets");
            OnPropertyChanged("Capital");
            OnPropertyChanged("RetainedEarnings");
            OnPropertyChanged("TotalEquity");
            OnPropertyChanged("AccountsPayable");
            OnPropertyChanged("TotalCurrentLiabilities");
            OnPropertyChanged("TotalLiabilitiesAndEquity");
        }

        private decimal FindCurrentBalance(LedgerAccount account)
        {
            var period = _periodMonth;
            var periodYearBalances = account.LedgerAccountBalances.Single(e => e.PeriodYear.Equals(_periodYear));

            switch (period)
            {
                case 1:
                    return periodYearBalances.Balance1;
                case 2:
                    return periodYearBalances.Balance2;
                case 3:
                    return periodYearBalances.Balance3;
                case 4:
                    return periodYearBalances.Balance4;
                case 5:
                    return periodYearBalances.Balance5;
                case 6:
                    return periodYearBalances.Balance6;
                case 7:
                    return periodYearBalances.Balance7;
                case 8:
                    return periodYearBalances.Balance8;
                case 9:
                    return periodYearBalances.Balance9;
                case 10:
                    return periodYearBalances.Balance10;
                case 11:
                    return periodYearBalances.Balance11;
                default:
                    return periodYearBalances.Balance12;
            }
        }
    }
}