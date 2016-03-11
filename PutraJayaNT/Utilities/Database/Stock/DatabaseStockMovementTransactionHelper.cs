namespace PutraJayaNT.Utilities.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.StockCorrection;
    using Utilities;

    public static class DatabaseStockMovementTransactionHelper
    {
        public static IEnumerable<MoveStockTransaction> Get(Func<MoveStockTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return
                    context.MoveStockTransactions.Include("FromWarehouse")
                        .Include("ToWarehouse")
                        .Include("MoveStockTransactionLines")
                        .Include("MoveStockTransactionLines.Item")
                        .Include("User")
                        .Where(condition)
                        .ToList();
        }
    }
}