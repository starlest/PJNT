namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models;
    using Models.Accounting;
    using Models.Inventory;
    using Models.Sales;

    public static class SalesReturnTransactionHelper
    {
        public static bool IsLastSaveSuccessful;

        public static void AddSalesReturnTransactionToDatabase(SalesReturnTransaction salesReturnTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                var context = new ERPContext(UtilityMethods.GetDBName());

                AttachSalesReturnTransactionPropertiesToDatabaseContext(context, ref salesReturnTransaction);
                salesReturnTransaction.SalesTransaction.Customer.SalesReturnCredits += salesReturnTransaction.NetTotal;

                var lines = salesReturnTransaction.SalesReturnTransactionLines.ToList();
                salesReturnTransaction.SalesReturnTransactionLines.Clear();
                foreach (var salesReturnTransactionLine in lines)
                {
                    salesReturnTransactionLine.SalesReturnTransaction = salesReturnTransaction;
                    AddSalesReturnTransactionLineToDatabaseContext(context, salesReturnTransactionLine);
                    DecreaseSalesReturnTransactionLineItemSoldOrReturnedInDatabaseContext(context, salesReturnTransactionLine);
                    InceaseSalesReturnTransactionLineItemStockInDatabaseContext(context, salesReturnTransactionLine);
                    context.SaveChanges();
                }

                AddSalesReturnTransactionLedgerTransactionsToDatabaseContext(context, salesReturnTransaction);
                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        #region Add Helper Methods
        private static void AttachSalesReturnTransactionPropertiesToDatabaseContext(ERPContext context,
            ref SalesReturnTransaction salesReturnTransaction)
        {
            var user = Application.Current.FindResource("CurrentUser") as User;
            salesReturnTransaction.User = context.Users.FirstOrDefault(e => e.Username.Equals(user.Username));
            context.SalesTransactions.Attach(salesReturnTransaction.SalesTransaction);
            context.Customers.Attach(salesReturnTransaction.SalesTransaction.Customer);
        }

        private static void AddSalesReturnTransactionLineToDatabaseContext(ERPContext context, SalesReturnTransactionLine salesReturnTransactionLine)
        {
            var item = context.Inventory.First(e => e.ItemID.Equals(salesReturnTransactionLine.Item.ItemID));
            var warehouse = context.Warehouses.First(e => e.ID.Equals(salesReturnTransactionLine.Warehouse.ID));
            salesReturnTransactionLine.Item = item;
            salesReturnTransactionLine.Warehouse = warehouse;
            context.SalesReturnTransactionLines.Add(salesReturnTransactionLine);
        }

        private static void DecreaseSalesReturnTransactionLineItemSoldOrReturnedInDatabaseContext(ERPContext context, SalesReturnTransactionLine salesReturnTransactionLine)
        {
            var purchases = context.PurchaseTransactionLines
                .Where(e => e.ItemID.Equals(salesReturnTransactionLine.Item.ItemID) && e.SoldOrReturned > 0)
                .OrderByDescending(transaction => transaction.PurchaseTransactionID)
                .ThenByDescending(transaction => transaction.Quantity - transaction.SoldOrReturned)
                .ThenByDescending(transaction => transaction.PurchasePrice)
                .ThenByDescending(transaction => transaction.Discount)
                .ThenByDescending(transaction => transaction.WarehouseID);

            var tracker = salesReturnTransactionLine.Quantity;
            foreach (var purchase in purchases)
            {
                if (purchase.SoldOrReturned >= tracker)
                {
                    purchase.SoldOrReturned -= tracker;
                    break;
                }
                if (purchase.SoldOrReturned < tracker)
                {
                    tracker -= purchase.SoldOrReturned;
                    purchase.SoldOrReturned = 0;
                }
            }
            context.SaveChanges();
        }

        private static void InceaseSalesReturnTransactionLineItemStockInDatabaseContext(ERPContext context, SalesReturnTransactionLine salesReturnTransactionLine)
        {
            var stockFromDatabase = context.Stocks
                .SingleOrDefault(stock => stock.Item.ItemID.Equals(salesReturnTransactionLine.Item.ItemID) && stock.Warehouse.ID.Equals(salesReturnTransactionLine.Warehouse.ID));
            if (stockFromDatabase != null) stockFromDatabase.Pieces += salesReturnTransactionLine.Quantity;
            else
            {
                var s = new Stock
                {
                    Item = salesReturnTransactionLine.Item,
                    Warehouse = salesReturnTransactionLine.Warehouse,
                    Pieces = salesReturnTransactionLine.Quantity
                };
                context.Stocks.Add(s);
            }
        }

        private static void AddSalesReturnTransactionLedgerTransactionsToDatabaseContext(ERPContext context, SalesReturnTransaction salesReturnTransaction)
        {
            var totalCOGS = salesReturnTransaction.SalesReturnTransactionLines.ToList().Sum(salesReturnTransactionLine => salesReturnTransactionLine.CostOfGoodsSold);

            // Record the corresponding ledger transactions in the database
            var ledgerTransaction1 = new LedgerTransaction();
            var ledgerTransaction2 = new LedgerTransaction();

            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, ledgerTransaction1, UtilityMethods.GetCurrentDate().Date, salesReturnTransaction.SalesReturnTransactionID, "Sales Return")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, ledgerTransaction1, "Sales Returns and Allowances", "Debit", salesReturnTransaction.NetTotal);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, ledgerTransaction1,
                $"{salesReturnTransaction.SalesTransaction.Customer.Name} Accounts Receivable", "Credit", salesReturnTransaction.NetTotal);

            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, ledgerTransaction2, UtilityMethods.GetCurrentDate().Date, salesReturnTransaction.SalesReturnTransactionID, "Sales Return")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, ledgerTransaction2, "Inventory", "Debit", totalCOGS);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, ledgerTransaction2, "Cost of Goods Sold", "Credit", totalCOGS);

            context.SaveChanges();
        }
        #endregion
    }
}
