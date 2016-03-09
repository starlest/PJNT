namespace PJMixTests.Customers
{
    using System.Linq;
    using System.Transactions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PutraJayaNT.Models.Accounting;
    using PutraJayaNT.Models.Sales;
    using PutraJayaNT.Utilities;
    using PutraJayaNT.Utilities.Database.Ledger;
    using PutraJayaNT.Utilities.Database.Sales;
    using PutraJayaNT.ViewModels.Customers;

    [TestClass]
    public class SalesCollectTests
    {
        [TestMethod]
        public void TestCollect()
        {
            var testSalesTransaction =
                DatabaseSalesTransactionHelper.FirstOrDefaultWithoutLines(salesTransaction => salesTransaction.Paid < salesTransaction.NetTotal);
            var remainingAmount = testSalesTransaction.NetTotal - testSalesTransaction.Paid;
            const string paymentMode = "Cash";
            SalesCollectVM.Collect(testSalesTransaction, 0, remainingAmount, paymentMode);
            var result1 = IsSalesTransactionInDatabaseFullyPaid(testSalesTransaction) && IsSalesReceiptLedgerTransactionRecordedInDatabase(testSalesTransaction);
            Assert.AreEqual(result1, true);
            RevertCollection(testSalesTransaction, remainingAmount);
            var result2 = IsSalesTransactionInDatabaseFullyPaid(testSalesTransaction) && IsSalesReceiptLedgerTransactionRecordedInDatabase(testSalesTransaction);
            Assert.AreEqual(result2, false);
        }

        private static void RevertCollection(SalesTransaction salesTransaction, decimal remainingAmount)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();

                DatabaseSalesTransactionHelper.AttachToDatabaseContext(context, ref salesTransaction);
                salesTransaction.Paid -= remainingAmount;

                var ledgerTransaction =
                    DatabaseLedgerTransactionHelper.FirstOrDefault(
                        transaction =>
                        transaction.Documentation.Equals(salesTransaction.SalesTransactionID) &&
                        transaction.Description.Equals("Sales Transaction Receipt"));
                RemoveLedgerTransactionFromDatabaseContext(context, ledgerTransaction);

                ts.Complete();
            }
        }

        private static void RemoveLedgerTransactionFromDatabaseContext(ERPContext context, LedgerTransaction ledgerTransaction)
        {
            DatabaseLedgerTransactionHelper.AttachToObjectFromDatabaseContext(context, ref ledgerTransaction);
            foreach (var transactionLine in ledgerTransaction.LedgerTransactionLines.ToList())
                context.Ledger_Transaction_Lines.Remove(transactionLine); 
            context.Ledger_Transactions.Remove(ledgerTransaction);
            context.SaveChanges();
        }

        private static bool IsSalesTransactionInDatabaseFullyPaid(SalesTransaction salesTransaction)
        {
            var salesTransactionFromDatabase =
                DatabaseSalesTransactionHelper.FirstOrDefault(
                    transaction => transaction.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));
            return salesTransactionFromDatabase.NetTotal - salesTransactionFromDatabase.Paid == 0;
        }

        private static bool IsSalesReceiptLedgerTransactionRecordedInDatabase(SalesTransaction salesTransaction)
        {
            var ledgerTransaction =
                DatabaseLedgerTransactionHelper.FirstOrDefault(
                    transaction =>
                    transaction.Documentation.Equals(salesTransaction.SalesTransactionID) &&
                    transaction.Description.Equals("Sales Transaction Receipt"));
            return ledgerTransaction != null;
        }
    }
}
