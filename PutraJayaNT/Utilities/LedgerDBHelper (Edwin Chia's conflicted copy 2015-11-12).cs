using PUJASM.ERP.Models.Accounting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUJASM.ERP.Utilities
{
    public static class LedgerDBHelper
    {
        public static void AddTransaction(ERPContext context, LedgerTransaction transaction, string accountName, string description, string seq, decimal amount)
        {
            var account = (from Account in context.Ledger_Accounts
                           where Account.Name == accountName
                           select Account).First();
            var ledgerAccount = (from Account in context.Ledger_General
                           where Account.ID == account.ID
                           select Account).First();

            transaction.Account = account;
            transaction.DateStamp = DateTime.Now;
            transaction.Description = description;
            transaction.Seq = seq;
            transaction.Amount = amount;

            // Update the amount of the account
            if (seq == "Debit") ledgerAccount.Debit += amount;
            else if (seq == "Credit") ledgerAccount.Credit += amount;

            context.Ledger_General.Attach(ledgerAccount);
            context.Ledger_Accounts.Attach(account);
            context.Ledger_Transactions.Add(transaction);
            ((IObjectContextAdapter)context).ObjectContext.
            ObjectStateManager.ChangeObjectState(ledgerAccount, EntityState.Modified);
        }
    }
}
