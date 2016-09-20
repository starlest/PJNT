namespace PJMixTests.Database.Salesman
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.Salesman;
    using ECRP.Utilities;

    public static class DatabaseSalesmanCommisionHelper
    {
        //public static IEnumerable<Salesman> GetAll()
        //{
        //    using (var context = UtilityMethods.createContext())
        //        return context.Salesmans.Include("SalesCommissions").OrderBy(salesman => salesman.Name).ToList();
        //}

        public static IEnumerable<SalesCommission> Get(Func<SalesCommission, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesCommissions.Include("Salesman").Include("Category").Where(condition).OrderBy(salesCommision => salesCommision.Salesman.Name).ToList();
        }

        public static SalesCommission FirstOrDefault(Func<SalesCommission, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.SalesCommissions.Include("Salesman").Include("Category").Where(condition).FirstOrDefault();
        }


        public static void AttachToDatabaseContext(ERPContext context, ref SalesCommission salesCommissionToBeAttached)
        {
            var salesmanID = salesCommissionToBeAttached.Salesman.ID;
            var categoryID = salesCommissionToBeAttached.Category.ID;
            salesCommissionToBeAttached = context.SalesCommissions.FirstOrDefault(salesCommision => salesCommision.Salesman_ID.Equals(salesmanID) && salesCommision.Category_ID.Equals(categoryID));
        }
    }
}
