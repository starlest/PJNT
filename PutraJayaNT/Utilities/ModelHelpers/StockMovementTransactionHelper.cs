namespace ECERP.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models;
    using Models.Inventory;
    using Models.StockCorrection;

    public static class StockMovementTransactionHelper
    {
        public static void AddStockMovementTransactionToDatabase(StockMovementTransaction stockMovementTransaction)
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                var fromWarehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(stockMovementTransaction.FromWarehouse.ID));
                var toWarehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(stockMovementTransaction.ToWarehouse.ID));
                var stockMovementTransactionLines = stockMovementTransaction.StockMovementTransactionLines.ToList();

                foreach (var line in stockMovementTransactionLines)
                {
                    var itemFromDatabase = context.Inventory.Single(e => e.ItemID.Equals(line.Item.ItemID));
                    line.Item = context.Inventory.Single(e => e.ItemID.Equals(line.Item.ItemID));

                    // Adjust the stock for the affected warehouses
                    var fromStock = context.Stocks.Single(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(fromWarehouse.ID));
                    var toStock = context.Stocks.SingleOrDefault(e => e.ItemID.Equals(line.Item.ItemID) && e.WarehouseID.Equals(toWarehouse.ID));

                    fromStock.Pieces -= line.Quantity;
                    if (fromStock.Pieces == 0) context.Stocks.Remove(fromStock);

                    if (toStock == null)
                    {
                        var newStock = new Stock
                        {
                            Item = itemFromDatabase,
                            Warehouse = toWarehouse,
                            Pieces = line.Quantity
                        };

                        context.Stocks.Add(newStock);
                    }
                    else toStock.Pieces += line.Quantity;
                }

                stockMovementTransaction.FromWarehouse = fromWarehouse;
                stockMovementTransaction.ToWarehouse = toWarehouse;
                var user = Application.Current.FindResource("CurrentUser") as User;
                stockMovementTransaction.User = context.Users.Single(e => e.Username.Equals(user.Username));
                context.StockMovementTransactions.Add(stockMovementTransaction);

                context.SaveChanges();
                ts.Complete();
            }
        }
    }
}
