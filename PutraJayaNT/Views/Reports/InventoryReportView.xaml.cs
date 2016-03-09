using PutraJayaNT.ViewModels.Reports;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
    /// <summary>
    /// Interaction logic for InventoryView.xaml
    /// </summary>
    public partial class InventoryReportView
    {
        public InventoryReportView()
        { 
            InitializeComponent();
            var vm = new InventoryReportVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
