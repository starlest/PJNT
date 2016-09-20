namespace PJMixTests.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.StockCorrection;
    using ECRP.Utilities;

    public static class DatabaseStockMovementTransactionHelper
    {
        public static IEnumerable<StockMovementTransaction> Get(Func<StockMovementTransaction, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.StockMovementTransactions.Include("FromWarehouse").Include("ToWarehouse").Include("StockMovementTransactionLines").Include("StockMovementTransactionLines.Item").Include("User").Where(condition).ToList();
        }
    }
}
