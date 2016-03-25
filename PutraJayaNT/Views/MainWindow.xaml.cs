namespace PutraJayaNT.Views
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using FirstFloor.ModernUI;
    using FirstFloor.ModernUI.Presentation;
    using Models;
    using Utilities;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _connectionString;
        private readonly User _user;
        private readonly string _selectedServerName;

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


        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(1000);
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        SetTitle();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
            }
        }

        private void SetTitle()
        {
            Title = _selectedServerName + " - User: " + _user.Username + ", Server: " + _connectionString + ", Date: " + UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy");
        }
    }
}
