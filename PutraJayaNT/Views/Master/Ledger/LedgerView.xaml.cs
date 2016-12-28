namespace ECERP.Views.Master.Ledger
{
    using ViewModels.Master.Ledger;

    /// <summary>
    /// Interaction logic for LedgerView.xaml
    /// </summary>
    public partial class LedgerView
    {
        public LedgerView()
        {
            InitializeComponent();
            var vm = new MasterLedgerVM();
            DataContext = vm;
        }
    }
}
