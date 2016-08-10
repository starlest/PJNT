namespace PJMixTests.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Sales;
    using PutraJayaNT.Utilities;

    public static class DatabaseSalesTransactionLineHelper
    {
        public static IEnumerable<SalesTransactionLine> Get(Func<SalesTransactionLine, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                return context.SalesTransactionLines.Include("Item").Include("Warehouse").Include("SalesTransaction").Include("SalesTransaction.Customer").Where(condition).ToList();
        }
    }
}
