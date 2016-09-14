namespace PutraJayaNT.ViewModels.Accounting
{
    using Ledger;
    using MVVMFramework;
    using Models.Accounting;
    using PutraJayaNT.Reports.Windows;
    using Utilities;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    public class DailyCashFlowVM : ViewModelBase
    {
        private DateTime _date;
        private decimal _beginningBalance;
        private decimal _endingBalance;

        private ICommand _printCommand;

        public DailyCashFlowVM()
        {
            DisplayedLines = new ObservableCollection<LedgerTransactionLineVM>();
            _date = UtilityMethods.GetCurrentDate().Date;
            UpdateDisplayedLines();
        }

        public ObservableCollection<LedgerTransactionLineVM> DisplayedLines { get; }

        #region Properties
        public DateTime Date
        {
            get { return _date; }
            set
            {
                SetProperty(ref _date, value, () => Date);
                UpdateDisplayedLines();
            }
        }

        public decimal BeginningBalance
        {
            get { return _beginningBalance; }
            set { SetProperty(ref _beginningBalance, value, () => BeginningBalance); }
        }

        public decimal EndingBalance
        {
            get { return _endingBalance; }
            set { SetProperty(ref _endingBalance, value, () => EndingBalance); }
        }
        #endregion

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (DisplayedLines.Count == 0) return;
                    ShowPrintWindow();
                }));
            }
        }

        #region Helper Methods
        private void UpdateDisplayedLines()
        {
            DisplayedLines.Clear();
            SetBeginningBalance();
            _endingBalance = _beginningBalance;
            using (var context = UtilityMethods.createContext())
            {
                var lines = context.Ledger_Transaction_Lines
                    .Include("LedgerAccount")
                    .Include("LedgerTransaction")
                    .Where(line => line.LedgerAccount.Name.Equals("Cash") &&
                    line.LedgerTransaction.Date.Equals(_date))
                    .ToList();

                var account = new LedgerAccount { ID = -1, Name = "Sales Receipt", Class = "Asset" };
                var transaction = new LedgerTransaction { ID = -1, Date = _date, Description = "Sales Receipt", Documentation = "Sales Receipt" };
                var salesreceiptLine = new LedgerTransactionLine { LedgerTransaction = transaction, LedgerAccount = account, Seq = "Debit" };
                foreach (var line in lines)
                {
                    var lineVM = new LedgerTransactionLineVM { Model = line };
                    foreach (var oppositeLine in lineVM.OpposingLines)
                    {
                        if (!oppositeLine.Description.Equals("Sales Transaction Receipt"))
                        {
                            if (line.Seq == "Credit") oppositeLine.Amount = -oppositeLine.Amount;
                            DisplayedLines.Add(new LedgerTransactionLineVM { Model = oppositeLine.Model, Balance = _beginningBalance + oppositeLine.Amount });
                        }

                        else
                            salesreceiptLine.Amount += oppositeLine.Amount;
                        
                        _endingBalance += oppositeLine.Amount;
                        oppositeLine.Balance = _endingBalance;
                    }
                }
                if (salesreceiptLine.Amount > 0)
                    DisplayedLines.Add(new LedgerTransactionLineVM { Model = salesreceiptLine, Balance = _endingBalance });
            }
            OnPropertyChanged("EndingBalance");
        }

        private void SetBeginningBalance()
        {
            using (var context = UtilityMethods.createContext())
            {
                var cashBalance = context.Ledger_Account_Balances
                    .Include("LedgerAccount")
                    .SingleOrDefault(e => e.LedgerAccount.Name.Equals("Cash") && e.PeriodYear == _date.Year);

                if (cashBalance == null)
                {
                    BeginningBalance = 0;
                    return;
                }

                var beginningBalance = GetPeriodBeginningBalance(cashBalance);
                var beginningPeriodDate = _date.AddDays(-_date.Day + 1);

                var lines = context.Ledger_Transaction_Lines.Include("LedgerTransaction")
                    .Where(line => line.LedgerAccount.Name.Equals("Cash") &&
                    line.LedgerTransaction.Date >= beginningPeriodDate &&
                    line.LedgerTransaction.Date < _date)
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

        private decimal GetPeriodBeginningBalance(LedgerAccountBalance cashBalance)
        {
            switch (_date.Month)
            {
                case 1:
                    return cashBalance.BeginningBalance;
                case 2:
                    return cashBalance.Balance1;
                case 3:
                    return cashBalance.Balance2;
                case 4:
                    return cashBalance.Balance3;
                case 5:
                    return cashBalance.Balance4;
                case 6:
                    return cashBalance.Balance5;
                case 7:
                    return cashBalance.Balance6;
                case 8:
                    return cashBalance.Balance7;
                case 9:
                    return cashBalance.Balance8;
                case 10:
                    return cashBalance.Balance9;
                case 11:
                    return cashBalance.Balance10;
                default:
                    return cashBalance.Balance11;
            }
        }

        private void ShowPrintWindow()
        {
            var dailyCashFlowReportWindow = new DailyCashFlowReportWindow(this)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dailyCashFlowReportWindow.Show();
        }
        #endregion
    }
}
