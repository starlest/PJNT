using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.Models;
using System;
using System.Configuration;

namespace PutraJayaNT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            //AppearanceManager.Current.AccentColor = Colors.Black;
            ModernUIHelper.TrySetPerMonitorDpiAware();
            InitializeComponent();
            this.IsEnabled = false;
            var window = new LoginWindow();
            window.ShowDialog();
            var user = App.Current.TryFindResource("CurrentUser") as User;
            if (user != null) this.IsEnabled = true;
            else App.Current.Shutdown();

            var connectionString = ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString.Substring(7).Split(';')[0];
            this.Title = "Putra Jaya - User: " + user.Username + ", Server: " + connectionString;

            if (user.IsAdmin)
            {
                var linkGroup = new LinkGroup();
                linkGroup.DisplayName = "Admin";

                var link = new Link { DisplayName = "Test", Source = new Uri("/Views/Test/TestView.xaml", UriKind.Relative) };
                linkGroup.Links.Add(link);
                this.MenuLinkGroups.Add(linkGroup);
            }
        }
    }
}
