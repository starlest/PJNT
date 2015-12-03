﻿using PutraJayaNT.Models.Accounting;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace PutraJayaNT.Utilities
{
    public static class LedgerDBHelper
    {
        public static void AddTransaction(ERPContext context, LedgerTransaction transaction, DateTime date, string documentation, string description)
        {
            transaction.Documentation = documentation;
            transaction.Date = date;
            transaction.Description = description;
            context.Ledger_Transactions.Add(transaction);
        }

        public static void AddTransactionLine(ERPContext context, LedgerTransaction transaction, string accountName, string seq, decimal amount)
        {
            var transactionLine = new LedgerTransactionLine();
            var account = (from Account in context.Ledger_Accounts
                           where Account.Name == accountName
                           select Account).First();
            var ledgerAccount = (from Account in context.Ledger_General
                           where Account.ID == account.ID
                           select Account).First();

            transactionLine.LedgerTransaction = transaction;
            transactionLine.LedgerAccount = account;
            transactionLine.Seq = seq;
            transactionLine.Amount = amount;

            // Update the amount of the account
            if (seq == "Debit") ledgerAccount.Debit += amount;
            else if (seq == "Credit") ledgerAccount.Credit += amount;

            context.Ledger_General.Attach(ledgerAccount);
            context.Ledger_Accounts.Attach(account);
            ((IObjectContextAdapter)context).ObjectContext.
            ObjectStateManager.ChangeObjectState(ledgerAccount, EntityState.Modified);
            context.Ledger_Transaction_Lines.Add(transactionLine);
        }
    }
}
