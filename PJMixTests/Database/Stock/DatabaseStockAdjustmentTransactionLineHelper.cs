namespace PJMixTests.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.StockCorrection;
    using PutraJayaNT.Utilities;

    public static class DatabaseStockAdjustmentTransactionLineHelper
    {
        public static IEnumerable<StockAdjustmentTransactionLine> Get(Func<StockAdjustmentTransactionLine, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                return context.StockAdjustmentTransactionLines.Include("Item").Include("Warehouse").Include("StockAdjustmentTransaction").Include("StockAdjustmentTransaction.User").Where(condition).ToList();
        }
    }
}
