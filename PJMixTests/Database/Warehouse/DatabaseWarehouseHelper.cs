﻿namespace PJMixTests.Database.Warehouse
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECERP.Models.Inventory;
    using ECERP.Utilities;

    public static class DatabaseWarehouseHelper
    {
        public static IEnumerable<Warehouse> GetAll()
        {
            using (var context = UtilityMethods.createContext())
                return context.Warehouses.OrderBy(warehouse => warehouse.Name).ToList();
        }

        public static IEnumerable<Warehouse> Get(Func<Warehouse, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Warehouses.Where(condition).OrderBy(warehouse => warehouse.Name).ToList();
        }

        public static Warehouse FirstOrDefault(Func<Warehouse, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Warehouses.Where(condition).OrderBy(warehouse => warehouse.Name).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref Warehouse warehouseToBeAttached)
        {
            var warehouseID = warehouseToBeAttached.ID;
            warehouseToBeAttached = context.Warehouses.First(warehouse => warehouse.ID.Equals(warehouseID));
        }
    }
}
