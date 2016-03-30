namespace PJMixTests.Database.Purchase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Purchase;
    using PutraJayaNT.Utilities;

    public static class DatabasePurchaseTransactionLineHelper
    {
        public static IEnumerable<PurchaseTransactionLine> Get(Func<PurchaseTransactionLine, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.PurchaseTransactionLines
                    .Include("Item")
                    .Include("Warehouse")
                    .Include("PurchaseTransaction")
                    .Include("PurchaseTransaction.Supplier")
                    .Where(condition)
                    .ToList();
        }
    }
}
