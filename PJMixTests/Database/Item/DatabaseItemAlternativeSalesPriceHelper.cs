namespace PJMixTests.Database.Item
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ECRP.Models.Inventory;
    using ECRP.Utilities;

    public static class DatabaseItemAlternativeSalesPriceHelper
    {
        public static IEnumerable<AlternativeSalesPrice> GetAll()
        {
            using (var context = UtilityMethods.createContext())
                return context.AlternativeSalesPrices.OrderBy(alternativeSalesPrice => alternativeSalesPrice.Name).ToList();
        }

        public static IEnumerable<AlternativeSalesPrice> Get(Func<AlternativeSalesPrice, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.AlternativeSalesPrices.Include("Item").Where(condition).OrderBy(alternativeSalesPrice => alternativeSalesPrice.Name).ToList();
        }

        public static AlternativeSalesPrice FirstOrDefault(Func<AlternativeSalesPrice, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.AlternativeSalesPrices.Include("Item").Where(condition).OrderBy(alternativeSalesPrice => alternativeSalesPrice.Name).FirstOrDefault();
        }
    }
}
