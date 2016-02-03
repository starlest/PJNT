using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using System;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.Utilities
{
    public static class LedgerDBHelper
    {
        public static bool AddTransaction(ERPContext context, LedgerTransaction transaction, DateTime date, string documentation, string description)
        {
            using (var c = new ERPContext())
            {
                var period = c.Ledger_General.FirstOrDefault().Period;
                if (period != date.Month)
                {
                    MessageBox.Show("Wrong period.", "Invalid Transaction", MessageBoxButton.OK);
                    return false;
                }

                if ((date.Date.Day != DateTime.Now.Date.Day))
                {
                    if (!(date.AddDays(2).Day <= DateTime.Now.Day && date.AddDays(2).Day >= date.Day))
                    {
                        MessageBox.Show("Wrong period.", "Invalid Transaction", MessageBoxButton.OK);
                        return false;
                    }
                }
            }

            transaction.Documentation = documentation;
            transaction.Date = date;
            transaction.Description = description;
            var user = App.Current.TryFindResource("CurrentUser") as User;
            if (user != null) transaction.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
            context.Ledger_Transactions.Add(transaction);

            return true;
        }

        public static void AddTransactionLine(ERPContext context, LedgerTransaction transaction, string accountName, string seq, decimal amount)
        {
            var transactionLine = new LedgerTransactionLine();
            var account = context.Ledger_Accounts
                .Where(e => e.Name.Equals(accountName))
                .FirstOrDefault();
            var ledgerAccount = context.Ledger_General
                .Where(e => e.ID.Equals(account.ID))
                .FirstOrDefault(); 

            transactionLine.LedgerTransaction = transaction;
            transactionLine.LedgerAccount = account;
            transactionLine.Seq = seq;
            transactionLine.Amount = amount;

            // Update the amount of the account
            if (seq == "Debit") ledgerAccount.Debit += amount;
            else if (seq == "Credit") ledgerAccount.Credit += amount;

            context.Ledger_Transaction_Lines.Add(transactionLine);
        }
    }
}
