namespace PJMixTests.Database.Item
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.Inventory;
    using ECRP.Utilities;

    public static class DatabaseItemHelper
    {
        public static IEnumerable<Item> GetAll()
        {
            using (var context = UtilityMethods.createContext())
                return context.Inventory.Include("Category").Include("Suppliers").Include("AlternativeSalesPrices").OrderBy(item => item.Name).ToList();
        }

        public static IEnumerable<Item> GetAllActive()
        {
            using (var context = UtilityMethods.createContext())
                return context.Inventory.Include("Category").Include("Suppliers").Include("AlternativeSalesPrices").Where(item => item.Active.Equals(true)).OrderBy(item => item.Name).ToList();
        }

        public static IEnumerable<Item> Get(Func<Item, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Inventory.Include("Category").Include("Suppliers").Include("AlternativeSalesPrices").Where(condition).OrderBy(item => item.Name).ToList();
        }

        public static Item FirstOrDefault(Func<Item, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.Inventory.Include("Category").Include("Suppliers").Include("AlternativeSalesPrices").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref Item itemToBeAttached)
        {
            var itemID = itemToBeAttached.ItemID;
            itemToBeAttached = context.Inventory.First(item => item.ItemID.Equals(itemID));
        }
    }
}
