﻿namespace PutraJayaNT.Utilities.Database.Item
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Inventory;
    using Utilities;

    public static class DatabaseItemAlternativeSalesPriceHelper
    {
        public static IEnumerable<AlternativeSalesPrice> GetAll()
        {
            using (var context = new ERPContext())
                return
                    context.AlternativeSalesPrices.OrderBy(alternativeSalesPrice => alternativeSalesPrice.Name).ToList();
        }

        public static IEnumerable<AlternativeSalesPrice> Get(Func<AlternativeSalesPrice, bool> condition)
        {
            using (var context = new ERPContext())
                return
                    context.AlternativeSalesPrices.Include("Item")
                        .Where(condition)
                        .OrderBy(alternativeSalesPrice => alternativeSalesPrice.Name)
                        .ToList();
        }

        public static AlternativeSalesPrice FirstOrDefault(Func<AlternativeSalesPrice, bool> condition)
        {
            using (var context = new ERPContext())
                return
                    context.AlternativeSalesPrices.Include("Item")
                        .Where(condition)
                        .OrderBy(alternativeSalesPrice => alternativeSalesPrice.Name)
                        .FirstOrDefault();
        }
    }
}