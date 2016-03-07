using System;
using System.Collections.Generic;
using System.Linq;
using PutraJayaNT.Models;

namespace PutraJayaNT.Utilities.Database
{
    public static class DatabaseSupplierHelper
    {
        public static IEnumerable<Supplier> GetAll()
        {
            using (var context = new ERPContext())
                return context.Suppliers.Include("Items").Where(supplier => !supplier.Name.Equals("-")).OrderBy(supplier => supplier.Name).ToList();
        }

        public static IEnumerable<Supplier> Get(Func<Supplier, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Suppliers.Include("Items").Where(condition).Where(supplier => !supplier.Name.Equals("-")).OrderBy(supplier => supplier.Name).ToList();
        }

        public static Supplier FirstOrDefault(Func<Supplier, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Suppliers.Include("Items").Where(condition).FirstOrDefault();
        }


        public static void AttachToDatabaseContext(ERPContext context, ref Supplier supplierToBeAttached)
        {
            var supplierID = supplierToBeAttached.ID;
            supplierToBeAttached = context.Suppliers.First(supplier => supplier.ID.Equals(supplierID));
        }
    }
}
