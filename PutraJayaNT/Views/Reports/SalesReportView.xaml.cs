using PutraJayaNT.ViewModels.Reports;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
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
    }
}
