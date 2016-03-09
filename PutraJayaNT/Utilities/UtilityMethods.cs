using PutraJayaNT.Models.Inventory;
using System;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.Utilities
{
    using Views;

    public static class UtilityMethods
    {
        public static bool GetVerification()
        {
            App.Current.MainWindow.IsEnabled = false;
            var window = new VerificationWindow();
            window.ShowDialog();
            App.Current.MainWindow.IsEnabled = true;
            var isVerified = App.Current.TryFindResource("IsVerified");
            return isVerified != null;
        }

        public static int GetRemainingStock(Item item, Warehouse warehouse)
        {
            using (var context = new ERPContext())
            {
                var stock = context.Stocks.Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID)).FirstOrDefault();

                if (stock == null) return 0;
                else return stock.Pieces;
            }
        }

        public static DateTime GetCurrentDate()
        {
            using (var context = new ERPContext())
                return context.Dates.First(e => e.Name.Equals("Current")).DateTime;          
        }

        public static void CloseForemostWindow()
        {
            var editWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
            editWindow?.Close();
        }
    }
}
