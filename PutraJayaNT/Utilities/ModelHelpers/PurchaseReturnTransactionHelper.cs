namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models;
    using Models.Accounting;
    using Models.Purchase;

    public static class PurchaseReturnTransactionHelper
    {
        public static bool IsLastSaveSuccessful;

        public static void AddPurchaseReturnTransactionToDatabase(PurchaseReturnTransaction purchaseReturnTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                AttachPurchaseReturnTransactionPropertiesToDatabaseContext(context, ref purchaseReturnTransaction);
                purchaseReturnTransaction.PurchaseTransaction.Supplier.PurchaseReturnCredits +=
                    purchaseReturnTransaction.NetTotal;

                decimal totalCOGS = 0;
                var lines = purchaseReturnTransaction.PurchaseReturnTransactionLines.ToList();
                purchaseReturnTransaction.PurchaseReturnTransactionLines.Clear();
                foreach (var purchaseReturnTransactionLine in lines)
                {
                    if (!IsThereEnoughLineItemStockInDatabaseContext(context, purchaseReturnTransactionLine)) return;
                    purchaseReturnTransactionLine.PurchaseReturnTransaction = purchaseReturnTransaction;
                    AddPurchaseReturnTransactionLineToDatabaseContext(context, purchaseReturnTransactionLine);
                    IncreasePurchaseReturnTransactionLineItemSoldOrReturnedInDatabaseContext(context, purchaseReturnTransactionLine);
                    DecreasePurchaseReturnTransactionLineItemStockInDatabaseContext(context, purchaseReturnTransactionLine);
                    totalCOGS += CalculateLineCOGSFromDatabaseContext(context, purchaseReturnTransactionLine);
                    context.SaveChanges();
                }

                AddPurchaseReturnTransactionLedgerTransactionToDatabaseContext(context, purchaseReturnTransaction, totalCOGS);
                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        private static decimal CalculateLineCOGSFromDatabaseContext(ERPContext context, PurchaseReturnTransactionLine purchaseReturnTransactionLine)
        {
            var purchaseTransactionLine = context.PurchaseTransactionLines
            .Single(
                line => line.PurchaseTransactionID.Equals(purchaseReturnTransactionLine.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID) &&
                line.ItemID.Equals(purchaseReturnTransactionLine.Item.ItemID) && line.WarehouseID.Equals(purchaseReturnTransactionLine.Warehouse.ID) &&
                line.PurchasePrice.Equals(purchaseReturnTransactionLine.PurchasePrice) && line.Discount.Equals(purchaseReturnTransactionLine.Discount));

            var purchaseLineNetTotal = purchaseTransactionLine.PurchasePrice - purchaseTransactionLine.Discount;
            if (purchaseLineNetTotal == 0) return 0;
            var fractionOfTransactionDiscount = purchaseReturnTransactionLine.Quantity * purchaseLineNetTotal / 
                purchaseTransactionLine.PurchaseTransaction.GrossTotal * purchaseTransactionLine.PurchaseTransaction.Discount;
            var fractionOfTransactionTax = purchaseReturnTransactionLine.Quantity * purchaseLineNetTotal / 
                purchaseTransactionLine.PurchaseTransaction.GrossTotal * purchaseTransactionLine.PurchaseTransaction.Tax;
            return purchaseReturnTransactionLine.Quantity * purchaseLineNetTotal - fractionOfTransactionDiscount + fractionOfTransactionTax;
        }

        private static void AttachPurchaseReturnTransactionPropertiesToDatabaseContext(ERPContext context, ref PurchaseReturnTransaction purchaseReturnTransaction)
        {
            var user = Application.Current.FindResource("CurrentUser") as User;
            purchaseReturnTransaction.User = context.Users.FirstOrDefault(e => e.Username.Equals(user.Username));
            context.PurchaseTransactions.Attach(purchaseReturnTransaction.PurchaseTransaction);
            context.Suppliers.Attach(purchaseReturnTransaction.PurchaseTransaction.Supplier);
        }

        private static bool IsThereEnoughLineItemStockInDatabaseContext(ERPContext context, PurchaseReturnTransactionLine purchaseReturnTransactionLine)
        {
            var stockFromDatabase = context.Stocks.SingleOrDefault(
                stock => stock.Item.ItemID.Equals(purchaseReturnTransactionLine.Item.ItemID) &&
                stock.Warehouse.ID.Equals(purchaseReturnTransactionLine.ReturnWarehouse.ID));
            if (stockFromDatabase != null && stockFromDatabase.Pieces >= purchaseReturnTransactionLine.Quantity)
                return true;
            var availableQuantity = stockFromDatabase?.Pieces ?? 0;
            MessageBox.Show(
                $"{purchaseReturnTransactionLine.Item.Name} has only {availableQuantity / purchaseReturnTransactionLine.Item.PiecesPerUnit} units {availableQuantity % purchaseReturnTransactionLine.Item.PiecesPerUnit} pieces left.",
                "Invalid Quantity", MessageBoxButton.OK);
            return false;
        }

        private static void AddPurchaseReturnTransactionLineToDatabaseContext(ERPContext context, PurchaseReturnTransactionLine purchaseReturnTransactionLine)
        {
            purchaseReturnTransactionLine.Item = context.Inventory.Single(item => item.ItemID.Equals(purchaseReturnTransactionLine.Item.ItemID));
            purchaseReturnTransactionLine.Warehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(purchaseReturnTransactionLine.Warehouse.ID));
            purchaseReturnTransactionLine.ReturnWarehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(purchaseReturnTransactionLine.ReturnWarehouse.ID));
            context.PurchaseReturnTransactionLines.Add(purchaseReturnTransactionLine);
        }

        private static void IncreasePurchaseReturnTransactionLineItemSoldOrReturnedInDatabaseContext(ERPContext context, PurchaseReturnTransactionLine purchaseReturnTransactionLine)
        {
            var purchaseTransactionLine = context.PurchaseTransactionLines
                    .Single(
                line => line.PurchaseTransactionID.Equals(purchaseReturnTransactionLine.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID) &&
                line.ItemID.Equals(purchaseReturnTransactionLine.Item.ItemID) && line.WarehouseID.Equals(purchaseReturnTransactionLine.Warehouse.ID) && 
                line.PurchasePrice.Equals(purchaseReturnTransactionLine.PurchasePrice) && line.Discount.Equals(purchaseReturnTransactionLine.Discount));
            purchaseTransactionLine.SoldOrReturned += purchaseReturnTransactionLine.Quantity;
        }

        private static void DecreasePurchaseReturnTransactionLineItemStockInDatabaseContext(ERPContext context, PurchaseReturnTransactionLine purchaseReturnTransactionLine)
        {
            var stockFromDatabase = context.Stocks.Single(
                stock => stock.Item.ItemID.Equals(purchaseReturnTransactionLine.Item.ItemID) && 
                stock.Warehouse.ID.Equals(purchaseReturnTransactionLine.Warehouse.ID));
            stockFromDatabase.Pieces -= purchaseReturnTransactionLine.Quantity;
            if (stockFromDatabase.Pieces == 0) context.Stocks.Remove(stockFromDatabase);
        }

        private static void AddPurchaseReturnTransactionLedgerTransactionToDatabaseContext(ERPContext context, PurchaseReturnTransaction purchaseReturnTransaction, decimal totalCOGS)
        {
            var purchaseReturnLedgerTransaction = new LedgerTransaction();
            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, purchaseReturnLedgerTransaction, UtilityMethods.GetCurrentDate(), purchaseReturnTransaction.PurchaseReturnTransactionID, "Purchase Return")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseReturnLedgerTransaction,
                $"{purchaseReturnTransaction.PurchaseTransaction.Supplier.Name} Accounts Payable", "Debit", purchaseReturnTransaction.NetTotal);
            if (totalCOGS - purchaseReturnTransaction.NetTotal > 0)
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseReturnLedgerTransaction, "Cost of Goods Sold", "Debit", totalCOGS - purchaseReturnTransaction.NetTotal);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseReturnLedgerTransaction, "Inventory", "Credit", totalCOGS);
            context.SaveChanges();
        }
    }
}
