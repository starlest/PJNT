namespace PutraJayaNT.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using FirstFloor.ModernUI;
    using FirstFloor.ModernUI.Presentation;
    using Models;
    using Telegram.Bot;
    using Utilities;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _connectionString;
        private readonly User _user;
        private readonly string _selectedServerName;
        private bool _isServer;

        public MainWindow()
        {
            ModernUIHelper.TrySetPerMonitorDpiAware();
            InitializeComponent();
            IsEnabled = false;
            var window = new LoginWindow();
            window.ShowDialog();
            _user = Application.Current.TryFindResource("CurrentUser") as User;

            if (_user == null) Application.Current.Shutdown();

            else
            {
                _selectedServerName = Application.Current.FindResource("SelectedServer") as string;
                IsEnabled = true;
                _connectionString =
                    ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString.Substring(7).Split(';')[0];
                CheckIfIsServer();
                SetColorTheme();

                if (_user.Username.Equals("edwin92"))
                {
                    var linkGroup = new LinkGroup {DisplayName = "Admin"};
                    var link = new Link
                    {
                        DisplayName = "Test",
                        Source = new Uri("/Views/Test/TestView.xaml", UriKind.Relative)
                    };
                    linkGroup.Links.Add(link);
                    MenuLinkGroups.Add(linkGroup);
                }

                if (!_user.IsAdmin)
                {
                    MenuLinkGroups[1].Links.Clear();
                    var link = new Link
                    {
                        DisplayName = "Inventory",
                        Source = new Uri("/Views/Master/Inventory/MasterInventoryView.xaml", UriKind.Relative)
                    };
                    MenuLinkGroups[1].Links.Add(link);
                }
                RunUpdateTitleLoop();
            }
        }

        private void CheckIfIsServer()
        {
            var hostName = Dns.GetHostName(); // Retrive the Name of HOST
            #pragma warning disable 618
            var host = Dns.GetHostByName(hostName);
            foreach (var ip in host.AddressList)
            {
                if (ip.ToString().Equals(_connectionString))
                    _isServer = true;
            }
        }

        private void SetColorTheme()
        {
            if (_selectedServerName.Equals("Mix"))
                AppearanceManager.Current.AccentColor = Colors.Blue;
            else
                AppearanceManager.Current.AccentColor = Colors.Yellow;
        }

        private void RunUpdateTitleLoop()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        private bool _isSendingNotifications = false;

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        SetTitle();
                        if (_isServer && !_isSendingNotifications)
                        {
                            _isSendingNotifications = true;
                            SendNotifications();
                            _isSendingNotifications = false;
                        }
                    }
                    catch (Exception)
                    {
                        _isSendingNotifications = false;
                    }
                });
                Thread.Sleep(5000);
            }
        }

        private void SetTitle()
        {
            Title = _selectedServerName + " - User: " + _user.Username + ", Server: " + _connectionString + ", Date: " + UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy");
        }

        private static void SendNotifications()
        {
            var Bot = new Api("229513906:AAH5-4dU6h_BnI20CpY_X0XAm4xB9xrnvdw");

            List<TelegramBotNotification> unsentNotifications;
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                unsentNotifications =
                    context.TelegramBotNotifications.Where(notification => !notification.Sent).ToList();
                if (unsentNotifications.Count == 0) return;
            }

            foreach (var notification in unsentNotifications)
            {
                var result = Bot.SendTextMessage(-104676249,
                    $"{notification.When} - {notification.Message}");
            }

            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                unsentNotifications =
                    context.TelegramBotNotifications.Where(notification => !notification.Sent).ToList();
                foreach (var notification in unsentNotifications)
                    notification.Sent = true;
                context.SaveChanges();
            }
        }
    }
}
