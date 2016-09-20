namespace ECRP.Views.Master
{
    using System.Windows.Controls;
    using ViewModels.Master;

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
