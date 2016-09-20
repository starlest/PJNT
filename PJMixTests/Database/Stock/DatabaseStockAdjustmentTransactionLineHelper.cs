namespace PJMixTests.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.StockCorrection;
    using ECRP.Utilities;

    public static class DatabaseStockAdjustmentTransactionLineHelper
    {
        public static IEnumerable<StockAdjustmentTransactionLine> Get(Func<StockAdjustmentTransactionLine, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.StockAdjustmentTransactionLines.Include("Item").Include("Warehouse").Include("StockAdjustmentTransaction").Include("StockAdjustmentTransaction.User").Where(condition).ToList();
        }
    }
}
