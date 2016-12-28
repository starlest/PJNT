namespace PJMixTests.Database.Stock
{
    using System;
    using System.Linq;
    using ECERP.Models.Inventory;
    using ECERP.Utilities;

    public static class DatabaseStockBalanceHelper
    {
        public static StockBalance FirstOrDefault(Func<StockBalance, bool> condition)
        {
            using (var context = UtilityMethods.createContext())
                return context.StockBalances.Where(condition).OrderBy(stock => stock.Item.Name).ThenBy(stock => stock.Warehouse.Name).FirstOrDefault();
        }

    }
}
