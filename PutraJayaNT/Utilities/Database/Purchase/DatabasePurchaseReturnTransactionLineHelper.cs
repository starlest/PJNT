namespace PutraJayaNT.Utilities.Database.Purchase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Purchase;

    public static class DatabasePurchaseReturnTransactionLineHelper
    {
        public static IEnumerable<PurchaseReturnTransactionLine> Get(Func<PurchaseReturnTransactionLine, bool> condition)
        {
            using (var context = new ERPContext())
                return context.PurchaseReturnTransactionLines.Include("Item").Include("Warehouse").Include("PurchaseReturnTransaction").Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier").Where(condition).ToList();
        }
    }
}
