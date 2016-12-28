namespace ECERP.Views.Accounting
{
    using System.Windows.Controls;
    using ViewModels.Accounting;

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
