using PutraJayaNT.ViewModels;
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
            var vm = new InventoryVM();
            DataContext = vm;
        }
    }
}
