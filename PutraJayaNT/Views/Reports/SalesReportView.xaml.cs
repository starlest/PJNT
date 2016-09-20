namespace ECRP.Views.Reports
{
    using System.Windows.Controls;
    using ViewModels.Reports;

    /// <summary>
    /// Interaction logic for SalesReportView.xaml
    /// </summary>
    public partial class SalesReportView : UserControl
    {
        public SalesReportView()
        {
            InitializeComponent();
            var vm = new SalesReportVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
