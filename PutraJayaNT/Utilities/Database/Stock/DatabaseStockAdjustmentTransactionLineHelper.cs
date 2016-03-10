namespace PutraJayaNT.Utilities.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.StockCorrection;

    public static class DatabaseStockAdjustmentTransactionLineHelper
    {
        public static IEnumerable<AdjustStockTransactionLine> Get(Func<AdjustStockTransactionLine, bool> condition)
        {
            using (var context = new ERPContext())
                return context.AdjustStockTransactionLines.Include("Item").Include("Warehouse").Include("AdjustStockTransaction").Include("AdjustStockTransaction.User").Where(condition).ToList();
        }
    }
}
