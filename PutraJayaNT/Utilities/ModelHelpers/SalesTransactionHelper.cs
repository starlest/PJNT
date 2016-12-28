namespace ECERP.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models;
    using Models.Accounting;
    using Models.Inventory;
    using Models.Sales;

    public static class SalesTransactionHelper
    {
        public static bool IsLastSaveSuccessful;

        public static void AddTransactionToDatabase(SalesTransaction salesTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var context = UtilityMethods.createContext())
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

            IsLastSaveSuccessful = true;
        }

        public static void EditNotIssuedInvoiceTransaction(SalesTransaction editedSalesTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();
                var salesTransactionFromDatabaseContext = GetDatabaseContextSalesTransaction(context,
                    editedSalesTransaction);
                AssignEditedPropertiesToSalesTransactionFromDatabaseContext(editedSalesTransaction,
                    salesTransactionFromDatabaseContext);
                AttachSalesTransactionPropertiesToDatabaseContext(context, salesTransactionFromDatabaseContext);
                AssignEditedSalesTransactionLinesToSalesTransactionFromDatabaseContext(context, editedSalesTransaction,
                    salesTransactionFromDatabaseContext);
                context.SaveChanges();
                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        public static void EditIssuedInvoiceTransaction(SalesTransaction editedSalesTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                var salesTransactionFromDatabaseContext = GetDatabaseContextSalesTransaction(context,
                    editedSalesTransaction);

                if (salesTransactionFromDatabaseContext.NetTotal != editedSalesTransaction.NetTotal)
                    RecordSalesRevenueAdjustmentLedgerTransaction(context, editedSalesTransaction,
                        salesTransactionFromDatabaseContext);

                AssignEditedSalesTransactionLinesToSalesTransactionFromDatabaseContext(context, editedSalesTransaction,
                    salesTransactionFromDatabaseContext);
                AssignEditedInvoiceIssuedSalesTransactionPropertiesToSalesTransactionFromDatabaseContext(
                    editedSalesTransaction, salesTransactionFromDatabaseContext);
                context.SaveChanges();
                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        public static void IssueSalesTransactionInvoice(SalesTransaction salesTransaction)
        {
            IsLastSaveSuccessful = true;

            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                var salesTransactionFromDatabase = context.SalesTransactions.Include("Customer").Single(
                    transaction => transaction.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));

                salesTransactionFromDatabase.InvoiceIssued = UtilityMethods.GetCurrentDate().Date;
                RecordSalesRevenueRecognitionLedgerTransactionInDatabaseContext(context, salesTransactionFromDatabase);
                RecordCostOfGoodsSoldLedgerTransactionInDatabaseContext(context, salesTransactionFromDatabase);
                //IncreaseSoldOrReturnedOfSalesTransactionItemsInDatabaseContext(context, salesTransactionFromDatabase);

                var user = Application.Current.FindResource("CurrentUser") as User;
                salesTransactionFromDatabase.User = context.Users.Single(e => e.Username.Equals(user.Username));

                context.SaveChanges();
                ts.Complete();
            }

            IsLastSaveSuccessful = false;
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

        private static void AttachSalesTransactionPropertiesToDatabaseContext(ERPContext context,
            SalesTransaction salesTransaction)
        {
            salesTransaction.Customer =
                context.Customers.Single(customer => customer.ID.Equals(salesTransaction.Customer.ID));
            salesTransaction.User = context.Users.Single(user => user.Username.Equals(salesTransaction.User.Username));
            salesTransaction.CollectionSalesman = salesTransaction.CollectionSalesman == null
                ? context.Salesmans.Single(salesman => salesman.Name.Equals(" "))
                : context.Salesmans.Single(salesman => salesman.ID.Equals(salesTransaction.CollectionSalesman.ID));
        }

        #endregion

        #region Edit Not Issued Invoice Transaction Helper Methods

        private static void AssignEditedPropertiesToSalesTransactionFromDatabaseContext(
            SalesTransaction editedSalesTransaction, SalesTransaction salesTransactionFromDatabaseContext)
        {
            salesTransactionFromDatabaseContext.Date = editedSalesTransaction.Date;
            salesTransactionFromDatabaseContext.DueDate = editedSalesTransaction.DueDate;
            salesTransactionFromDatabaseContext.Customer = editedSalesTransaction.Customer;
            salesTransactionFromDatabaseContext.Notes = editedSalesTransaction.Notes;
            salesTransactionFromDatabaseContext.Discount = editedSalesTransaction.Discount;
            salesTransactionFromDatabaseContext.Tax = editedSalesTransaction.Tax;
            salesTransactionFromDatabaseContext.SalesExpense = editedSalesTransaction.SalesExpense;
            salesTransactionFromDatabaseContext.GrossTotal = editedSalesTransaction.GrossTotal;
            salesTransactionFromDatabaseContext.NetTotal = editedSalesTransaction.NetTotal;
            salesTransactionFromDatabaseContext.InvoiceIssued = editedSalesTransaction.InvoiceIssued;
            salesTransactionFromDatabaseContext.User = editedSalesTransaction.User;
        }

        private static void AssignEditedSalesTransactionLinesToSalesTransactionFromDatabaseContext(ERPContext context,
            SalesTransaction editedSalesTransaction, SalesTransaction salesTransactionFromDatabaseContext)
        {
            RemoveAllSalesTransactionLinesFromSalesTransactionFromDatabaseContext(
                context, salesTransactionFromDatabaseContext);
            AddAllEditedSalesTransactionLinesToSalesTransactionFromDatabaseContext(
                context, editedSalesTransaction);
        }

        private static void RemoveAllSalesTransactionLinesFromSalesTransactionFromDatabaseContext(ERPContext context,
            SalesTransaction salesTransactionFromDatabaseContext)
        {
            var originalTransactionLines = salesTransactionFromDatabaseContext.SalesTransactionLines.ToList();
            foreach (var line in originalTransactionLines)
            {
                salesTransactionFromDatabaseContext.SalesTransactionLines.Remove(line);
                IncreaseLineItemStockInDatabaseContext(context, line);
                context.SaveChanges();
            }
        }

        private static void IncreaseLineItemStockInDatabaseContext(ERPContext context, SalesTransactionLine line)
        {
            var stock = context.Stocks.SingleOrDefault(
                e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID));
            if (stock != null) stock.Pieces += line.Quantity;
            else
            {
                var newStock = new Stock
                {
                    Item = context.Inventory.Single(item => item.ItemID.Equals(line.Item.ItemID)),
                    Warehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(line.Warehouse.ID)),
                    Pieces = line.Quantity
                };
                context.Stocks.Add(newStock);
            }
        }

        private static void AddAllEditedSalesTransactionLinesToSalesTransactionFromDatabaseContext(ERPContext context,
            SalesTransaction editedSalesTransaction)
        {
            var salesTransactionFromDatabaseContext = GetDatabaseContextSalesTransaction(context, editedSalesTransaction);
            foreach (var line in editedSalesTransaction.SalesTransactionLines.ToList())
            {
                line.SalesTransaction = salesTransactionFromDatabaseContext;
                AttachSalesTransactionLineToDatabaseContext(context, line);
                context.SalesTransactionLines.Add(line);
                ReduceLineItemStockInDatabaseContext(context, line);
                context.SaveChanges();
            }
        }

        private static SalesTransaction GetDatabaseContextSalesTransaction(ERPContext context,
            SalesTransaction salesTransaction)
        {
            return context.SalesTransactions
                .Include("Customer")
                .Include("SalesTransactionLines")
                .Include("SalesTransactionLines.Salesman")
                .Include("SalesTransactionLines.Warehouse")
                .Include("SalesTransactionLines.Item")
                .Single(e => e.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));
        }

        #endregion

        #region Edit Issued Invoice Transaction Helper Methods

        private static void RecordSalesRevenueAdjustmentLedgerTransaction(ERPContext context,
            SalesTransaction editedSalesTransaction, SalesTransaction transactionFromDatabaseContext)
        {
            var transactionTotalDifference = editedSalesTransaction.NetTotal - transactionFromDatabaseContext.NetTotal;

            var salesRevenueAdjustmentLedgerTransaction = new LedgerTransaction();
            LedgerTransactionHelper.AddTransactionToDatabase(context, salesRevenueAdjustmentLedgerTransaction,
                UtilityMethods.GetCurrentDate().Date, editedSalesTransaction.SalesTransactionID,
                "Sales Revenue Adjustment");
            context.SaveChanges();

            if (transactionTotalDifference > 0)
            {
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, salesRevenueAdjustmentLedgerTransaction,
                    $"{transactionFromDatabaseContext.Customer.Name} Accounts Receivable", "Debit",
                    transactionTotalDifference);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, salesRevenueAdjustmentLedgerTransaction,
                    "Sales Revenue", "Credit", transactionTotalDifference);
            }

            else
            {
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, salesRevenueAdjustmentLedgerTransaction,
                    "Sales Revenue", "Debit", -transactionTotalDifference);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context, salesRevenueAdjustmentLedgerTransaction,
                    $"{transactionFromDatabaseContext.Customer.Name} Accounts Receivable",
                    "Credit", -transactionTotalDifference);
            }

            context.SaveChanges();
        }

        private static void AssignEditedInvoiceIssuedSalesTransactionPropertiesToSalesTransactionFromDatabaseContext(
            SalesTransaction editedSalesTransaction, SalesTransaction salesTransactionFromDatabaseContext)
        {
            salesTransactionFromDatabaseContext.Notes = editedSalesTransaction.Notes;
            salesTransactionFromDatabaseContext.GrossTotal = editedSalesTransaction.GrossTotal;
            salesTransactionFromDatabaseContext.Discount = editedSalesTransaction.Discount;
            salesTransactionFromDatabaseContext.Tax = editedSalesTransaction.Tax;
            salesTransactionFromDatabaseContext.SalesExpense = editedSalesTransaction.SalesExpense;
            salesTransactionFromDatabaseContext.NetTotal = editedSalesTransaction.NetTotal;
        }

        #endregion

        #region Issue Invoice Helper Methods

        private static void RecordSalesRevenueRecognitionLedgerTransactionInDatabaseContext(ERPContext context,
            SalesTransaction salesTransaction)
        {
            var salesRevenueRecognitionLedgerTransaction = new LedgerTransaction();
            if (
                !LedgerTransactionHelper.AddTransactionToDatabase(context, salesRevenueRecognitionLedgerTransaction,
                    UtilityMethods.GetCurrentDate().Date, salesTransaction.SalesTransactionID, "Sales Revenue")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, salesRevenueRecognitionLedgerTransaction,
                $"{salesTransaction.Customer.Name} Accounts Receivable", "Debit", salesTransaction.NetTotal);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, salesRevenueRecognitionLedgerTransaction,
                "Sales Revenue", "Credit", salesTransaction.NetTotal);
            context.SaveChanges();
        }

        private static void RecordCostOfGoodsSoldLedgerTransactionInDatabaseContext(ERPContext context,
            SalesTransaction salesTransaction)
        {
            var costOfGoodsSoldAmount = CalculateCOGSAndIncreaseSoldOrReturned(context, salesTransaction);

            var costOfGoodsSoldLedgerTransaction = new LedgerTransaction();
            if (
                !LedgerTransactionHelper.AddTransactionToDatabase(context, costOfGoodsSoldLedgerTransaction,
                    UtilityMethods.GetCurrentDate().Date, salesTransaction.SalesTransactionID, "Cost of Goods Sold"))
                return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, costOfGoodsSoldLedgerTransaction,
                "Cost of Goods Sold", "Debit", costOfGoodsSoldAmount);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, costOfGoodsSoldLedgerTransaction, "Inventory",
                "Credit", costOfGoodsSoldAmount);
            context.SaveChanges();
        }

        private static decimal CalculateCOGSAndIncreaseSoldOrReturned(ERPContext context,
            SalesTransaction salesTransaction)
        {
            var costOfGoodsSoldAmount = 0m;

            foreach (var line in salesTransaction.SalesTransactionLines.ToList())
            {
                var itemID = line.Item.ItemID;

                var purchases = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Where(
                        purchaseTransactionLine =>
                            purchaseTransactionLine.ItemID.Equals(itemID) &&
                            purchaseTransactionLine.SoldOrReturned < purchaseTransactionLine.Quantity)
                    .OrderBy(purchaseTransaction => purchaseTransaction.PurchaseTransactionID)
                    .ThenByDescending(
                        purchaseTransactionLine =>
                            purchaseTransactionLine.Quantity - purchaseTransactionLine.SoldOrReturned)
                    .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.PurchasePrice)
                    .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.Discount)
                    .ThenByDescending(purchaseTransactionLine => purchaseTransactionLine.WarehouseID)
                    .ToList();

                var tracker = line.Quantity;

                foreach (var purchase in purchases)
                {
                    var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                    var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;

                    if (tracker <= availableQuantity)
                    {
                        purchase.SoldOrReturned += tracker;
                        if (purchaseLineNetTotal == 0) break;
                        var fractionOfTransactionDiscount = tracker * purchaseLineNetTotal /
                                                            purchase.PurchaseTransaction.GrossTotal *
                                                            purchase.PurchaseTransaction.Discount;
                        var fractionOfTransactionTax = tracker * purchaseLineNetTotal /
                                                       purchase.PurchaseTransaction.GrossTotal *
                                                       purchase.PurchaseTransaction.Tax;
                        costOfGoodsSoldAmount += tracker * purchaseLineNetTotal - fractionOfTransactionDiscount +
                                                 fractionOfTransactionTax;
                        break;
                    }

                    if (tracker > availableQuantity)
                    {
                        purchase.SoldOrReturned += availableQuantity;
                        tracker -= availableQuantity;
                        if (purchaseLineNetTotal == 0) continue;
                        var fractionOfTransactionDiscount = availableQuantity * purchaseLineNetTotal /
                                                            purchase.PurchaseTransaction.GrossTotal *
                                                            purchase.PurchaseTransaction.Discount;
                        var fractionOfTransactionTax = availableQuantity * purchaseLineNetTotal /
                                                       purchase.PurchaseTransaction.GrossTotal *
                                                       purchase.PurchaseTransaction.Tax;
                        costOfGoodsSoldAmount += availableQuantity * purchaseLineNetTotal -
                                                 fractionOfTransactionDiscount + fractionOfTransactionTax;
                    }
                }
            }
            context.SaveChanges();
            return costOfGoodsSoldAmount;
        }

        #endregion
    }
}