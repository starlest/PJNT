namespace PJMixTests.Database.Salesman
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Salesman;
    using PutraJayaNT.Utilities;

    public static class DatabaseSalesmanHelper
    {
        public static IEnumerable<Salesman> GetAll()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Salesmans.Include("SalesCommissions").Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name).ToList();
        }

        public static IEnumerable<Salesman> GetAllIncludingEmptySalesman()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Salesmans.Include("SalesCommissions").OrderBy(salesman => salesman.Name).ToList();
        }

        public static IEnumerable<Salesman> Get(Func<Salesman, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Salesmans.Include("SalesCommissions").Where(condition).Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name).ToList();
        }

        public static Salesman FirstOrDefault(Func<Salesman, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Salesmans.Include("SalesCommissions").Where(condition).FirstOrDefault();
        }


        public static void AttachToDatabaseContext(ERPContext context, ref Salesman salesmanToBeAttached)
        {
            var salesmanID = salesmanToBeAttached.ID;
            salesmanToBeAttached = context.Salesmans.First(salesman => salesman.ID.Equals(salesmanID));
        }
    }
}
