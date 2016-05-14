namespace PutraJayaNT.Views.Analysis
{
    using System.Windows.Controls;
    using ViewModels.Analysis;

    /// <summary>
    /// Interaction logic for SalesForecastMonthlyView.xaml
    /// </summary>
    public partial class SalesForecastMonthlyView
    {
        public SalesForecastMonthlyView()
        {
            InitializeComponent();
            var vm = new SalesForecastMonthlyVM();
            DataContext = vm;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        }
    }
}
