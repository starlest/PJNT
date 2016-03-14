namespace PJMixTests.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.StockCorrection;
    using PutraJayaNT.Utilities;

    public static class DatabaseStockAdjustmentTransactionLineHelper
    {
        public static IEnumerable<AdjustStockTransactionLine> Get(Func<AdjustStockTransactionLine, bool> condition)
        {
            using (var context = new ERPContext())
                return context.AdjustStockTransactionLines.Include("Item").Include("Warehouse").Include("AdjustStockTransaction").Include("AdjustStockTransaction.User").Where(condition).ToList();
        }
    }
}
