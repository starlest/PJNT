namespace PJMixTests.Database.Purchase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Purchase;
    using PutraJayaNT.Utilities;

    public static class DatabasePurchaseReturnTransactionLineHelper
    {
        public static IEnumerable<PurchaseReturnTransactionLine> Get(Func<PurchaseReturnTransactionLine, bool> condition)
        {
            using (var context = new ERPContext())
                return context.PurchaseReturnTransactionLines.Include("Item").Include("Warehouse").Include("PurchaseReturnTransaction").Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier").Where(condition).ToList();
        }
    }
}
