using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PutraJayaNT.ViewModels.Accounting
{
    class IncomeStatementVM : ViewModelBase
    {
        List<int> _months;

        int _month;
        int _year;

        decimal _revenues;
        decimal _salesReturnsAndAllowances;
        decimal _costOfGoodsSold;
        decimal _grossMargin;
        decimal _operatingExpenses;
        decimal _operatingIncome;
        private decimal _netIncome;
        private decimal _otherIncome;

        public IncomeStatementVM()
        {
            _months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            _month = UtilityMethods.GetCurrentDate().Month;
            _year = UtilityMethods.GetCurrentDate().Year;
        }

        public string ForTheDate
        {
            get { return "For the Period Ended 31/" + _month + "/" + _year; }
        }

        public List<int> Months
        {
            get { return _months; }
        }

        public int Month
        {
            get { return _month; }
            set
            {
                SetProperty(ref _month, value, "Month");
                RefreshIncomeStatement();
            }
        }

        public decimal Revenues
        {
            get
            {
                _revenues = 0;

                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var revenues = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Sales Revenue"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .FirstOrDefault();

                    _revenues += FindCurrentBalance(revenues);
                }

                return _revenues;
            }
        }

        public decimal OtherIncome
        {
            get
            {
                _otherIncome = 0;

                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var otherIncome = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Other Income"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .FirstOrDefault();

                    _otherIncome += FindCurrentBalance(otherIncome);
                }

                return _otherIncome;
            }
        }

        public decimal SalesReturnsAndAllowances
        {
            get
            {
                _salesReturnsAndAllowances = 0;

                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var salesReturnsAndAllowancesAccount = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Sales Returns and Allowances"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .FirstOrDefault();

                    _salesReturnsAndAllowances += FindCurrentBalance(salesReturnsAndAllowancesAccount);
                }

                return _salesReturnsAndAllowances;
            }
        }

        public decimal CostOfGoodsSold
        {
            get
            {
                _costOfGoodsSold = 0;

                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var cogsAccount = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Cost of Goods Sold"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .FirstOrDefault();

                    _costOfGoodsSold += FindCurrentBalance(cogsAccount);
                }

                return _costOfGoodsSold;
            }
        }

        public decimal GrossMargin
        {
            get
            {
                _grossMargin = _revenues - _costOfGoodsSold - _salesReturnsAndAllowances;
                return _grossMargin;
            }
        }

        public decimal OperatingExpenses
        {
            get
            {
                _operatingExpenses = 0;

                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var operatingExpenseAccounts = context.Ledger_Accounts
                        .Where(e => e.Notes.Contains("Operating Expense"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances");

                    foreach (var account in operatingExpenseAccounts)
                        _operatingExpenses += FindCurrentBalance(account);
                }

                return _operatingExpenses;
            }
        }

        public decimal OperatingIncome
        {
            get
            {
                _operatingIncome = _grossMargin - _operatingExpenses;
                return _operatingIncome;
            }
        }

        public decimal NetIncome
        {
            get
            {
                _netIncome = _operatingIncome + _otherIncome;
                return _netIncome;
            }
        }

        private void RefreshIncomeStatement()
        {
            OnPropertyChanged("ForTheDate");
            OnPropertyChanged("Revenues");
            OnPropertyChanged("SalesReturnsAndAllowances");
            OnPropertyChanged("CostOfGoodsSold");
            OnPropertyChanged("GrossMargin");
            OnPropertyChanged("OperatingExpenses");
            OnPropertyChanged("OperatingIncome");
            OnPropertyChanged("OtherIncome");
            OnPropertyChanged("NetIncome");
        }

        private decimal FindCurrentBalance(LedgerAccount account)
        {
            var period = _month;
            var periodYearBalances = account.LedgerAccountBalances.Where(e => e.PeriodYear.Equals(_year)).FirstOrDefault();

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
