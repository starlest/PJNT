﻿using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace PutraJayaNT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        string _connectionString;
        User _user;

        public MainWindow()
        {
            //AppearanceManager.Current.AccentColor = Colors.Black;
            ModernUIHelper.TrySetPerMonitorDpiAware();
            InitializeComponent();
            this.IsEnabled = false;
            var window = new LoginWindow();
            window.ShowDialog();
            _user = App.Current.TryFindResource("CurrentUser") as User;
            if (_user != null) this.IsEnabled = true;
            else App.Current.Shutdown();

            _connectionString = ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString.Substring(7).Split(';')[0];
            SetTitle();

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
                MenuLinkGroups.Remove(MenuLinkGroups[1]);
            }

            if (_user.ViewOnly)
            {
                MenuLinkGroups.Clear();
                var linkGroup = new LinkGroup();
                linkGroup.DisplayName = "Reports";
                var link1 = new Link { DisplayName = "Inventory", Source = new Uri("/Views/Reports/InventoryReportView.xaml", UriKind.Relative) };
                var link2 = new Link { DisplayName = "Sales", Source = new Uri("/Views/Reports/OverallSalesReportView.xaml", UriKind.Relative) };
                linkGroup.Links.Add(link1);
                linkGroup.Links.Add(link2);
                this.MenuLinkGroups.Add(linkGroup);
            }

            // Set up database listener for changes in the program's date
      
                //using (var context = new ERPContext())
                //{
                //    var iquery = from x in context.Dates
                //                 where x.Name.Equals("Current")
                //                 select x;

                //    var listener = new DatabaseListener<Date>(context, iquery, 5000);
                //    listener.OnChanged += NotificationOnChanged;
                //}


            //var worker = new BackgroundWorker();
            //worker.DoWork += worker_DoWork;
            //worker.RunWorkerAsync();
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(1000);
                this.Dispatcher.Invoke((Action)(() =>
                {
                    SetTitle();
                }));
            }
        }

        private void SetTitle()
        {
            this.Title = "Putra Jaya - User: " + _user.Username + ", Server: " + _connectionString + ", Date: " + UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy");
        }
    }
}
