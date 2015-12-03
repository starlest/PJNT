using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
    /// <summary>
    /// Interaction logic for SalesReturnsReportView.xaml
    /// </summary>
    public partial class SalesReturnsReportView : UserControl
    {
        public SalesReturnsReportView()
        {
            InitializeComponent();
            var vm = new SalesReturnsReportVM();
            DataContext = vm;
        }
    }
}
