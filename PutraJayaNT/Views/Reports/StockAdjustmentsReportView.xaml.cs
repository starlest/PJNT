namespace ECERP.Views.Reports
{
    using System.Windows.Controls;
    using ViewModels.Reports;

    /// <summary>
    /// Interaction logic for StockAdjustmentsReportView.xaml
    /// </summary>
    public partial class StockAdjustmentsReportView : UserControl
    {
        public StockAdjustmentsReportView()
        {
            InitializeComponent();
            var vm = new StockAdjustmentsReportVM();
            DataContext = vm;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
