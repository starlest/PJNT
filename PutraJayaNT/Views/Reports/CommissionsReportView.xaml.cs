namespace ECERP.Views.Reports
{
    using System.Windows.Controls;
    using ViewModels.Reports;

    /// <summary>
    /// Interaction logic for CommissionsReportView.xaml
    /// </summary>
    public partial class CommissionsReportView : UserControl
    {
        public CommissionsReportView()
        {
            InitializeComponent();
            var vm = new CommissionsReportVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
