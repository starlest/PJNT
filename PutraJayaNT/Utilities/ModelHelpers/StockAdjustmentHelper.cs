namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Collections.ObjectModel;
    using System.Transactions;
    using System.Windows;
    using Models;
    using Models.Accounting;
    using Models.Inventory;
    using Models.Purchase;
    using Models.StockCorrection;
    using System.Linq;

    public class StockAdjustmentHelper
    {
        public static void AddStockAdjustmentTransactionToDatabase(StockAdjustmentTransaction stockAdjustmentTransaction)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                var stockAdjustmentPurchaseTransaction = MakeNewstockAdjustmentPurchaseTransaction(context, stockAdjustmentTransaction);

                decimal totalCOGSAdjustment = 0;

                var isThereDecreaseAdjustmentLine = false;
                var isThereIncreaseAdjustmentLine = false;

                foreach (var line in stockAdjustmentTransaction.AdjustStockTransactionLines)
                {
                    AttachLineToDatabaseContext(context, line);

                    if (line.Quantity < 0)
                    {
                        isThereDecreaseAdjustmentLine = true;
                        DecreaseStock(context, line.Warehouse, line.Item, line.Quantity);
                        totalCOGSAdjustment += CalculateCOGS(context, line.Item, -line.Quantity);
                        IncreaseSoldOrReturned(context, line.Item, -line.Quantity);
                    }

                    else
                    {
                        isThereIncreaseAdjustmentLine = true;
                        IncreaseStock(context, line.Warehouse, line.Item, line.Quantity);
                        AddLineToStockAdjustmentPurchaseTransaction(line, stockAdjustmentPurchaseTransaction);
                    }
                }

                if (isThereDecreaseAdjustmentLine)
                    AddStockAdjustmentDecrementLedgerTransactionToDatabase(context, stockAdjustmentTransaction, totalCOGSAdjustment);

                if (isThereIncreaseAdjustmentLine)
                    context.PurchaseTransactions.Add(stockAdjustmentPurchaseTransaction);

                AddStockAdjustmentTransactionToDatabaseContext(context, stockAdjustmentTransaction);
                context.SaveChanges();
                ts.Complete();
            }
        }

        #region Helper Methods
        private static PurchaseTransaction MakeNewstockAdjustmentPurchaseTransaction(ERPContext context, StockAdjustmentTransaction stockAdjustmentTransaction)
        {
            return new PurchaseTransaction
            {
                PurchaseID = stockAdjustmentTransaction.StockAdjustmentTransactionID,
                Supplier = context.Suppliers.Single(e => e.Name.Equals("-")),
                Date = UtilityMethods.GetCurrentDate().Date,
                DueDate = UtilityMethods.GetCurrentDate().Date,
                Discount = 0,
                GrossTotal = 0,
                Total = 0,
                Paid = 0,
                User = stockAdjustmentTransaction.User,
                PurchaseTransactionLines = new ObservableCollection<PurchaseTransactionLine>()
            };
        }

        private static void AttachLineToDatabaseContext(ERPContext context, StockAdjustmentTransactionLine line)
        {
            line.Item = context.Inventory.Single(e => e.ItemID.Equals(line.Item.ItemID));
            line.Warehouse = context.Warehouses.Single(e => e.ID.Equals(line.Warehouse.ID));
        }

        private static void DecreaseStock(ERPContext context, Warehouse warehouse, Item item, int quantity)
        {
            var stock = context.Stocks.Single(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));
            stock.Pieces += quantity;
            if (stock.Pieces == 0) context.Stocks.Remove(stock);
        }

        private static decimal CalculateCOGS(ERPContext context, Item item, int quantity)
        {
            var purchases = context.PurchaseTransactionLines
            .Include("PurchaseTransaction")
            .Where(e => e.ItemID.Equals(item.ItemID) && e.SoldOrReturned < e.Quantity)
            .OrderBy(purchaseTransactionLine => purchaseTransactionLine.PurchaseTransactionID)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.Quantity - purchaseTransactionLine.SoldOrReturned)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.PurchasePrice)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.Discount)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.WarehouseID)
            .ToList();

            var totalCOGS = 0m;
            var tracker = quantity;

            foreach (var purchase in purchases)
            {
                var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;

                if (tracker <= availableQuantity)
                {
                    if (purchaseLineNetTotal == 0) break;
                    var fractionOfTransactionDiscount = tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal * purchase.PurchaseTransaction.Discount;
                    var fractionOfTransactionTax = tracker * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal * purchase.PurchaseTransaction.Tax;
                    totalCOGS += tracker * purchaseLineNetTotal - fractionOfTransactionDiscount + fractionOfTransactionTax;
                    break;
                }

                if (tracker > availableQuantity)
                {
                    tracker -= availableQuantity;
                    if (purchaseLineNetTotal == 0) continue;
                    var fractionOfTransactionDiscount = availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal * purchase.PurchaseTransaction.Discount;
                    var fractionOfTransactionTax = availableQuantity * purchaseLineNetTotal / purchase.PurchaseTransaction.GrossTotal * purchase.PurchaseTransaction.Tax;
                    totalCOGS += availableQuantity * purchaseLineNetTotal - fractionOfTransactionDiscount + fractionOfTransactionTax;
                }
            }

            return totalCOGS;
        }

        private static void IncreaseSoldOrReturned(ERPContext context, Item item, int quantity)
        {
            var purchases = context.PurchaseTransactionLines
            .Include("PurchaseTransaction")
            .Where(e => e.ItemID.Equals(item.ItemID) && e.SoldOrReturned < e.Quantity)
            .OrderBy(purchaseTransactionLine => purchaseTransactionLine.PurchaseTransactionID)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.Quantity - purchaseTransactionLine.SoldOrReturned)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.PurchasePrice)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.Discount)
            .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.WarehouseID)
            .ToList();

            var tracker = quantity;

            foreach (var purchase in purchases)
            {
                var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;

                if (tracker <= availableQuantity)
                {
                    purchase.SoldOrReturned += tracker;
                    break;
                }

                if (tracker > availableQuantity)
                {
                    purchase.SoldOrReturned += availableQuantity;
                    tracker -= availableQuantity;
                }
            }
        }

        private static void IncreaseStock(ERPContext context, Warehouse warehouse, Item item, int quantity)
        {
            var stockFromDatabase = context.Stocks.SingleOrDefault(stock => stock.ItemID.Equals(item.ItemID) && stock.WarehouseID.Equals(warehouse.ID));
            if (stockFromDatabase == null)
            {
                var newStock = new Stock
                {
                    Item = item,
                    Warehouse = warehouse,
                    Pieces = quantity
                };
                context.Stocks.Add(newStock);
            }
            else stockFromDatabase.Pieces += quantity;
        }

        private static void AddStockAdjustmentDecrementLedgerTransactionToDatabase(ERPContext context, StockAdjustmentTransaction stockAdjustmentTransaction, decimal totalCOGSAdjustment)
        {
            var ledgerTransaction = new LedgerTransaction();
            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, ledgerTransaction, UtilityMethods.GetCurrentDate().Date,
                stockAdjustmentTransaction.StockAdjustmentTransactionID, "Stock Adjustment (Decrement)")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, ledgerTransaction, "Cost of Goods Sold", "Debit", totalCOGSAdjustment);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, ledgerTransaction, "Inventory", "Credit", totalCOGSAdjustment);
        }

        private static void AddStockAdjustmentTransactionToDatabaseContext(ERPContext context, StockAdjustmentTransaction stockAdjustmentTransaction)
        {
            var user = Application.Current.FindResource("CurrentUser") as User;
            stockAdjustmentTransaction.User = context.Users.Single(e => e.Username.Equals(user.Username));
            context.StockAdjustmentTransactions.Add(stockAdjustmentTransaction);
        }

        private static void AddLineToStockAdjustmentPurchaseTransaction(StockAdjustmentTransactionLine line, PurchaseTransaction stockAdjustmentPurchaseTransaction)
        {
            var stockAdjustmentPurchaseLine = new PurchaseTransactionLine
            {
                PurchaseTransaction = stockAdjustmentPurchaseTransaction,
                Item = line.Item,
                Warehouse = line.Warehouse,
                PurchasePrice = 0,
                Discount = 0,
                Quantity = line.Quantity,
                Total = 0,
                SoldOrReturned = 0
            };
            stockAdjustmentPurchaseTransaction.PurchaseTransactionLines.Add(stockAdjustmentPurchaseLine);
        }
        #endregion
    }
}
