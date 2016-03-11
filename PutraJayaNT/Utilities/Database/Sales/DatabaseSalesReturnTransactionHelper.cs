namespace PutraJayaNT.Utilities.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Sales;
    using Utilities;

    public static class DatabaseSalesReturnTransactionHelper
    {
        public static IEnumerable<SalesReturnTransaction> Get(Func<SalesReturnTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesReturnTransactions
                    .Include("SalesTransaction")
                    .Include("SalesTransaction.Customer")
                    .Include("SalesReturnTransactionLines")
                    .Include("SalesReturnTransactionLines.Warehouse")
                    .Include("SalesReturnTransactionLines.Item")
                    .Where(condition)
                    .ToList();
        }

        public static SalesReturnTransaction FirstOrDefault(Func<SalesReturnTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesReturnTransactions
                    .Include("SalesTransaction")
                    .Include("SalesTransaction.Customer")
                    .Include("SalesReturnTransactionLines")
                    .Include("SalesReturnTransactionLines.Warehouse")
                    .Include("SalesReturnTransactionLines.Item")
                    .Where(condition)
                    .FirstOrDefault();
        }
    }
}