using PutraJayaNT.Models.Salesman;

namespace PutraJayaNT.Utilities.Database.Salesman
{
    using System;
    using System.Linq;

    public static class DatabaseSalesmanCommisionHelper
    {
        //public static IEnumerable<Salesman> GetAll()
        //{
        //    using (var context = new ERPContext())
        //        return context.Salesmans.Include("SalesCommissions").OrderBy(salesman => salesman.Name).ToList();
        //}

        //public static IEnumerable<Salesman> Get(Func<Salesman, bool> condition)
        //{
        //    using (var context = new ERPContext())
        //        return context.Salesmans.Include("SalesCommissions").Where(condition).OrderBy(salesman => salesman.Name).ToList();
        //}

        public static SalesCommission FirstOrDefault(Func<SalesCommission, bool> condition)
        {
            using (var context = new ERPContext())
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
