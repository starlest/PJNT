using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
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

        public IncomeStatementVM()
        {
            _months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            _month = 5;
            _year = 2015;
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

                using (var context = new ERPContext())
                {
                    var revenues = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Sales Revenue"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalance")
                        .FirstOrDefault();

                    _revenues += FindCurrentBalance(revenues);
                }

                return _revenues;
            }
        }

        public decimal SalesReturnsAndAllowances
        {
            get
            {
                _salesReturnsAndAllowances = 0;

                using (var context = new ERPContext())
                {
                    var salesReturnsAndAllowancesAccount = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Sales Returns and Allowances"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalance")
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

                using (var context = new ERPContext())
                {
                    var cogsAccount = context.Ledger_Accounts
                        .Where(e => e.Name.Equals("Cost of Goods Sold"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalance")
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

                using (var context = new ERPContext())
                {
                    var operatingExpenseAccounts = context.Ledger_Accounts
                        .Where(e => e.Notes.Contains("Operating Expense"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalance");

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

        private void RefreshIncomeStatement()
        {
            OnPropertyChanged("ForTheDate");
            OnPropertyChanged("Revenues");
            OnPropertyChanged("SalesReturnsAndAllowances");
            OnPropertyChanged("CostOfGoodsSold");
            OnPropertyChanged("GrossMargin");
            OnPropertyChanged("OperatingExpenses");
            OnPropertyChanged("OperatingIncome");
        }

        private decimal FindCurrentBalance(LedgerAccount account)
        {
            var period = _month;

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
