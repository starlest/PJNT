namespace ECERP.Views.Reports
{
    using System.Windows.Controls;
    using ViewModels.Reports;

    /// <summary>
    /// Interaction logic for StockBalancesReportView.xaml
    /// </summary>
    public partial class StockCardReportView
    {
        public StockCardReportView()
        {
            InitializeComponent();
            var vm = new StockCardReportVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
