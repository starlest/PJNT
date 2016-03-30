namespace PJMixTests.Database.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Customer;
    using PutraJayaNT.Utilities;

    public static class DatabaseCustomerHelper
    {
        public static IEnumerable<Customer> GetAll()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Customers.Include("Group").OrderBy(customer => customer.Name).ToList();
        }

        public static IEnumerable<Customer> Get(Func<Customer, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Customers.Include("Group").Where(condition).OrderBy(customer => customer.Name).ToList();
        }

        public static PutraJayaNT.Models.Customer.Customer FirstOrDefault(Func<Customer, bool> condition)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Customers.Include("Group").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref PutraJayaNT.Models.Customer.Customer customerToBeAttached)
        {
            var customerID = customerToBeAttached.ID;
            var customerGroupID = customerToBeAttached.Group.ID;
            customerToBeAttached = context.Customers.First(customer => customer.ID.Equals(customerID));
            customerToBeAttached.Group = context.CustomerGroups.First(customerGroup => customerGroup.ID.Equals(customerGroupID));
        }
    }
}
