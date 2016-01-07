using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.ViewModels.Customers;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for BrowseSalesTransactionsView.xaml
    /// </summary>
    public partial class BrowseSalesTransactionsView : ModernWindow
    {
        public BrowseSalesTransactionsView()
        {
            InitializeComponent();
            var vm = new BrowseSalesTransactionsVM();
            DataContext = vm;
        }

        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
