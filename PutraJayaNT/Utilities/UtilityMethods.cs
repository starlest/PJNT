namespace PutraJayaNT.Utilities
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
            var window = new VerificationWindow(false);
            window.ShowDialog();
            Application.Current.MainWindow.IsEnabled = true;
            var isVerified = Application.Current.TryFindResource(Constants.ISVERIFIED);
            return isVerified != null;
        }

        public static bool GetMasterAdminVerification()
        {
            Application.Current.MainWindow.IsEnabled = false;
            var window = new VerificationWindow(true);
            window.ShowDialog();
            Application.Current.MainWindow.IsEnabled = true;
            var isVerified = Application.Current.TryFindResource(Constants.ISVERIFIED);
            return isVerified != null;
        }

        public static int GetRemainingStock(Item item, Warehouse warehouse)
        {
            using (var context = new ERPContext(GetDBName(), GetIpAddress()))
            {
                var stock = context.Stocks.SingleOrDefault(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));
                return stock?.Pieces ?? 0;
            }
        }

        public static DateTime GetCurrentDate()
        {
            using (var context = new ERPContext(GetDBName(), GetIpAddress()))
                return context.Dates.First(e => e.Name.Equals("Current")).DateTime;          
        }

        public static string GetDBName()
        {
           var selectedServerName = Application.Current.FindResource(Constants.SELECTEDSERVER) as string;
            if (selectedServerName != null && selectedServerName.Equals(Constants.MIX))
                return "putrajayant";
            return "pjnestle";
        }

        public static string GetIpAddress() => Application.Current.FindResource(Constants.IPADDRESS) as string;
    }
}
