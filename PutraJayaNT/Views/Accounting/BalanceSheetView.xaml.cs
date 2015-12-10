using PutraJayaNT.ViewModels.Accounting;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for BalanceSheetView.xaml
    /// </summary>
    public partial class BalanceSheetView : UserControl
    {
        public BalanceSheetView()
        {
            InitializeComponent();
            var vm = new BalanceSheetVM();
            DataContext = vm;
        }
    }
}
