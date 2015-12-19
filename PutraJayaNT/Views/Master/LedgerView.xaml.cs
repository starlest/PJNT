using PutraJayaNT.ViewModels.Master;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Master
{
    /// <summary>
    /// Interaction logic for LedgerView.xaml
    /// </summary>
    public partial class LedgerView : UserControl
    {
        public LedgerView()
        {
            InitializeComponent();
            var vm = new MasterLedgerVM();
            DataContext = vm;
        }
    }
}
