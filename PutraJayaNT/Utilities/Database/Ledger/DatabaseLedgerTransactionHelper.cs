using System;
using System.Collections.Generic;
using System.Linq;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;

namespace PutraJayaNT.Utilities.Database.Ledger
{
    public static class DatabaseLedgerTransactionHelper
    {
        public static IEnumerable<LedgerTransaction> GetAll()
        {
            using (var context = new ERPContext())
                return context.Ledger_Transactions.Include("LedgerTransactionLines").ToList();
        }

        public static IEnumerable<LedgerTransaction> Get(Func<LedgerTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Ledger_Transactions.Include("LedgerTransactionLines").Where(condition).ToList();
        }

        public static LedgerTransaction FirstOrDefault(Func<LedgerTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Ledger_Transactions.Include("LedgerTransactionLines").Where(condition).FirstOrDefault();
        }

        public static IEnumerable<LedgerTransaction> GetWithoutLines(Func<LedgerTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Ledger_Transactions.Where(condition).ToList();
        }
    }
}
