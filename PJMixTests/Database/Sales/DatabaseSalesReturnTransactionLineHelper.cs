﻿namespace PJMixTests.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.Sales;
    using ECRP.Utilities;

    public static class DatabaseSalesReturnTransactionLineHelper
    {
        public static IEnumerable<SalesReturnTransactionLine> Get(Func<SalesReturnTransactionLine, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesReturnTransactionLines
                    .Include("Item")
                    .Include("Warehouse")
                    .Include("SalesReturnTransaction")
                    .Include("SalesReturnTransaction.SalesTransaction.Customer")
                    .Where(condition).ToList();
        }
    }
}
