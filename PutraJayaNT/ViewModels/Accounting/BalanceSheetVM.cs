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

            _periodYear = DateTime.Now.Year;
            _periodMonth = DateTime.Now.Month - 1;
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
                using (var context = new ERPContext())
                {
                    _cashAndCashEquivalents = 0;

                    var equivalents = context.Ledger_Accounts
                        .Where(e => e.Name == "Cash" || e.Name.Contains("Bank"))
                        .Include("LedgerAccountBalance")
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
                using (var context = new ERPContext())
                {
                    _inventory = 0;

                    var Inventory = context.Ledger_Accounts
                        .Where(e => e.Name == "Inventory")
                        .Include("LedgerAccountBalance")
                        .Include("LedgerGeneral")
                        .FirstOrDefault();

                    _inventory = FindCurrentBalance(Inventory);

                    return _inventory;
                }
            }
        }

        public decimal TotalCurrentAssets
        {
            get
            {
                _totalCurrentAssets = 0;
                _totalCurrentAssets = _cashAndCashEquivalents + _inventory;

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
                using (var context = new ERPContext())
                {
                    _capital = 0;

                    var capital = context.Ledger_Accounts
                        .Where(e => e.Name == "Capital")
                        .Include("LedgerAccountBalance")
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
                using (var context = new ERPContext())
                {
                    _retainedEarnings = 0;

                    var retainedEarnings = context.Ledger_Accounts
                        .Where(e => e.Name == "Retained Earnings")
                        .Include("LedgerAccountBalance")
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
                using (var context = new ERPContext())
                {
                    _accountsPayable = 0;

                    var accountsPayable = context.Ledger_Accounts
                        .Where(e => e.Name.Contains("Accounts Payable"))
                        .Include("LedgerAccountBalance")
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
                using (var context = new ERPContext())
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

            if (period == 1)
                return account.LedgerAccountBalance.Balance1;
            else if (period == 2)
                return account.LedgerAccountBalance.Balance2;
            else if (period == 3)
                return account.LedgerAccountBalance.Balance3;
            else if (period == 4)
                return account.LedgerAccountBalance.Balance4;
            else if (period == 5)
                return account.LedgerAccountBalance.Balance5;
            else if (period == 6)
                return account.LedgerAccountBalance.Balance6;
            else if (period == 7)
                return account.LedgerAccountBalance.Balance7;
            else if (period == 8)
                return account.LedgerAccountBalance.Balance8;
            else if (period == 9)
                return account.LedgerAccountBalance.Balance9;
            else if (period == 10)
                return account.LedgerAccountBalance.Balance10;
            else if (period == 11)
                return account.LedgerAccountBalance.Balance11;
            else
                return account.LedgerAccountBalance.Balance12;
        }
    }
}
