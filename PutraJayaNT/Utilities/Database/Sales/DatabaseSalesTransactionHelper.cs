namespace PutraJayaNT.Utilities.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Sales;

    public static class DatabaseSalesTransactionHelper
    {
        public static IEnumerable<SalesTransaction> GetAll()
        {
            using (var context = new ERPContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Include("SalesTransactionLines").ToList();
        }

        public static IEnumerable<SalesTransaction> Get(Func<SalesTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Include("SalesTransactionLines").Where(condition).ToList();
        }

        public static SalesTransaction FirstOrDefault(Func<SalesTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Include("SalesTransactionLines").Where(condition).FirstOrDefault();
        }

        public static void AttachToDatabaseContext(ERPContext context, ref Models.Salesman.Salesman salesmanToBeAttached)
        {
            var salesmanID = salesmanToBeAttached.ID;
            salesmanToBeAttached = context.Salesmans.First(salesman => salesman.ID.Equals(salesmanID));
        }

        public static IEnumerable<SalesTransaction> GetWithoutLines(Func<SalesTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Where(condition).ToList();
        }

        public static SalesTransaction FirstOrDefaultWithoutLines(Func<SalesTransaction, bool> condition)
        {
            using (var context = new ERPContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Where(condition).FirstOrDefault();
        }
    }
}
