using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
    /// <summary>
    /// Interaction logic for OverallSalesReportView.xaml
    /// </summary>
    public partial class OverallSalesReportView : UserControl
    {
        public OverallSalesReportView()
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
