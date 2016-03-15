namespace PutraJayaNT.Utilities
{
    using System.Linq;
    using Models.Sales;

    public static class SalesTransactionHelper
    {
        public static void AddTransactionToDatabase(SalesTransaction salesTransaction)
        {
            using (var context = new ERPContext())
            {
                foreach (var line in salesTransaction.SalesTransactionLines.ToList())
                {
                    AttachSalesTransactionLineToDatabaseContext(context, line);
                    ReduceLineItemStockInDatabaseContext(context, line);
                }

                AttachSalesTransactionPropertiesToDatabaseContext(context, salesTransaction);
                context.SalesTransactions.Add(salesTransaction);
                context.SaveChanges();
            }
        }

        #region Add Transaction Helper Methods
        private static void AttachSalesTransactionLineToDatabaseContext(ERPContext context, SalesTransactionLine line)
        {
            line.Item = context.Inventory.Single(item => item.ItemID.Equals(line.Item.ItemID));
            line.Warehouse = context.Warehouses.Single(warehouse => warehouse.ID == line.Warehouse.ID);
            line.Salesman = context.Salesmans.Single(salesman => salesman.ID.Equals(line.Salesman.ID));
        }

        private static void ReduceLineItemStockInDatabaseContext(ERPContext context, SalesTransactionLine line)
        {
            var stockFromDatabase = context.Stocks.Single(
                stock => stock.Item.ItemID.Equals(line.Item.ItemID) && stock.WarehouseID == line.Warehouse.ID);
            stockFromDatabase.Pieces -= line.Quantity;
            if (stockFromDatabase.Pieces == 0) context.Stocks.Remove(stockFromDatabase);
        }

        private static void AttachSalesTransactionPropertiesToDatabaseContext(ERPContext context, SalesTransaction salesTransaction)
        {
            salesTransaction.Customer = context.Customers.Single(customer => customer.ID.Equals(salesTransaction.Customer.ID));
            salesTransaction.User = context.Users.Single(user => user.Username.Equals(salesTransaction.User.Username));
            salesTransaction.CollectionSalesman = context.Salesmans.Single(salesman => salesman.Name.Equals(" "));
        }
        #endregion
    }
}
