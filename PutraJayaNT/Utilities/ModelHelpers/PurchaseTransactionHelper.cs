namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Data.Entity;
    using System.Transactions;
    using Models.Accounting;
    using Models.Purchase;

    public static class PurchaseTransactionHelper
    {
        public static void MakePayment(PurchaseTransaction purchaseTransaction, decimal paymentAmount,
            decimal useCreditsAmount, string paymentMode)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                context.Entry(purchaseTransaction).State = EntityState.Modified;
                purchaseTransaction.Paid += paymentAmount + useCreditsAmount;
                purchaseTransaction.Supplier.PurchaseReturnCredits -= useCreditsAmount;

                RecordPaymentLedgerTransactionInDatabaseContext(context, purchaseTransaction, paymentAmount, paymentMode);

                context.SaveChanges();
                ts.Complete();
            }
        }

        #region Payment helper Methods
        private static void RecordPaymentLedgerTransactionInDatabaseContext(ERPContext context, PurchaseTransaction purchaseTransaction, decimal paymentAmount, string paymentMode)
        {
            var accountsPayableName = purchaseTransaction.Supplier.Name + " Accounts Payable";
            var paymentLedgerTransaction = new LedgerTransaction();

            if (!LedgerTransactionHelper.AddTransactionToDatabase(context, paymentLedgerTransaction, UtilityMethods.GetCurrentDate(), purchaseTransaction.PurchaseID, "Purchase Payment")) return;
            context.SaveChanges();

            LedgerTransactionHelper.AddTransactionLineToDatabase(context, paymentLedgerTransaction, accountsPayableName, "Debit", paymentAmount);
            LedgerTransactionHelper.AddTransactionLineToDatabase(context, paymentLedgerTransaction, paymentMode, "Credit", paymentAmount);
            context.SaveChanges();
        }
        #endregion
    }
}
