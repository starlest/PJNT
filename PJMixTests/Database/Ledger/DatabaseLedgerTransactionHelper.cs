namespace PJMixTests.Database.Ledger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Accounting;
    using PutraJayaNT.Utilities;

    public static class DatabaseLedgerTransactionHelper
    {
        public static IEnumerable<LedgerTransaction> GetAll()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                return context.Ledger_Transactions.Include("LedgerTransactionLines").ToList();
        }

        public static IEnumerable<LedgerTransaction> Get(Func<LedgerTransaction, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                return context.Ledger_Transactions.Include("LedgerTransactionLines").Where(condition).ToList();
        }

        public static LedgerTransaction FirstOrDefault(Func<LedgerTransaction, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                return context.Ledger_Transactions.Include("LedgerTransactionLines").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref LedgerTransaction ledgerTransactionToBeAttached)
        {
            var ledgerTransactionID = ledgerTransactionToBeAttached.ID;
            ledgerTransactionToBeAttached = context.Ledger_Transactions.First(transaction => transaction.ID.Equals(ledgerTransactionID));
        }

        public static IEnumerable<LedgerTransaction> GetWithoutLines(Func<LedgerTransaction, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                return context.Ledger_Transactions.Where(condition).ToList();
        }
    }
}
