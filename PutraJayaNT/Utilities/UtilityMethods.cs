namespace ECERP.Utilities
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using Models;
    using Models.Inventory;
    using Views;

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
            using (var context = createContext())
            {
                var stock =
                    context.Stocks.SingleOrDefault(
                        e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));
                return stock?.Pieces ?? 0;
            }
        }

        public static DateTime GetCurrentDate()
        {
            using (var context = createContext())
            {
                var dateTimeString = context.SystemParameters.Single(param => param.Key.Equals(Constants.CURRENTDATE)).Value;
                var date = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture);
                return date;
            }
        }

        public static void advanceSystemDate(int numOfDays)
        {
            using (var context = createContext())
            {
                var dateParameter = context.SystemParameters.Single(param => param.Key.Equals(Constants.CURRENTDATE));
                var date = DateTime.Parse(dateParameter.Value, CultureInfo.InvariantCulture);
                date = date.AddDays(numOfDays);
                dateParameter.Value = date.ToString(CultureInfo.InvariantCulture);
                context.SaveChanges();
            }
        }

        public static string GetDBName() => ((Server) Application.Current.TryFindResource(Constants.SELECTEDSERVER)).DatabaseName;

        public static ERPInitialContext createInitialContext() => new ERPInitialContext(GetIpAddress());

        public static ERPContext createContext() => new ERPContext(GetDBName(), GetIpAddress());

        public static string GetIpAddress() => Application.Current.FindResource(Constants.IPADDRESS) as string;

        public static string GetServerName() => ((Server) Application.Current.TryFindResource(Constants.SELECTEDSERVER)).ServerName;

        public static string GetTelegramKey() => Application.Current.FindResource(Constants.TELEGRAMKEY) as string;

        public static string GetTelegramChatID() => Application.Current.FindResource(Constants.TELEGRAMCHATID) as string;

        public static string GetThemeColor() => Application.Current.FindResource(Constants.THEMECOLOR) as string;
    }
}