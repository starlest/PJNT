namespace ECERP.Utilities.ModelHelpers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models;
    using Models.Accounting;
    using Models.Inventory;
    using Models.Purchase;

    public static class PurchaseTransactionHelper
    {
        public static bool IsLastSaveSuccessful;

        public static void AddNewTransactionToDatabase(PurchaseTransaction purchaseTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                using (var context = UtilityMethods.createContext())
                {
                    foreach (var line in purchaseTransaction.PurchaseTransactionLines)
                    {
                        AttachPurchaseTransactionLineToDatabaseContext(context, line);
                        IncreaseLineItemStockInDatabaseContext(context, line);
                        context.SaveChanges();
                    }

                    AttachPurchaseTransactionPropertiesToDatabaseContext(context, purchaseTransaction);
                    RecordPurchaseLedgerTransactionInDatabaseContext(context, purchaseTransaction);
                }

                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        public static void EditTransactionInDatabase(PurchaseTransaction editedPurchaseTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                using (var context = UtilityMethods.createContext())
                {
                    RecordEditedPurchaseTransactionLedgerTransactionsInDatabaseContext(context,
                        editedPurchaseTransaction);
                    SetPurchaseTransactionInDatabaseContextToEditedPurchaseTransactionProperties(context,
                        editedPurchaseTransaction);
                }

                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        public static void DeleteTransactionInDatabase(PurchaseTransaction purchaseTransaction)
        {
            IsLastSaveSuccessful = false;

            using (var ts = new TransactionScope())
            {
                using (var context = UtilityMethods.createContext())
                {
                    var purchaseTransactionInDatabase =
                        context.PurchaseTransactions
                            .Include("Supplier")
                            .Include("PurchaseTransactionLines.Item")
                            .Include("PurchaseTransactionLines.Warehouse")
                            .Include("PurchaseTransactionLines")
                            .SingleOrDefault(
                                transaction => transaction.PurchaseID.Equals(purchaseTransaction.PurchaseID));
                    if (purchaseTransactionInDatabase == null) return; // Purchase transaction could not be found

                    var lines = purchaseTransactionInDatabase.PurchaseTransactionLines.ToList();
                    foreach (var line in lines)
                    {
                        if (!IsThereEnoughLineItemStockInDatabaseContext(context, line)) return;
                        DecreasePurchaseTransactionLineItemStockInDatabaseContext(context, line);
                        context.SaveChanges();
                    }

                    AddPurchaseTransactionDeletionLedgerTransactionToDatabaseContext(context,
                        purchaseTransactionInDatabase);
                    context.PurchaseTransactions.Remove(purchaseTransactionInDatabase);
                    context.SaveChanges();
                }

                ts.Complete();
            }

            IsLastSaveSuccessful = true;
        }

        public static void MakePayment(PurchaseTransaction purchaseTransaction, decimal paymentAmount,
            decimal useCreditsAmount, string paymentMode)
        {
            if (purchaseTransaction.Paid + useCreditsAmount + paymentAmount > purchaseTransaction.Total)
            {
                MessageBox.Show("There was a problem making the payment. Please contact Edwin.", "Invalid Amount",
                    MessageBoxButton.OK);
                return;
            }
            using (var ts = new TransactionScope())
            {
                using (var context = UtilityMethods.createContext())
                {
                    context.Entry(purchaseTransaction).State = EntityState.Modified;
                    purchaseTransaction.Paid += paymentAmount + useCreditsAmount;
                    purchaseTransaction.Supplier.PurchaseReturnCredits -= useCreditsAmount;

                    RecordPaymentLedgerTransactionInDatabaseContext(context, purchaseTransaction, paymentAmount, paymentMode);

                    context.SaveChanges();
                }

                ts.Complete();
            }
        }

        #region Add New Transaction Helper Methods

        private static void AttachPurchaseTransactionLineToDatabaseContext(ERPContext context,
            PurchaseTransactionLine line)
        {
            line.Item = context.Inventory.Single(item => item.ItemID.Equals(line.Item.ItemID));
            line.Warehouse = context.Warehouses.Single(warehouse => warehouse.ID.Equals(line.Warehouse.ID));
        }

        private static void IncreaseLineItemStockInDatabaseContext(ERPContext context, PurchaseTransactionLine line)
        {
            var stockFromDatabase = context.Stocks.SingleOrDefault(
                stock => stock.ItemID.Equals(line.Item.ItemID) && stock.WarehouseID.Equals(line.Warehouse.ID));

            if (stockFromDatabase == null)
            {
                context.Stocks.Add(new Stock { Item = line.Item, Warehouse = line.Warehouse, Pieces = line.Quantity });
                context.SaveChanges();
            }
            else
                stockFromDatabase.Pieces += line.Quantity;
        }

        private static void RecordPurchaseLedgerTransactionInDatabaseContext(ERPContext context,
            PurchaseTransaction purchaseTransaction)
        {
            var purchaseLedgerTransaction = new LedgerTransaction();
            var accountsPayableName = purchaseTransaction.Supplier.Name + " Accounts Payable";
            if (
                !LedgerTransactionHelper.AddTransactionToDatabase(context, purchaseLedgerTransaction,
                    UtilityMethods.GetCurrentDate(), purchaseTransaction.PurchaseID, "Purchase Transaction")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseLedgerTransaction, Constants.Accounting.INVENTORY,
                Constants.Accounting.DEBIT, purchaseTransaction.Total);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseLedgerTransaction, accountsPayableName,
                Constants.Accounting.CREDIT, purchaseTransaction.Total);
            context.SaveChanges();
        }

        private static void AttachPurchaseTransactionPropertiesToDatabaseContext(ERPContext context,
            PurchaseTransaction purchaseTransaction)
        {
            purchaseTransaction.Supplier =
                context.Suppliers.Single(supplier => supplier.ID.Equals(purchaseTransaction.Supplier.ID));
            var currentUser = Application.Current.FindResource("CurrentUser") as User;
            purchaseTransaction.User = context.Users.Single(user => user.Username.Equals(currentUser.Username));
            context.PurchaseTransactions.Add(purchaseTransaction);
            context.SaveChanges();
        }

        #endregion

        #region Edit Transaction Helper Methods

        private static void RecordEditedPurchaseTransactionLedgerTransactionsInDatabaseContext(ERPContext context,
            PurchaseTransaction editedPurchaseTransaction)
        {
            var transactionFromDatabase = context.PurchaseTransactions
                .Include("PurchaseTransactionLines")
                .Include("PurchaseTransactionLines.Item")
                .Include("PurchaseTransactionLines.Warehouse")
                .Single(e => e.PurchaseID.Equals(editedPurchaseTransaction.PurchaseID));

            if (transactionFromDatabase.Total != editedPurchaseTransaction.Total)
            {
                var valueChanged = editedPurchaseTransaction.Total - transactionFromDatabase.Total;
                RecordEditedPurchaseTransactionNetTotalChangedLedgerTransactionInDatabaseContext(context,
                    editedPurchaseTransaction, valueChanged);
            }

            var transactionFromDatabaseTotalCOGS = CalculatePurchaseTransactionTotalCOGS(transactionFromDatabase);
            var editedtransactionTotalCOGS = CalculatePurchaseTransactionTotalCOGS(editedPurchaseTransaction);
            var totalCOGSChanged = editedtransactionTotalCOGS - transactionFromDatabaseTotalCOGS;
            if (totalCOGSChanged != 0)
                RecordEditedPurchaseTransactionTotalCOGSChangedLedgerTransactionInDatabaseContext(context,
                    editedPurchaseTransaction, totalCOGSChanged);
        }

        private static void RecordEditedPurchaseTransactionNetTotalChangedLedgerTransactionInDatabaseContext(
            ERPContext context, PurchaseTransaction editedPurchaseTransaction, decimal valueChanged)
        {
            var purchaseTransactionNetTotalChangedLedgerTransaction = new LedgerTransaction();

            LedgerTransactionHelper.AddTransactionToDatabase(context,
                purchaseTransactionNetTotalChangedLedgerTransaction,
                UtilityMethods.GetCurrentDate().Date, editedPurchaseTransaction.PurchaseID,
                "Purchase Transaction Adjustment");
            context.SaveChanges();

            if (valueChanged > 0)
            {
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionNetTotalChangedLedgerTransaction, "Inventory",
                    "Debit", valueChanged);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionNetTotalChangedLedgerTransaction,
                    $"{editedPurchaseTransaction.Supplier.Name} Accounts Payable", "Credit", valueChanged);
            }
            else
            {
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionNetTotalChangedLedgerTransaction,
                    $"{editedPurchaseTransaction.Supplier.Name} Accounts Payable", "Debit", -valueChanged);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionNetTotalChangedLedgerTransaction, "Inventory",
                    "Credit", -valueChanged);
            }
            context.SaveChanges();
        }

        private static decimal CalculatePurchaseTransactionTotalCOGS(PurchaseTransaction purchaseTransaction)
        {
            var totalCOGS = 0m;
            foreach (var line in purchaseTransaction.PurchaseTransactionLines)
            {
                if (line.SoldOrReturned <= 0) continue;
                var lineNetTotal = line.PurchasePrice - line.Discount;
                if (lineNetTotal == 0) continue;
                var lineFractionOfTransactionDiscount = line.SoldOrReturned * lineNetTotal /
                                                        purchaseTransaction.GrossTotal * purchaseTransaction.Discount;
                var lineFractionOfTransactionTax = line.SoldOrReturned * lineNetTotal /
                                                   purchaseTransaction.GrossTotal * purchaseTransaction.Tax;
                var lineCOGS = line.SoldOrReturned * lineNetTotal - lineFractionOfTransactionDiscount +
                               lineFractionOfTransactionTax;
                totalCOGS += lineCOGS;
            }
            return totalCOGS;
        }

        private static void RecordEditedPurchaseTransactionTotalCOGSChangedLedgerTransactionInDatabaseContext(
            ERPContext context, PurchaseTransaction editedPurchaseTransaction, decimal totalCOGSChanged)
        {
            var purchaseTransactionTotalCOGSChangedLedgerTransaction = new LedgerTransaction();
            LedgerTransactionHelper.AddTransactionToDatabase(context,
                purchaseTransactionTotalCOGSChangedLedgerTransaction, UtilityMethods.GetCurrentDate().Date,
                editedPurchaseTransaction.PurchaseID, "Purchase Transaction Adjustment (COGS)");
            context.SaveChanges();

            if (totalCOGSChanged > 0)
            {
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionTotalCOGSChangedLedgerTransaction, "Cost of Goods Sold", "Debit",
                    totalCOGSChanged);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionTotalCOGSChangedLedgerTransaction, "Inventory", "Credit", totalCOGSChanged);
            }
            else
            {
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionTotalCOGSChangedLedgerTransaction, "Inventory", "Debit", -totalCOGSChanged);
                LedgerTransactionHelper.AddTransactionLineToDatabase(context,
                    purchaseTransactionTotalCOGSChangedLedgerTransaction, "Cost of Goods Sold", "Credit",
                    -totalCOGSChanged);
            }
            context.SaveChanges();
        }

        private static void SetPurchaseTransactionInDatabaseContextToEditedPurchaseTransactionProperties(
            ERPContext context, PurchaseTransaction editedPurchaseTransaction)
        {
            var transactionFromDatabase = context.PurchaseTransactions
                .Include("Supplier")
                .Include("PurchaseTransactionLines")
                .Include("PurchaseTransactionLines.Item")
                .Include("PurchaseTransactionLines.Warehouse")
                .Single(e => e.PurchaseID.Equals(editedPurchaseTransaction.PurchaseID));

            transactionFromDatabase.Date = editedPurchaseTransaction.Date;
            transactionFromDatabase.DueDate = editedPurchaseTransaction.DueDate;
            transactionFromDatabase.InvoiceDate = editedPurchaseTransaction.InvoiceDate;
            transactionFromDatabase.DOID = editedPurchaseTransaction.DOID;
            transactionFromDatabase.Note = editedPurchaseTransaction.Note;
            transactionFromDatabase.Tax = editedPurchaseTransaction.Tax;
            transactionFromDatabase.GrossTotal = editedPurchaseTransaction.GrossTotal;
            transactionFromDatabase.Discount = editedPurchaseTransaction.Discount;
            transactionFromDatabase.Total = editedPurchaseTransaction.Total;
            var currentUser = Application.Current.FindResource(Constants.CURRENTUSER) as User;
            transactionFromDatabase.User = context.Users.Single(user => user.Username.Equals(currentUser.Username));

            foreach (var line in transactionFromDatabase.PurchaseTransactionLines.ToList())
            {
                context.PurchaseTransactionLines.Remove(line);
                context.SaveChanges();
            }

            foreach (var editedLine in editedPurchaseTransaction.PurchaseTransactionLines)
            {
                editedLine.PurchaseTransaction = transactionFromDatabase;
                editedLine.Item = context.Inventory.Single(item => item.ItemID.Equals(editedLine.Item.ItemID));
                editedLine.Warehouse =
                    context.Warehouses.Single(warehouse => warehouse.ID.Equals(editedLine.Warehouse.ID));
                context.PurchaseTransactionLines.Add(editedLine);
                context.SaveChanges();
            }

            context.SaveChanges();
        }

        #endregion

        #region Delete Transaction Helper Methods

        private static bool IsThereEnoughLineItemStockInDatabaseContext(ERPContext context,
            PurchaseTransactionLine purchaseTransactionLine)
        {
            var stockFromDatabase = context.Stocks.SingleOrDefault(
                stock => stock.Item.ItemID.Equals(purchaseTransactionLine.Item.ItemID) &&
                         stock.Warehouse.ID.Equals(purchaseTransactionLine.Warehouse.ID));
            if (stockFromDatabase != null && stockFromDatabase.Pieces >= purchaseTransactionLine.Quantity)
                return true;
            var availableQuantity = stockFromDatabase?.Pieces ?? 0;
            MessageBox.Show(
                $"{purchaseTransactionLine.Item.Name} has only {availableQuantity / purchaseTransactionLine.Item.PiecesPerUnit} units {availableQuantity % purchaseTransactionLine.Item.PiecesPerUnit} pieces left.",
                "Invalid Quantity", MessageBoxButton.OK);
            return false;
        }

        private static void DecreasePurchaseTransactionLineItemStockInDatabaseContext(ERPContext context,
            PurchaseTransactionLine purchaseTransactionLine)
        {
            var stockFromDatabase = context.Stocks.Single(
                stock => stock.Item.ItemID.Equals(purchaseTransactionLine.Item.ItemID) &&
                         stock.Warehouse.ID.Equals(purchaseTransactionLine.Warehouse.ID));
            stockFromDatabase.Pieces -= purchaseTransactionLine.Quantity;
            if (stockFromDatabase.Pieces == 0) context.Stocks.Remove(stockFromDatabase);
        }

        private static void AddPurchaseTransactionDeletionLedgerTransactionToDatabaseContext(ERPContext context,
            PurchaseTransaction purchaseTransaction)
        {
            var purchaseDeletionLedgerTransaction = new LedgerTransaction();
            if (
                !LedgerTransactionHelper.AddTransactionToDatabase(context, purchaseDeletionLedgerTransaction,
                    UtilityMethods.GetCurrentDate(), purchaseTransaction.PurchaseID, "Purchase Transaction Deletion"))
                return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseDeletionLedgerTransaction,
                $"{purchaseTransaction.Supplier.Name} Accounts Payable", "Debit", purchaseTransaction.Total);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, purchaseDeletionLedgerTransaction, "Inventory",
                "Credit", purchaseTransaction.Total);
            context.SaveChanges();
        }

        #endregion

        #region Payment helper Methods

        private static void RecordPaymentLedgerTransactionInDatabaseContext(ERPContext context,
            PurchaseTransaction purchaseTransaction, decimal paymentAmount, string paymentMode)
        {
            var accountsPayableName = purchaseTransaction.Supplier.Name + " Accounts Payable";
            var paymentLedgerTransaction = new LedgerTransaction();

            if (
                !LedgerTransactionHelper.AddTransactionToDatabase(context, paymentLedgerTransaction,
                    UtilityMethods.GetCurrentDate(), purchaseTransaction.PurchaseID, "Purchase Payment")) return;
            context.SaveChanges();

            LedgerTransactionHelper.AddTransactionLineToDatabase(context, paymentLedgerTransaction, accountsPayableName,
                "Debit", paymentAmount);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, paymentLedgerTransaction, paymentMode,
                "Credit", paymentAmount);
            context.SaveChanges();
        }

        #endregion
    }
}