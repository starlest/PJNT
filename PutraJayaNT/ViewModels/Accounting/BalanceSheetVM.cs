using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PutraJayaNT.ViewModels.Accounting
{
    class BalanceSheetVM : ViewModelBase
    {
        List<int> _months;

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
            _months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            _periodYear = UtilityMethods.GetCurrentDate().Year;
            _periodMonth = UtilityMethods.GetCurrentDate().Month;
        }

        public List<int> Months
        {
            get { return _months;  }
        }

        public int PeriodYear
        {
            get { return _periodYear; }
            set { SetProperty(ref _periodYear, value, "PeriodYear"); }
        }

        public int PeriodMonth
        {
            get { return _periodMonth; }
            set
            {
                SetProperty(ref _periodMonth, value, "PeriodMonth");
                RefreshBalanceSheet();
            }
        }

        public string AsOfDate
        { 
            get
            {
                return "As of 31/" + _periodMonth + "/" + _periodYear;
            }
        }

        public decimal CashAndCashEquivalents
        {
            get
            {
                using (var context = UtilityMethods.createContext())
                {
                    _cashAndCashEquivalents = 0;

                    var equivalents = context.Ledger_Accounts
                        .Where(e => e.Name == "Cash" || e.Name.Equals("Bank BNI"))
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

                    var Inventory = context.Ledger_Accounts
                        .Where(e => e.Name == "Inventory")
                        .Include("LedgerAccountBalances")
                        .Include("LedgerGeneral")
                        .FirstOrDefault();

                    _inventory = FindCurrentBalance(Inventory);

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
                        .Where(e => e.Name.Contains("Accounts Receivable") || e.Notes.Contains("Accounts Receivable"))
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
                        .Where(e => e.Name.Contains("Accounts Payable") || e.Notes.Contains("Accounts Payable"))
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
                using (var context = UtilityMethods.createContext())
                {
                    _totalCurrentLiabilities = _accountsPayable;

                    return _totalCurrentLiabilities;
                }
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
            var periodYearBalances = account.LedgerAccountBalances.Where(e => e.PeriodYear.Equals(UtilityMethods.GetCurrentDate().Year)).FirstOrDefault();

            if (period == 1)
                return periodYearBalances.Balance1;
            else if (period == 2)
                return periodYearBalances.Balance2;
            else if (period == 3)
                return periodYearBalances.Balance3;
            else if (period == 4)
                return periodYearBalances.Balance4;
            else if (period == 5)
                return periodYearBalances.Balance5;
            else if (period == 6)
                return periodYearBalances.Balance6;
            else if (period == 7)
                return periodYearBalances.Balance7;
            else if (period == 8)
                return periodYearBalances.Balance8;
            else if (period == 9)
                return periodYearBalances.Balance9;
            else if (period == 10)
                return periodYearBalances.Balance10;
            else if (period == 11)
                return periodYearBalances.Balance11;
            else
                return periodYearBalances.Balance12;
        }
    }
}
