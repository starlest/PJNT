using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Reports.Windows;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Accounting
{
    public class DailyCashFlowVM : ViewModelBase
    {
        DateTime _date;
        decimal _beginningBalance;
        decimal _endingBalance;

        ICommand _printCommand;

        public DailyCashFlowVM()
        {
            Lines = new ObservableCollection<LedgerTransactionLineVM>();
            _date = UtilityMethods.GetCurrentDate().Date;
            UpdateLines();
        }

        public ObservableCollection<LedgerTransactionLineVM> Lines { get; }

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

        public decimal EndingBalance
        {
            get { return _endingBalance; }
            set { SetProperty(ref _endingBalance, value, "EndingBalance"); }
        }

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (Lines.Count == 0) return;

                    var dailyCashFlowReportWindow = new DailyCashFlowReportWindow(this);
                    dailyCashFlowReportWindow.Owner = App.Current.MainWindow;
                    dailyCashFlowReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    dailyCashFlowReportWindow.Show();
                }));
            }
        }

        #region Helper Methods
        private void UpdateLines()
        {
            Lines.Clear();
            SetBeginningBalance();
            _endingBalance = _beginningBalance;
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var lines = context.Ledger_Transaction_Lines
                    .Include("LedgerAccount")
                    .Include("LedgerTransaction")
                    .Where(e => e.LedgerAccount.Name.Equals("Cash") &&
                    e.LedgerTransaction.Date.Equals(_date))
                    .ToList();

                var account = new LedgerAccount { ID = -1, Name = "Sales Receipt", Class = "Asset" };
                var transaction = new LedgerTransaction { ID = -1, Date = _date, Description = "Sales Receipt", Documentation = "Sales Receipt" };
                var srLine = new LedgerTransactionLine { LedgerTransaction = transaction, LedgerAccount = account, Seq = "Debit" };
                foreach (var line in lines)
                {
                    var lineVM = new LedgerTransactionLineVM { Model = line };
                    foreach (var l in lineVM.OpposingLines)
                    {
                        if (!l.Description.Equals("Sales Transaction Receipt"))
                        {
                            if (line.Seq == "Credit") l.Amount = -l.Amount;
                            Lines.Add(new LedgerTransactionLineVM { Model = l.Model, Balance = _beginningBalance + l.Amount });
                        }

                        else
                        {
                            srLine.Amount += l.Amount;
                        }
                        _endingBalance += l.Amount;
                        l.Balance = _endingBalance;
                    }
                }
                Lines.Add(new LedgerTransactionLineVM { Model = srLine, Balance = _endingBalance });
            }

            OnPropertyChanged("EndingBalance");
        }

        private void SetBeginningBalance()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var balances = context.Ledger_Account_Balances
                    .Include("LedgerAccount")
                    .FirstOrDefault(e => e.LedgerAccount.Name.Equals("Cash") && e.PeriodYear == _date.Year);

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
