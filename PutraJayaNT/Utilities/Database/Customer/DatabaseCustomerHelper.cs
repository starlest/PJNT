namespace PutraJayaNT.Utilities.Database.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DatabaseCustomerHelper
    {
        public static IEnumerable<Models.Customer.Customer> GetAll()
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").OrderBy(customer => customer.Name).ToList();
        }

        public static IEnumerable<Models.Customer.Customer> Get(Func<Models.Customer.Customer, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").Where(condition).OrderBy(customer => customer.Name).ToList();
        }

        public static Models.Customer.Customer FirstOrDefault(Func<Models.Customer.Customer, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref Models.Customer.Customer customerToBeAttached)
        {
            var customerID = customerToBeAttached.ID;
            var customerGroupID = customerToBeAttached.Group.ID;
            customerToBeAttached = context.Customers.First(customer => customer.ID.Equals(customerID));
            customerToBeAttached.Group = context.CustomerGroups.First(customerGroup => customerGroup.ID.Equals(customerGroupID));
        }
    }
}
