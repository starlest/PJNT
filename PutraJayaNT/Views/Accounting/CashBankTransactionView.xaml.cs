namespace ECERP.Views.Accounting
{
    using System.Windows.Controls;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for BankTransactionView.xaml
    /// </summary>
    public partial class CashBankTransactionView : UserControl
    {
        public CashBankTransactionView()
        {
            InitializeComponent();
            var vm = new CashBankTransactionVM();
            DataContext = vm;
        }
    }
}
