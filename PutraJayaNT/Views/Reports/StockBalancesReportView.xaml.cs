using PutraJayaNT.ViewModels.Reports;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
    /// <summary>
    /// Interaction logic for StockBalancesReportView.xaml
    /// </summary>
    public partial class StockBalancesReportView : UserControl
    {
        public StockBalancesReportView()
        {
            InitializeComponent();
            var vm = new StockBalancesReportVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
