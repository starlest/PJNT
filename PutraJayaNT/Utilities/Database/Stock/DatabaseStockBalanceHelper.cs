namespace PutraJayaNT.Utilities.Database.Stock
{
    using System;
    using System.Linq;
    using Models.Inventory;

    public static class DatabaseStockBalanceHelper
    {
        public static StockBalance FirstOrDefault(Func<StockBalance, bool> condition)
        {
            using (var context = new ERPContext())
                return context.StockBalances.Where(condition).OrderBy(stock => stock.Item.Name).ThenBy(stock => stock.Warehouse.Name).FirstOrDefault();
        }

    }
}
