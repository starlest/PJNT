using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for GeneralLedgerView.xaml
    /// </summary>
    public partial class GeneralLedgerView : UserControl
    {
        public GeneralLedgerView()
        {
            InitializeComponent();
            var vm = new GeneralLedgerVM();
            DataContext = vm;
        }
    }
}
