namespace PutraJayaNT.Utilities.Database.Purchase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Purchase;

    public static class DatabasePurchaseTransactionLineHelper
    {
        public static IEnumerable<PurchaseTransactionLine> Get(Func<PurchaseTransactionLine, bool> condition)
        {
            using (var context = new ERPContext())
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
