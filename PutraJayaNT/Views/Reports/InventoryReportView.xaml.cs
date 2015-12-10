using PutraJayaNT.ViewModels.Reports;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
    /// <summary>
    /// Interaction logic for InventoryView.xaml
    /// </summary>
    public partial class InventoryReportView : UserControl
    {
        public InventoryReportView()
        { 
            InitializeComponent();
            var vm = new InventoryReportVM();
            DataContext = vm;
        }
    }
}
