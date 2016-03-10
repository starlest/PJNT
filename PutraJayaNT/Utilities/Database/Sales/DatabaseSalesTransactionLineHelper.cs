namespace PutraJayaNT.Utilities.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Sales;

    public static class DatabaseSalesTransactionLineHelper
    {
        public static IEnumerable<SalesTransactionLine> Get(Func<SalesTransactionLine, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesTransactionLines.Include("Item").Include("Warehouse").Include("SalesTransaction").Include("SalesTransaction.Customer").Where(condition).ToList();
        }
    }
}
