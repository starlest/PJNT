namespace ECRP.Views.Accounting
{
    using System.Windows.Controls;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for DailyCashFlowView.xaml
    /// </summary>
    public partial class DailyCashFlowView : UserControl
    {
        public DailyCashFlowView()
        {
            InitializeComponent();
            var vm = new DailyCashFlowVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
