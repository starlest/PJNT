﻿namespace PJMixTests.Customers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using ECERP.Models.Accounting;
    using ECERP.Models.Sales;
    using ECERP.Services;
    using ECERP.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SalesCollectTests
    {
        [TestMethod]
        public void TestCollect()
        {
            SalesTransaction testSalesTransaction;
            using (var context = UtilityMethods.createContext())
            {
                testSalesTransaction =
                    context.SalesTransactions.FirstOrDefault(
                        salesTransaction => salesTransaction.Paid < salesTransaction.NetTotal);
            }

            var remainingAmount = testSalesTransaction.NetTotal - testSalesTransaction.Paid;
            const string paymentMode = "Cash";

            SalesTransactionService.Collect(testSalesTransaction, 0, remainingAmount, paymentMode);
            var result1 = IsSalesTransactionInDatabaseFullyPaid(testSalesTransaction) &&
                          IsSalesReceiptLedgerTransactionRecordedInDatabase(testSalesTransaction);
            Assert.AreEqual(result1, true);

            RevertCollection(testSalesTransaction, remainingAmount);
            var result2 = IsSalesTransactionInDatabaseFullyPaid(testSalesTransaction) &&
                          IsSalesReceiptLedgerTransactionRecordedInDatabase(testSalesTransaction);
            Assert.AreEqual(result2, false);

        }

        private static void RevertCollection(SalesTransaction salesTransaction, decimal remainingAmount)
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                salesTransaction =
                    context.SalesTransactions
                        .Single(transaction => transaction.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));
                salesTransaction.Paid -= remainingAmount;

                var ledgerTransaction =
                    context.Ledger_Transactions.SingleOrDefault(
                        transaction =>
                        transaction.Documentation.Equals(salesTransaction.SalesTransactionID) &&
                        transaction.Description.Equals("Sales Transaction Receipt"));
                RemoveLedgerTransactionFromDatabaseContext(context, ledgerTransaction);

                context.SaveChanges();
                ts.Complete();
            }
        }

        private static void RemoveLedgerTransactionFromDatabaseContext(ERPContext context, LedgerTransaction ledgerTransaction)
        {
            context.Entry(ledgerTransaction).State = EntityState.Modified;
            foreach (var transactionLine in ledgerTransaction.LedgerTransactionLines.ToList())
                context.Ledger_Transaction_Lines.Remove(transactionLine); 
            context.Ledger_Transactions.Remove(ledgerTransaction);
            context.SaveChanges();
        }

        private static bool IsSalesTransactionInDatabaseFullyPaid(SalesTransaction salesTransaction)
        {
            using (var context = UtilityMethods.createContext())
            {
                var salesTransactionFromDatabase =
                    context.SalesTransactions.Single(
                        transaction => transaction.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));
                return salesTransactionFromDatabase.NetTotal - salesTransactionFromDatabase.Paid == 0;
            }
        }

        private static bool IsSalesReceiptLedgerTransactionRecordedInDatabase(SalesTransaction salesTransaction)
        {
            using (var context = UtilityMethods.createContext())
            {
                var ledgerTransaction =
                    context.Ledger_Transactions.FirstOrDefault(
                        transaction =>
                            transaction.Documentation.Equals(salesTransaction.SalesTransactionID) &&
                            transaction.Description.Equals("Sales Transaction Receipt"));
                return ledgerTransaction != null;
            }
        }
    }
}
