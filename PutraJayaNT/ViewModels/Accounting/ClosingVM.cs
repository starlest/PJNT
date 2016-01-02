using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Linq;
using System.Transactions;
using System.Windows;

namespace PutraJayaNT.ViewModels.Accounting
{
    class ClosingVM : ViewModelBase
    {
        public int PeriodYear
        {
            get
            {
                using (var context = new ERPContext())
                {
                    return context.Ledger_General.FirstOrDefault().PeriodYear;
                }
            }
        }

        public int Period
        {
            get
            {
                using (var context = new ERPContext())
                {
                    return context.Ledger_General.FirstOrDefault().Period;
                }
            }
        }

        public void Close()
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                var accounts = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalance")
                    .ToList();

                var retainedEarnings = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalance") 
                    .Include("TransactionLines")               
                    .Where(e => e.Name.Equals("Retained Earnings"))
                    .FirstOrDefault();

                var revenueAndExpenseAccounts = context.Ledger_Accounts
                    .Include("LedgerGeneral")
                    .Include("LedgerAccountBalance")
                    .Where(e => e.Class.Equals("Expense") || e.Class.Equals("Revenue"))
                    .ToList();

                // Close the Revenue and Expense Accounts to Retained Earnings
                foreach (var account in revenueAndExpenseAccounts)
                {
                    if (account.LedgerGeneral.Debit != 0 || account.LedgerGeneral.Credit != 0)
                        CloseRevenueOrExpenseAccount(account, context);
                }

                foreach (var account in accounts)
                {
                    // Close the balances
                    if (account.Class.Equals("Asset") || account.Class.Equals("Expense"))
                    {
                        switch (Period)
                        {
                            case 1:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance1 += account.LedgerAccountBalance.BeginningBalance;
                                
                                account.LedgerAccountBalance.Balance1 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 2:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance2 += account.LedgerAccountBalance.Balance1;

                                account.LedgerAccountBalance.Balance2 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 3:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance3 += account.LedgerAccountBalance.Balance2;

                                account.LedgerAccountBalance.Balance3 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 4:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance4 += account.LedgerAccountBalance.Balance3;

                                account.LedgerAccountBalance.Balance4 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 5:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance5 += account.LedgerAccountBalance.Balance4;

                                account.LedgerAccountBalance.Balance5 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 6:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance6 += account.LedgerAccountBalance.Balance5;

                                account.LedgerAccountBalance.Balance6 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 7:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance7 += account.LedgerAccountBalance.Balance6;

                                account.LedgerAccountBalance.Balance7 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 8:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance8 += account.LedgerAccountBalance.Balance7;

                                account.LedgerAccountBalance.Balance8 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 9:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance9 += account.LedgerAccountBalance.Balance8;

                                account.LedgerAccountBalance.Balance9 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 10:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance10 += account.LedgerAccountBalance.Balance9;

                                account.LedgerAccountBalance.Balance10 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 11:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance11 += account.LedgerAccountBalance.Balance10;

                                account.LedgerAccountBalance.Balance11 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 12:
                                if (!account.Class.Equals("Expense"))
                                    account.LedgerAccountBalance.Balance12 += account.LedgerAccountBalance.Balance11;

                                account.LedgerAccountBalance.Balance12 += account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;
                                account.LedgerGeneral.PeriodYear++;

                                account.LedgerGeneral.Period = 1;
                                break;
                            default:
                                break;
                        }
                    }

                    else
                    {
                        switch (Period)
                        {
                            case 1:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance1 += account.LedgerAccountBalance.BeginningBalance;

                                account.LedgerAccountBalance.Balance1 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 2:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance2 += account.LedgerAccountBalance.Balance1;

                                account.LedgerAccountBalance.Balance2 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 3:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance3 += account.LedgerAccountBalance.Balance2;

                                account.LedgerAccountBalance.Balance3 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 4:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance4 += account.LedgerAccountBalance.Balance3;

                                account.LedgerAccountBalance.Balance4 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 5:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance5 += account.LedgerAccountBalance.Balance4;

                                account.LedgerAccountBalance.Balance5 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 6:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance6 += account.LedgerAccountBalance.Balance5;

                                account.LedgerAccountBalance.Balance6 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 7:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance7 += account.LedgerAccountBalance.Balance6;

                                account.LedgerAccountBalance.Balance7 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 8:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance8 += account.LedgerAccountBalance.Balance7;

                                account.LedgerAccountBalance.Balance8 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 9:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance9 += account.LedgerAccountBalance.Balance8;

                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 10:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance10 += account.LedgerAccountBalance.Balance9;

                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 11:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance11 += account.LedgerAccountBalance.Balance10;

                                account.LedgerAccountBalance.Balance11 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.Period++;
                                break;
                            case 12:
                                if (!account.Class.Equals("Revenue"))
                                    account.LedgerAccountBalance.Balance12 += account.LedgerAccountBalance.Balance11;

                                account.LedgerAccountBalance.Balance12 += account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                                account.LedgerGeneral.Debit = 0;
                                account.LedgerGeneral.Credit = 0;

                                account.LedgerGeneral.PeriodYear++;
                                account.LedgerGeneral.Period = 1;
                                break;
                            default:
                                break;
                        }
                    }
                }

                context.SaveChanges();
                ts.Complete();
            }

            OnPropertyChanged("PeriodYear");
            OnPropertyChanged("Period");
        }

        void CloseRevenueOrExpenseAccount(LedgerAccount account, ERPContext context)
        {
            var retainedEarnings = context.Ledger_Accounts
                .Include("LedgerGeneral")
                .Include("LedgerAccountBalance")
                .Include("TransactionLines")
                .Where(e => e.Name.Equals("Retained Earnings"))
                .FirstOrDefault();

            if (account.Class.Equals("Expense"))
            {
                var amount = account.LedgerGeneral.Debit - account.LedgerGeneral.Credit;
                var transaction = new LedgerTransaction();

                LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, "Closing Entry", account.Name);
                context.SaveChanges();
                LedgerDBHelper.AddTransactionLine(context, transaction, account.Name, "Credit", amount);
                LedgerDBHelper.AddTransactionLine(context, transaction, retainedEarnings.Name, "Debit", amount);

                account.LedgerGeneral.Credit -= amount;
            }

            else if (account.Class.Equals("Revenue"))
            {
                var amount = account.LedgerGeneral.Credit - account.LedgerGeneral.Debit;
                var transaction = new LedgerTransaction();

                LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, "Closing Entry", account.Name);
                context.SaveChanges();
                LedgerDBHelper.AddTransactionLine(context, transaction, account.Name, "Debit", amount);
                LedgerDBHelper.AddTransactionLine(context, transaction, retainedEarnings.Name, "Credit", amount);

                account.LedgerGeneral.Debit -= amount;
            }
        }
    }
}
