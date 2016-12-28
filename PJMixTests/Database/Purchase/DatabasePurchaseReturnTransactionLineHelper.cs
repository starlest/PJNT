namespace PJMixTests.Database.Purchase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECERP.Models.Purchase;
    using ECERP.Utilities;

    public static class DatabasePurchaseReturnTransactionLineHelper
    {
        public static IEnumerable<PurchaseReturnTransactionLine> Get(Func<PurchaseReturnTransactionLine, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.PurchaseReturnTransactionLines.Include("Item").Include("Warehouse").Include("PurchaseReturnTransaction").Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier").Where(condition).ToList();
        }
    }
}
