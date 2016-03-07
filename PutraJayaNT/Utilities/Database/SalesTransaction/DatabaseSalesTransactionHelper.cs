using System;
using System.Collections.Generic;
using System.Linq;

namespace PutraJayaNT.Utilities.Database.SalesTransaction
{
    public static class DatabaseSalesTransactionHelper
    {
        public static IEnumerable<Models.Sales.SalesTransaction> GetAll()
        {
            using (var context = new ERPContext())
                return context.SalesTransactions.Include("SalesCommissions").ToList();
        }

        public static IEnumerable<Models.Salesman.Salesman> Get(Func<Models.Salesman.Salesman, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Salesmans.Include("SalesCommissions").Where(condition).Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name).ToList();
        }

        public static Models.Salesman.Salesman FirstOrDefault(Func<Models.Salesman.Salesman, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Salesmans.Include("SalesCommissions").Where(condition).FirstOrDefault();
        }


        public static void AttachToDatabaseContext(ERPContext context, ref Models.Salesman.Salesman salesmanToBeAttached)
        {
            var salesmanID = salesmanToBeAttached.ID;
            salesmanToBeAttached = context.Salesmans.First(salesman => salesman.ID.Equals(salesmanID));
        }
    }
}
