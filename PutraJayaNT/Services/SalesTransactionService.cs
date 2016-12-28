namespace ECERP.Services
{
    using System.Data.Entity;
    using System.Transactions;
    using Models.Accounting;
    using Models.Sales;
    using Utilities;
    using Utilities.ModelHelpers;

    public static class SalesTransactionService
    {
        public static bool Collect(SalesTransaction salesTransaction, decimal creditsUsed, decimal collectionAmount,
            string paymentMode)
        {
            var totalAmountCollected = creditsUsed + collectionAmount;

            if (salesTransaction.Paid + totalAmountCollected > salesTransaction.NetTotal)
                return false;

            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();
                context.Entry(salesTransaction).State = EntityState.Modified;
                salesTransaction.Paid += totalAmountCollected;
                salesTransaction.Customer.SalesReturnCredits -= creditsUsed;
                SaveCollectionLedgerTransactionInDatabase(context, salesTransaction, collectionAmount, paymentMode);
                context.SaveChanges();
                ts.Complete();
            }

            return true;
        }

        #region Collection Helper Methods

        private static void SaveCollectionLedgerTransactionInDatabase(ERPContext context,
            SalesTransaction salesTransaction, decimal collectionAmount, string paymentMode)
        {
            if (collectionAmount <= 0) return;

            var accountsReceivableName = salesTransaction.Customer.Name + " " + Constants.ACCOUNTS_RECEIVABLE;
            var date = UtilityMethods.GetCurrentDate().Date;
            var transaction = new LedgerTransaction();

            if (
                !LedgerTransactionHelper.AddTransactionToDatabase(context, transaction, date,
                    salesTransaction.SalesTransactionID, "Sales Transaction Receipt")) return;
            context.SaveChanges();
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, paymentMode, Constants.DEBIT,
                collectionAmount);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, transaction, accountsReceivableName, Constants.CREDIT,
                collectionAmount);
            context.SaveChanges();
        }

        #endregion
    }
}