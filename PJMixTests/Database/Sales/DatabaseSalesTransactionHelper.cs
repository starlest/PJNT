namespace PJMixTests.Database.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Sales;
    using PutraJayaNT.Utilities;

    public static class DatabaseSalesTransactionHelper
    {
        public static IEnumerable<SalesTransaction> GetAll()
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Include("SalesTransactionLines").ToList();
        }

        public static IEnumerable<SalesTransaction> Get(Func<SalesTransaction, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesTransactions.Include("Customer").Include("User").Include("Customer.Group").Include("CollectionSalesman").Include("SalesTransactionLines").Where(condition).ToList();
        }

        public static SalesTransaction FirstOrDefault(Func<SalesTransaction, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group")
                    .Include("CollectionSalesman").Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Item").Include("SalesTransactionLines.Warehouse")
                    .Where(condition).FirstOrDefault();
        }

        public static void AttachToDatabaseContext(ERPContext context, ref SalesTransaction salestransactionToBeAttached)
        {
            var salestransactionID = salestransactionToBeAttached.SalesTransactionID;
            salestransactionToBeAttached = context.SalesTransactions.First(salesTransaction => salesTransaction.SalesTransactionID.Equals(salestransactionID));
        }

        public static IEnumerable<SalesTransaction> GetWithoutLines(Func<SalesTransaction, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesTransactions.Include("Customer").Include("User").Include("Customer.Group").Include("CollectionSalesman").Where(condition).ToList();
        }

        public static SalesTransaction FirstOrDefaultWithoutLines(Func<SalesTransaction, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesTransactions.Include("Customer").Include("Customer.Group").Include("CollectionSalesman").Where(condition).FirstOrDefault();
        }
    }
}
