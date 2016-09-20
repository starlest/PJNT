namespace PJMixTests.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.Sales;
    using ECRP.Utilities;

    public static class DatabaseSalesTransactionLineHelper
    {
        public static IEnumerable<SalesTransactionLine> Get(Func<SalesTransactionLine, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesTransactionLines.Include("Item").Include("Warehouse").Include("SalesTransaction").Include("SalesTransaction.Customer").Where(condition).ToList();
        }
    }
}
