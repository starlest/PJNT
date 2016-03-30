namespace PJMixTests.Database.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Customer;
    using PutraJayaNT.Utilities;

    public static class DatabaseCustomerGroupHelper
    {
        public static IEnumerable<CustomerGroup> GetAll()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.CustomerGroups.OrderBy(group => group.Name).ToList();
        }

        public static CustomerGroup Get(Func<CustomerGroup, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.CustomerGroups.Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref CustomerGroup customerGroupToBeAttached)
        {
            var customerGroupID = customerGroupToBeAttached.ID;
            customerGroupToBeAttached = context.CustomerGroups.First(customerGroup => customerGroup.ID.Equals(customerGroupID));
        }
    }
}
