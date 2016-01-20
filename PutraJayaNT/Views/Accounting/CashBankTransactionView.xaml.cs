using PutraJayaNT.ViewModels.Accounting;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
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
