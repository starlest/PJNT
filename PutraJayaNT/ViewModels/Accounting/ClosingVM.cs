namespace ECRP.ViewModels.Accounting
{
    using System.ComponentModel;
    using System.Linq;
    using System.Transactions;
    using Models.Accounting;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;

    internal class ClosingVM : ViewModelBase
    {
        private int _periodYear;
        private int _period;

        public ClosingVM()
        {
            using (var context = UtilityMethods.createContext())
            {
                _periodYear = context.Ledger_General.First().PeriodYear;
                _period = context.Ledger_General.First().Period;
            }
        }

        public int PeriodYear
        {
            get { return _periodYear; }
            set { SetProperty(ref _periodYear, value, () => PeriodYear); }
        }

        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value, () => Period); }
        }

        public void Close(BackgroundWorker worker)
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                var accounts = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalances")
                    .ToList();

                var revenueAndExpenseAccounts = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalances")
                    .Where(
                        account =>
                            account.LedgerAccountClass.Name.Equals(Constants.EXPENSE) ||
                            account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                    .ToList();

                var index = 1;
                var totalAccounts = accounts.Count + revenueAndExpenseAccounts.Count;
                // Close the Revenue and Expense Accounts to Retained Earnings
                foreach (var account in revenueAndExpenseAccounts)
                {
                    if (account.LedgerGeneral.Debit != 0 || account.LedgerGeneral.Credit != 0)
                        CloseRevenueOrExpenseAccountToRetainedEarnings(account, context);

                    worker.ReportProgress(index++*(totalAccounts/100));
                }

                foreach (var account in accounts)
                {
                    if (UtilityMethods.GetCurrentDate().Month == 12)
                    {
                        var newBalance = new LedgerAccountBalance
                        {
                            LedgerAccount = account,
                            PeriodYear = UtilityMethods.GetCurrentDate().Year + 1
                        };
                        context.Ledger_Account_Balances.Add(newBalance);
                    }

                    if (Period != 12) account.LedgerGeneral.Period++;
                    else
                    {
                        account.LedgerGeneral.PeriodYear++;
                        account.LedgerGeneral.Period = 1;
                    }

                    // Close the balances
                    if (account.LedgerAccountClass.Name.Equals(Constants.ASSET) ||
                        account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        CloseAssetOrExpenseAccount(account, context);
                    else
                        CloseLiabilityOrRevenueAccount(account, context);

                    worker.ReportProgress(index++*(totalAccounts/100));
                }

                context.SaveChanges();
                ts.Complete();
            }

            OnPropertyChanged("PeriodYear");
            OnPropertyChanged("Period");
        }

        private static void CloseRevenueOrExpenseAccountToRetainedEarnings(LedgerAccount account, ERPContext context)
        {
            var retainedEarnings = context.Ledger_Accounts
                .Include("LedgerGeneral")
                .Include("LedgerAccountBalances")
                .Include("LedgerTransactionLines")
                .Single(e => e.Name.Equals("Retained Earnings"));

            if (account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
            {
                var amount = account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                var transaction = new LedgerTransaction();

                LedgerTransactionHelper.AddTransactionToDatabase(context, transaction,
                    UtilityMethods.GetCurrentDate().Date, "Closing Entry", account.Name);
                context.SaveChanges();
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, account.Name,
                    Constants.CREDIT,
                    amount);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, retainedEarnings.Name,
                    Constants.DEBIT, amount);

                account.LedgerGeneral.Credit -= amount;
            }

            else if (account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
            {
                var amount = account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                var transaction = new LedgerTransaction();

                if (
                    !LedgerTransactionHelper.AddTransactionToDatabase(context, transaction,
                        UtilityMethods.GetCurrentDate().Date, "Closing Entry", account.Name)) return;
                context.SaveChanges();
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, account.Name, Constants.DEBIT,
                    amount);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, retainedEarnings.Name,
                    Constants.CREDIT, amount);

                account.LedgerGeneral.Debit -= amount;
            }
        }

        private void CloseAssetOrExpenseAccount(LedgerAccount account, ERPContext context)
        {
            var periodYearBalances =
                account.LedgerAccountBalances.First(
                    balance => balance.PeriodYear.Equals(UtilityMethods.GetCurrentDate().Year));
            switch (Period)
            {
                case 1:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance1 += periodYearBalances.BeginningBalance;

                    periodYearBalances.Balance1 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 2:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance2 += periodYearBalances.Balance1;

                    periodYearBalances.Balance2 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 3:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance3 += periodYearBalances.Balance2;

                    periodYearBalances.Balance3 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 4:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance4 += periodYearBalances.Balance3;

                    periodYearBalances.Balance4 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 5:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance5 += periodYearBalances.Balance4;

                    periodYearBalances.Balance5 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 6:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance6 += periodYearBalances.Balance5;

                    periodYearBalances.Balance6 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 7:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance7 += periodYearBalances.Balance6;

                    periodYearBalances.Balance7 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 8:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance8 += periodYearBalances.Balance7;

                    periodYearBalances.Balance8 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 9:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance9 += periodYearBalances.Balance8;

                    periodYearBalances.Balance9 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 10:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance10 += periodYearBalances.Balance9;

                    periodYearBalances.Balance10 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 11:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance11 += periodYearBalances.Balance10;

                    periodYearBalances.Balance11 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 12:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.EXPENSE))
                        periodYearBalances.Balance12 += periodYearBalances.Balance11;

                    periodYearBalances.Balance12 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
            }
            context.SaveChanges();
        }

        private void CloseLiabilityOrRevenueAccount(LedgerAccount account, ERPContext context)
        {
            var periodYearBalances =
                account.LedgerAccountBalances.First(
                    balance => balance.PeriodYear.Equals(UtilityMethods.GetCurrentDate().Year));
            switch (Period)
            {
                case 1:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance1 += periodYearBalances.BeginningBalance;

                    periodYearBalances.Balance1 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 2:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance2 += periodYearBalances.Balance1;

                    periodYearBalances.Balance2 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 3:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance3 += periodYearBalances.Balance2;

                    periodYearBalances.Balance3 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 4:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance4 += periodYearBalances.Balance3;

                    periodYearBalances.Balance4 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 5:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance5 += periodYearBalances.Balance4;

                    periodYearBalances.Balance5 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 6:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance6 += periodYearBalances.Balance5;

                    periodYearBalances.Balance6 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 7:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance7 += periodYearBalances.Balance6;

                    periodYearBalances.Balance7 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 8:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance8 += periodYearBalances.Balance7;

                    periodYearBalances.Balance8 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 9:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance9 += periodYearBalances.Balance8;

                    periodYearBalances.Balance9 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 10:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance10 += periodYearBalances.Balance9;

                    periodYearBalances.Balance10 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 11:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance11 += periodYearBalances.Balance10;

                    periodYearBalances.Balance11 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
                case 12:
                    if (!account.LedgerAccountClass.Name.Equals(Constants.REVENUE))
                        periodYearBalances.Balance12 += periodYearBalances.Balance11;

                    periodYearBalances.Balance12 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                    account.LedgerGeneral.Debit = 0;
                    account.LedgerGeneral.Credit = 0;
                    break;
            }
            context.SaveChanges();
        }
    }
}