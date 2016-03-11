namespace PutraJayaNT.Utilities.Database.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Customer;
    using Utilities;

    public static class DatabaseCustomerHelper
    {
        public static IEnumerable<Customer> GetAll()
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").OrderBy(customer => customer.Name).ToList();
        }

        public static IEnumerable<Customer> Get(Func<Customer, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").Where(condition).OrderBy(customer => customer.Name).ToList();
        }

        public static Customer FirstOrDefault(Func<Customer, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref Customer customerToBeAttached)
        {
            var customerID = customerToBeAttached.ID;
            var customerGroupID = customerToBeAttached.Group.ID;
            customerToBeAttached = context.Customers.First(customer => customer.ID.Equals(customerID));
            customerToBeAttached.Group =
                context.CustomerGroups.First(customerGroup => customerGroup.ID.Equals(customerGroupID));
        }
    }
}