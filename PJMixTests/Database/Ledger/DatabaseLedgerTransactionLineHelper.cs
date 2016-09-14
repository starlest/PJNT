namespace PJMixTests.Database.Ledger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Accounting;
    using PutraJayaNT.Utilities;

    public static class DatabaseLedgerTransactionLineHelper
    {
        public static IEnumerable<LedgerTransaction> GetAll()
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Transactions.Include("LedgerAccount").Include("LedgerTransactionLines").ToList();
        }

        public static IEnumerable<LedgerTransactionLine> Get(Func<LedgerTransactionLine, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Transaction_Lines.Include("LedgerAccount").Include("LedgerTransaction").Where(condition).ToList();
        }

        public static LedgerTransactionLine FirstOrDefault(Func<LedgerTransactionLine, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Ledger_Transaction_Lines.Include("LedgerAccount").Include("LedgerTransaction").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref LedgerTransaction ledgerTransactionToBeAttached)
        {
            var ledgerTransactionID = ledgerTransactionToBeAttached.ID;
            ledgerTransactionToBeAttached = context.Ledger_Transactions.First(transaction => transaction.ID.Equals(ledgerTransactionID));
        }
    }
}
