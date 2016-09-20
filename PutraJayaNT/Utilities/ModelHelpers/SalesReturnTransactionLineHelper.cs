namespace ECRP.Utilities.ModelHelpers
{
    using System.Linq;
    using Models.Sales;

    public static class SalesReturnTransactionLineHelper
    {
        public static decimal GetSalesReturnTransactionLineCOGS(SalesReturnTransactionLine salesReturnTransactionLine)
        {
            decimal amount = 0;

            using (var context = UtilityMethods.createContext())
            {
                var purchases = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Where(
                        line => line.ItemID.Equals(salesReturnTransactionLine.Item.ItemID) && line.SoldOrReturned > 0)
                    .OrderByDescending(e => e.PurchaseTransactionID)
                    .ThenByDescending(transaction => transaction.Quantity - transaction.SoldOrReturned)
                    .ThenByDescending(transaction => transaction.PurchasePrice)
                    .ThenByDescending(transaction => transaction.Discount)
                    .ThenByDescending(transaction => transaction.WarehouseID);

                var tracker = salesReturnTransactionLine.Quantity;
                foreach (var purchase in purchases)
                {
                    var purchaseLineTotal = purchase.PurchasePrice - purchase.Discount;

                    if (purchase.SoldOrReturned >= tracker)
                    {
                        if (purchaseLineTotal == 0) break;
                        var fractionOfTransactionDiscount = tracker*purchaseLineTotal/
                                                            purchase.PurchaseTransaction.GrossTotal*
                                                            purchase.PurchaseTransaction.Discount;
                        var fractionOfTransactionTax = tracker*purchaseLineTotal/purchase.PurchaseTransaction.GrossTotal*
                                                       purchase.PurchaseTransaction.Tax;
                        amount += tracker*purchaseLineTotal - fractionOfTransactionDiscount + fractionOfTransactionTax;
                        break;
                    }

                    if (purchase.SoldOrReturned < tracker)
                    {
                        tracker -= purchase.SoldOrReturned;
                        if (purchaseLineTotal == 0) continue;
                        var fractionOfTransactionDiscount = purchase.SoldOrReturned*purchaseLineTotal/
                                                            purchase.PurchaseTransaction.GrossTotal*
                                                            purchase.PurchaseTransaction.Discount;
                        var fractionOfTransactionTax = purchase.SoldOrReturned*purchaseLineTotal/
                                                       purchase.PurchaseTransaction.GrossTotal*
                                                       purchase.PurchaseTransaction.Tax;
                        amount += purchase.SoldOrReturned*purchaseLineTotal - fractionOfTransactionDiscount +
                                  fractionOfTransactionTax;
                    }
                }
            }

            return amount;
        }
    }
}
