namespace PutraJayaNT.Utilities.Database.Salesman
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Salesman;

    public static class DatabaseSalesmanHelper
    {
        public static IEnumerable<Salesman> GetAll()
        {
            using (var context = new ERPContext())
                return context.Salesmans.Include("SalesCommissions").Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name).ToList();
        }

        public static IEnumerable<Salesman> Get(Func<Salesman, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Salesmans.Include("SalesCommissions").Where(condition).Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name).ToList();
        }

        public static Salesman FirstOrDefault(Func<Salesman, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Salesmans.Include("SalesCommissions").Where(condition).FirstOrDefault();
        }


        public static void AttachToDatabaseContext(ERPContext context, ref Salesman salesmanToBeAttached)
        {
            var salesmanID = salesmanToBeAttached.ID;
            salesmanToBeAttached = context.Salesmans.First(salesman => salesman.ID.Equals(salesmanID));
        }
    }
}
