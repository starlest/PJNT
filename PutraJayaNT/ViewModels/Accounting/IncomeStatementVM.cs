namespace ECERP.ViewModels.Accounting
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows.Forms;
    using Models.Accounting;
    using MVVMFramework;
    using Utilities;

    internal class IncomeStatementVM : ViewModelBase
    {
        private int _month;
        private int _year;

        private decimal _revenues;
        private decimal _salesReturnsAndAllowances;
        private decimal _costOfGoodsSold;
        private decimal _grossMargin;
        private decimal _operatingExpenses;
        private decimal _operatingIncome;
        private decimal _netIncome;
        private decimal _otherIncome;

        public IncomeStatementVM()
        {
            Months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            _month = UtilityMethods.GetCurrentDate().Month - 1;
            _year = UtilityMethods.GetCurrentDate().Year;
        }

        public string ForTheDate => "For the Period Ended 31/" + _month + "/" + _year;

        public List<int> Months { get; }

        public int Year
        {
            get { return _year; }
            set
            {
                if (!IsYearIsValid(value)) return;
                SetProperty(ref _year, value, () => Year);
                RefreshIncomeStatement();
            }
        }

        public int Month
        {
            get { return _month; }
            set
            {
                SetProperty(ref _month, value, () => Month);
                RefreshIncomeStatement();
            }
        }

        public decimal Revenues
        {
            get
            {
                _revenues = 0;

                using (var context = UtilityMethods.createContext())
                {
                    var revenues = context.Ledger_Accounts
                        .Where(account => account.Name.Equals("Sales Revenue"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .SingleOrDefault();

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

                using (var context = UtilityMethods.createContext())
                {
                    var otherIncome = context.Ledger_Accounts
                        .Where(account => account.Name.Equals("Other Income"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .SingleOrDefault();

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

                using (var context = UtilityMethods.createContext())
                {
                    var salesReturnsAndAllowancesAccount = context.Ledger_Accounts
                        .Where(account => account.Name.Equals("Sales Returns and Allowances"))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .SingleOrDefault();

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

                using (var context = UtilityMethods.createContext())
                {
                    var cogsAccount = context.Ledger_Accounts
                        .Where(acount => acount.Name.Equals(Constants.COST_OF_GOODS_SOLD))
                        .Include("LedgerGeneral")
                        .Include("LedgerAccountBalances")
                        .SingleOrDefault();

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

                using (var context = UtilityMethods.createContext())
                {
                    var operatingExpenseAccounts = context.Ledger_Accounts
                        .Where(account => account.LedgerAccountGroup.Name.Equals(Constants.OPERATING_EXPENSE))
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

        #region Helper Methods

        private static bool IsYearIsValid(int year)
        {
            using (var context = UtilityMethods.createContext())
            {
                if (context.Ledger_Account_Balances
                        .FirstOrDefault(accountBalance => accountBalance.PeriodYear == year) != null) return true;
                MessageBox.Show(@"Please enter a valid year.", @"Invalid Year", MessageBoxButtons.OK);
                return false;
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
            var periodYearBalances = account.LedgerAccountBalances.Single(acc => acc.PeriodYear.Equals(_year));

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

    #endregion
}