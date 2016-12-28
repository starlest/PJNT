namespace PJMixTests.Customers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows.Input;
    using System.Windows;
    using System.Windows.Forms;
    using ECERP.Utilities;
    using ECERP.Utilities.ModelHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SalesIssueTests
    {
        [TestMethod]
        public void TestIssue()
        {
            using (var context = UtilityMethods.createContext())
            {
                var unissuedInvoices =
                    context.SalesTransactions.Where(transaction => transaction.InvoiceIssued == null).ToList();

                foreach (var invoice in unissuedInvoices)
                {
                    SalesTransactionHelper.IssueSalesTransactionInvoice(invoice);
                    if (!CheckInventoryValue())
                        MessageBox.Show(invoice.SalesTransactionID);
                }
            }
        }

        private bool CheckInventoryValue()
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                var actualCOGS = context.Ledger_Account_Balances.Where(e => e.LedgerAccount.Name.Equals("Inventory")).FirstOrDefault().Balance3 +
                context.Ledger_General.Where(e => e.LedgerAccount.Name.Equals("Inventory")).FirstOrDefault().Debit -
                context.Ledger_General.Where(e => e.LedgerAccount.Name.Equals("Inventory")).FirstOrDefault().Credit; // changge beginningbalaance

                decimal calculatedCOGS = 0;

                var purchaseTransactionLines = context.PurchaseTransactionLines
                .Include("PurchaseTransaction")
                .Where(e => e.SoldOrReturned < e.Quantity).ToList();

                foreach (var purchase in purchaseTransactionLines)
                {
                    var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                    var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;
                    if (purchaseLineNetTotal == 0) continue;
                    var fractionOfTransactionDiscount = ((availableQuantity * purchaseLineNetTotal) / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                    var fractionOfTransactionTax = ((availableQuantity * purchaseLineNetTotal) / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                    calculatedCOGS += (availableQuantity * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                }

                if (actualCOGS != calculatedCOGS)
                {
                    return false;
                }
                return true;
                ts.Complete();
            }
        }
    }
}
