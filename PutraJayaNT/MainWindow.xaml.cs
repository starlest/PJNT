using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows.Controls;
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
            ModernUIHelper.TrySetPerMonitorDpiAware();
            InitializeComponent();
            this.IsEnabled = false;
            var window = new LoginWindow();
            window.ShowDialog();
            var user = App.Current.TryFindResource("CurrentUser");
            if (user != null) this.IsEnabled = true;
            else App.Current.MainWindow.Close();

            string constring = ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString;
            this.Title = "Putra Jaya " + constring.Substring(7, 13);
        }
    }
}
