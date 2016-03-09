using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Transactions;

namespace PutraJayaNT.ViewModels.Accounting
{
    class ClosingVM : ViewModelBase
    {
        int _periodYear;
        int _period;

        public ClosingVM()
        {
            using (var context = new ERPContext())
            {
                _periodYear = context.Ledger_General.FirstOrDefault().PeriodYear;
                _period = context.Ledger_General.FirstOrDefault().Period;
            }
        }

        public int PeriodYear
        {
            get { return _periodYear; }
            set { SetProperty(ref _periodYear, value, "PeriodYear"); }
        }

        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value, "Period"); }
        }

        public void Close(BackgroundWorker worker)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                var accounts = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalances")
                    .ToList();

                var retainedEarnings = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalances") 
                    .Include("LedgerTransactionLines")               
                    .Where(e => e.Name.Equals("Retained Earnings"))
                    .FirstOrDefault();

                var revenueAndExpenseAccounts = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalances")
                    .Where(e => e.Class.Equals("Expense") || e.Class.Equals("Revenue"))
                    .ToList();

                var index = 1;
                var totalAccounts = accounts.Count + revenueAndExpenseAccounts.Count;
                // Close the Revenue and Expense Accounts to Retained Earnings
                foreach (var account in revenueAndExpenseAccounts)
                {
                    if (account.LedgerGeneral.Debit != 0 || account.LedgerGeneral.Credit != 0)
                        CloseRevenueOrExpenseAccountToRetainedEarnings(account, context);

                    worker.ReportProgress(index++ * (totalAccounts / 100));
                }

                foreach (var account in accounts)
                {
                    if (UtilityMethods.GetCurrentDate().Month == 12)
                    {
                        var newBalance = new LedgerAccountBalance { LedgerAccount = account, PeriodYear = UtilityMethods.GetCurrentDate().Year + 1 };
                        context.Ledger_Account_Balances.Add(newBalance);
                    }

                    if (Period != 12) account.LedgerGeneral.Period++;
                    else
                    {
                        account.LedgerGeneral.PeriodYear++;
                        account.LedgerGeneral.Period = 1;
                    }

                    var periodYearBalances = account.LedgerAccountBalances.Where(e => e.PeriodYear.Equals(UtilityMethods.GetCurrentDate().Year)).FirstOrDefault();

                    // Close the balances
                    if (account.Class.Equals("Asset") || account.Class.Equals("Expense"))
                        CloseAssetOrExpenseAccount(account, context);                     
                    else
                        CloseLiabilityOrRevenueAccount(account, context);

                    worker.ReportProgress(index++ * (totalAccounts / 100));
                }

                context.SaveChanges();
                ts.Complete();
            }

            OnPropertyChanged("PeriodYear");
            OnPropertyChanged("Period");
        }

        private void CloseRevenueOrExpenseAccountToRetainedEarnings(LedgerAccount account, ERPContext context)
        {
            var retainedEarnings = context.Ledger_Accounts
                .Include("LedgerGeneral")
                .Include("LedgerAccountBalances")
                .Include("LedgerTransactionLines")
                .Where(e => e.Name.Equals("Retained Earnings"))
                .FirstOrDefault();

            if (account.Class.Equals("Expense"))
            {
                var amount = account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                var transaction = new LedgerTransaction();

                DatabaseLedgerHelper.AddTransaction(context, transaction, UtilityMethods.GetCurrentDate().Date, "Closing Entry", account.Name);
                context.SaveChanges();
                DatabaseLedgerHelper.AddTransactionLine(context, transaction, account.Name, "Credit", amount);
                DatabaseLedgerHelper.AddTransactionLine(context, transaction, retainedEarnings.Name, "Debit", amount);

                account.LedgerGeneral.Credit -= amount;
            }

            else if (account.Class.Equals("Revenue"))
            {
                var amount = account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                var transaction = new LedgerTransaction();

                if (!DatabaseLedgerHelper.AddTransaction(context, transaction, UtilityMethods.GetCurrentDate().Date, "Closing Entry", account.Name)) return;
                context.SaveChanges();
                DatabaseLedgerHelper.AddTransactionLine(context, transaction, account.Name, "Debit", amount);
                DatabaseLedgerHelper.AddTransactionLine(context, transaction, retainedEarnings.Name, "Credit", amount);

                account.LedgerGeneral.Debit -= amount;
            }
        }

        private void CloseAssetOrExpenseAccount(LedgerAccount account, ERPContext context)
        {
            var periodYearBalances = account.LedgerAccountBalances.Where(e => e.PeriodYear.Equals(UtilityMethods.GetCurrentDate().Year)).FirstOrDefault();

            switch (Period)
            {
                case 1:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance1 += periodYearBalances.BeginningBalance;

                    periodYearBalances.Balance1 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 2:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance2 += periodYearBalances.Balance1;

                    periodYearBalances.Balance2 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 3:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance3 += periodYearBalances.Balance2;

                    periodYearBalances.Balance3 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 4:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance4 += periodYearBalances.Balance3;

                    periodYearBalances.Balance4 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 5:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance5 += periodYearBalances.Balance4;

                    periodYearBalances.Balance5 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 6:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance6 += periodYearBalances.Balance5;

                    periodYearBalances.Balance6 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 7:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance7 += periodYearBalances.Balance6;

                    periodYearBalances.Balance7 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 8:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance8 += periodYearBalances.Balance7;

                    periodYearBalances.Balance8 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 9:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance9 += periodYearBalances.Balance8;

                    periodYearBalances.Balance9 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 10:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance10 += periodYearBalances.Balance9;

                    periodYearBalances.Balance10 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 11:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance11 += periodYearBalances.Balance10;

                    periodYearBalances.Balance11 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 12:
                    if (!account.Class.Equals("Expense"))
                        periodYearBalances.Balance12 += periodYearBalances.Balance11;

                    periodYearBalances.Balance12 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                default:
                    break;
            }
        }

        private void CloseLiabilityOrRevenueAccount(LedgerAccount account, ERPContext context)
        {
            var periodYearBalances = account.LedgerAccountBalances.Where(e => e.PeriodYear.Equals(UtilityMethods.GetCurrentDate().Year)).FirstOrDefault();

            switch (Period)
            {
                case 1:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance1 += periodYearBalances.BeginningBalance;

                    periodYearBalances.Balance1 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 2:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance2 += periodYearBalances.Balance1;

                    periodYearBalances.Balance2 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 3:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance3 += periodYearBalances.Balance2;

                    periodYearBalances.Balance3 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 4:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance4 += periodYearBalances.Balance3;

                    periodYearBalances.Balance4 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 5:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance5 += periodYearBalances.Balance4;

                    periodYearBalances.Balance5 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 6:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance6 += periodYearBalances.Balance5;

                    periodYearBalances.Balance6 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 7:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance7 += periodYearBalances.Balance6;

                    periodYearBalances.Balance7 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 8:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance8 += periodYearBalances.Balance7;

                    periodYearBalances.Balance8 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 9:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance9 += periodYearBalances.Balance8;

                    periodYearBalances.Balance9 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 10:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance10 += periodYearBalances.Balance9;

                    periodYearBalances.Balance10 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 11:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance11 += periodYearBalances.Balance10;

                    periodYearBalances.Balance11 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 12:
                    if (!account.Class.Equals("Revenue"))
                        periodYearBalances.Balance12 += periodYearBalances.Balance11;

                    periodYearBalances.Balance12 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                default:
                    break;
            }
        }
    }
}
