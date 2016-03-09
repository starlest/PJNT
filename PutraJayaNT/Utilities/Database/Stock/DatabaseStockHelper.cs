namespace PutraJayaNT.Utilities.Database.Stock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Inventory;

    public static class DatabaseStockHelper
    {
        public static IEnumerable<Stock> GetAll()
        {
            using (var context = new ERPContext())
                return context.Stocks.Include("Item").Include("Warehouse").OrderBy(stock => stock.Item.Name).ThenBy(stock => stock.Warehouse.Name).ToList();
        }

        public static IEnumerable<Stock> Get(Func<Stock, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Stocks.Include("Item").Include("Warehouse").Where(condition).OrderBy(stock => stock.Item.Name).ThenBy(stock => stock.Warehouse.Name).ToList();
        }

        public static Models.Inventory.Item FirstOrDefault(Func<Models.Inventory.Item, bool> condition)
        {
            using (var context = new ERPContext())
                return context.Inventory.Include("Category").Include("Suppliers").Include("AlternativeSalesPrices").Where(condition).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref Models.Inventory.Item itemToBeAttached)
        {
            var itemID = itemToBeAttached.ItemID;
            itemToBeAttached = context.Inventory.First(item => item.ItemID.Equals(itemID));
        }
    }
}
