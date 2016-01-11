using MVVMFramework;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PutraJayaNT.ViewModels.Accounting
{
    class DailyCashFlowVM : ViewModelBase
    {
        ObservableCollection<LedgerTransactionLineVM> _lines;

        DateTime _date;
        decimal _beginningBalance;
        decimal _total;

        public DailyCashFlowVM()
        {
            _lines = new ObservableCollection<LedgerTransactionLineVM>();
            _date = DateTime.Now.Date;
            UpdateLines();
        }

        public ObservableCollection<LedgerTransactionLineVM> Lines
        {
            get { return _lines; }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                SetProperty(ref _date, value, "Date");
                UpdateLines();
            }
        }

        public decimal BeginningBalance
        {
            get { return _beginningBalance; }
            set { SetProperty(ref _beginningBalance, value, "BeginningBalance"); }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, "Total"); }
        }

        #region Helper Methods
        private void UpdateLines()
        {
            _lines.Clear();
            SetBeginningBalance();
            _total = _beginningBalance;
            using (var context = new ERPContext())
            {
                var lines = context.Ledger_Transaction_Lines.Include("LedgerTransaction")
                    .Where(e => e.LedgerAccount.Name.Equals("Cash") &&
                    e.LedgerTransaction.Date.Equals(_date))
                    .ToList();

                foreach (var line in lines)
                {
                    if (line.Seq == "Credit") line.Amount = -line.Amount;
                    _lines.Add(new LedgerTransactionLineVM { Model = line, Balance = _total + line.Amount });
                    _total += line.Amount;
                }
            }

            OnPropertyChanged("Total");
        }

        private void SetBeginningBalance()
        {
            using (var context = new ERPContext())
            {
                var balances = context.Ledger_Account_Balances
                    .Include("LedgerAccount")
                    .Where(e => e.LedgerAccount.Name.Equals("Cash") && e.PeriodYear == _date.Year)
                    .FirstOrDefault();

                if (balances == null)
                {
                    BeginningBalance = 0;
                    return;
                }

                decimal beginningBalance = 0;

                switch(_date.Month)
                {
                    case 1:
                        beginningBalance = balances.BeginningBalance;
                        break;
                    case 2:
                        beginningBalance = balances.Balance1;
                        break;
                    case 3:
                        beginningBalance = balances.Balance2;
                        break;
                    case 4:
                        beginningBalance = balances.Balance3;
                        break;
                    case 5:
                        beginningBalance = balances.Balance4;
                        break;
                    case 6:
                        beginningBalance = balances.Balance5;
                        break;
                    case 7:
                        beginningBalance = balances.Balance6;
                        break;
                    case 8:
                        beginningBalance = balances.Balance7;
                        break;
                    case 9:
                        beginningBalance = balances.Balance8;
                        break;
                    case 10:
                        beginningBalance = balances.Balance9;
                        break;
                    case 11:
                        beginningBalance = balances.Balance10;
                        break;
                    case 12:
                        beginningBalance = balances.Balance11;
                        break;
                    default:
                        break;
                }

                var beginningPeriodDate = _date.AddDays(-_date.Day + 1);
                var lines = context.Ledger_Transaction_Lines.Include("LedgerTransaction")
                    .Where(e => e.LedgerAccount.Name.Equals("Cash") &&
                    e.LedgerTransaction.Date >=  beginningPeriodDate && e.LedgerTransaction.Date < _date)
                    .ToList();

                foreach (var line in lines)
                {
                    if (line.Seq == "Debit")
                        beginningBalance += line.Amount;
                    else
                        beginningBalance -= line.Amount;
                }

                BeginningBalance = beginningBalance;
            }
        }
        #endregion
    }
}
