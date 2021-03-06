﻿namespace PJMixTests.Database.Ledger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECERP.Models.Accounting;
    using ECERP.Utilities;

    public static class DatabaseLedgerAccountHelper
    {
        public static IEnumerable<LedgerTransaction> GetAll()
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Transactions.Include("LedgerTransactionLines").ToList();
        }

        public static IEnumerable<LedgerAccount> Get(Func<LedgerAccount, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Accounts.Include("LedgerGeneral").Include("LedgerAccountBalances").Include("LedgerTransactionLines").Where(condition).ToList();
        }

        public static LedgerTransaction FirstOrDefault(Func<LedgerTransaction, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Transactions.Include("LedgerTransactionLines").Where(condition).FirstOrDefault();
        }

        public static IEnumerable<LedgerAccount> GetWithoutLines(Func<LedgerAccount, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Accounts.Include("LedgerGeneral").Include("LedgerAccountBalances").Where(condition).ToList();
        }
    }
}
