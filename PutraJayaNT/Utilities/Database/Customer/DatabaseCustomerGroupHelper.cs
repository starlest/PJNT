using System;
using System.Collections.Generic;
using System.Linq;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Customer;

namespace PutraJayaNT.Utilities.Database.Customer
{
    public static class DatabaseCustomerGroupHelper
    {
        public static IEnumerable<CustomerGroup> GetAll()
        {
            using (var context = new ERPContext())
                return context.CustomerGroups.OrderBy(group => group.Name).ToList();
        }

        public static CustomerGroup Get(Func<CustomerGroup, bool> condition)
        {
            using (var context = new ERPContext())
                return context.CustomerGroups.Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref CustomerGroup customerGroupToBeAttached)
        {
            var customerGroupID = customerGroupToBeAttached.ID;
            customerGroupToBeAttached = context.CustomerGroups.First(customerGroup => customerGroup.ID.Equals(customerGroupID));
        }
    }
}
