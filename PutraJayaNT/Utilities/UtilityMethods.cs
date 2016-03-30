﻿namespace PutraJayaNT.Utilities
{
    using Views;
    using Models.Inventory;
    using System;
    using System.Linq;
    using System.Windows;

    public static class UtilityMethods
    {
        public static bool GetVerification()
        {
            Application.Current.MainWindow.IsEnabled = false;
            var window = new VerificationWindow();
            window.ShowDialog();
            Application.Current.MainWindow.IsEnabled = true;
            var isVerified = Application.Current.TryFindResource("IsVerified");
            return isVerified != null;
        }

        public static int GetRemainingStock(Item item, Warehouse warehouse)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var stock = context.Stocks.FirstOrDefault(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));
                return stock?.Pieces ?? 0;
            }
        }

        public static DateTime GetCurrentDate()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
                return context.Dates.First(e => e.Name.Equals("Current")).DateTime;          
        }

        public static void CloseForemostWindow()
        {
            var editWindow = Application.Current.Windows[Application.Current.Windows.Count - 1];
            editWindow?.Close();
        }

        public static string GetDBName()
        {
           var selectedServerName = Application.Current.FindResource("SelectedServer") as string;

            if (selectedServerName != null && selectedServerName.Equals("Mix"))
                return "putrajayant";
            else
                return "pjnestle";
        }
    }
}
