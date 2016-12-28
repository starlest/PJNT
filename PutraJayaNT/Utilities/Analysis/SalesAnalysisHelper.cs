namespace ECERP.Utilities.Analysis
{
    using System.Linq;
    using ViewModels.Item;

    internal static class SalesAnalysisHelper
    {
        public static int GetSalesForecastInMonth(ERPContext context, ItemVM item, int year, int month)
        {
            if (month == 1)
            {
                year--;
                month = 13;
            }
            var previousMonthTotalSales = GetItemTotalSalesInMonth(context, item, year, month - 1);
            if (month == 2)
            {
                year--;
                month = 13;
            }
            var secondPreviousMonthTotalSales = GetItemTotalSalesInMonth(context, item, year, month - 2);
            if (month == 3)
            {
                year--;
                month = 13;
            }
            var thirdPreviousMonthTotalSales = GetItemTotalSalesInMonth(context, item, year, month - 3);

            if (previousMonthTotalSales == 0) return 0;
            if (secondPreviousMonthTotalSales == 0) return previousMonthTotalSales;
            return thirdPreviousMonthTotalSales == 0
                ? (previousMonthTotalSales + secondPreviousMonthTotalSales) / 2
                : (previousMonthTotalSales + secondPreviousMonthTotalSales + thirdPreviousMonthTotalSales) / 3;
        }

        public static int GetItemTotalSalesInMonth(ERPContext context, ItemVM item, int year, int month)
        {
            var totalSalesQuantity = 0;

            var monthSalesTransactionLines =
                context.SalesTransactionLines.Where(
                    line => line.SalesTransaction.Date.Year.Equals(year) && line.SalesTransaction.Date.Month.Equals(month) && line.ItemID.Equals(item.ID))
                    .ToList();

            var monthSalesReturnTransactionLines =
                context.SalesReturnTransactionLines.Where(
                    line => line.SalesReturnTransaction.Date.Year.Equals(year) && line.SalesReturnTransaction.Date.Month.Equals(month) && line.ItemID.Equals(item.ID))
                    .ToList();

            foreach (var line in monthSalesTransactionLines)
                totalSalesQuantity += line.Quantity;

            foreach (var line in monthSalesReturnTransactionLines)
                totalSalesQuantity -= line.Quantity;

            return totalSalesQuantity;
        }
    }
}
