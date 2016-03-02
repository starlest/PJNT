using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace PutraJayaNT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        readonly string _connectionString;
        readonly User _user;

        public MainWindow()
        {
            AppearanceManager.Current.AccentColor = Colors.Blue;
            ModernUIHelper.TrySetPerMonitorDpiAware();
            InitializeComponent();
            this.IsEnabled = false;
            var window = new LoginWindow();
            window.ShowDialog();
            _user = App.Current.TryFindResource("CurrentUser") as User;
            if (_user != null) this.IsEnabled = true;
            else App.Current.Shutdown();

            _connectionString = ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString.Substring(7).Split(';')[0];

            if (_user.Username.Equals("edwin92"))
            {
                var linkGroup = new LinkGroup();
                linkGroup.DisplayName = "Admin";
                var link = new Link { DisplayName = "Test", Source = new Uri("/Views/Test/TestView.xaml", UriKind.Relative) };
                linkGroup.Links.Add(link);
                this.MenuLinkGroups.Add(linkGroup);
            }

            if (!_user.IsAdmin)
            {
                MenuLinkGroups[1].Links.Clear();
                var link = new Link { DisplayName = "Inventory", Source = new Uri("/Views/Master/MasterInventoryView.xaml", UriKind.Relative) };
                MenuLinkGroups[1].Links.Add(link);
            }

            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(1000);
                this.Dispatcher.Invoke((Action)(() =>
                {
                    var context = new ERPContext();
                    try
                    {
                        SetTitle(context);
                    }
                    catch
                    {

                    }
                }));
            }
        }

        private void SetTitle(ERPContext context)
        {
            this.Title = "Putra Jaya - User: " + _user.Username + ", Server: " + _connectionString + ", Date: " + context.Dates.Where(e => e.Name.Equals("Current")).FirstOrDefault().DateTime.ToString("dd-MM-yyyy");
        }
    }
}
