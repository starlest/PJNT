namespace ECRP.Views.Analysis
{
    using System.Windows.Controls;
    using ViewModels.Analysis;

    /// <summary>
    /// Interaction logic for SalesAnalysisMonthlyView.xaml
    /// </summary>
    public partial class SalesAnalysisMonthlyView
    {
        public SalesAnalysisMonthlyView()
        {
            InitializeComponent();
            var vm = new SalesAnalysisMonthlyVM();
            DataContext = vm;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        }
    }
}
