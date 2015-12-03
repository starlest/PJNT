using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Reports
{
    /// <summary>
    /// Interaction logic for PurchasesReportView.xaml
    /// </summary>
    public partial class PurchasesReportView : UserControl
    {
        PurchasesReportVM vm;

        public PurchasesReportView()
        {
            InitializeComponent();
            vm = new PurchasesReportVM();
            DataContext = vm;
        }
    }
}
