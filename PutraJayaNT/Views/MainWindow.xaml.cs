namespace ECRP.Views
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Threading;
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
        private bool _performingTelegramBotActions;

        public MainWindow()
        {
            ModernUIHelper.TrySetPerMonitorDpiAware();
            InitializeComponent();
            IsEnabled = false;
            var window = new LoginWindow();
            window.ShowDialog();
            _user = Application.Current.TryFindResource(Constants.CURRENTUSER) as User;

            if (_user == null) Application.Current.Shutdown();

            else
            {
                _selectedServerName = Application.Current.FindResource(Constants.SELECTEDSERVER) as string;
                IsEnabled = true;
                _connectionString = UtilityMethods.GetIpAddress();
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

                SetUpBackgroundWorker();
            }
        }

        #region Helper Methods
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
            AppearanceManager.Current.AccentColor = _selectedServerName.Equals(Constants.MIX) ? Colors.Blue : Colors.Yellow;
        }

        private void SetUpBackgroundWorker()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Dispatcher.Invoke(UpdateTitle);              
                AttemptToSendNotifications();
                TelegramBot.CleanNotifications();
                Thread.Sleep(5000);
            }
        }

        private void UpdateTitle()
        {
            if (HasTitledChanged()) SetTitle();
        }

        private void AttemptToSendNotifications()
        {
            try
            {
                if (!_isServer || _performingTelegramBotActions) return;
                _performingTelegramBotActions = true;
                TelegramBot.CheckUpdates();
                TelegramBot.SendNotifications();
                _performingTelegramBotActions = false;
            }
            catch (Exception)
            {
                _performingTelegramBotActions = false;
            }

        }

        private bool HasTitledChanged()
        {
            var newTitle = _selectedServerName + " - User: " + _user.Username + ", Server: " + _connectionString + ", Date: " + UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy");
            return !newTitle.Equals(Title);
        }

        private void SetTitle()
        {
            Title = _selectedServerName + " - User: " + _user.Username + ", Server: " + _connectionString + ", Date: " + UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy");
        }
        #endregion
    }
}
