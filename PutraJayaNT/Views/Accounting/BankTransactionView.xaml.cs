using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for BankTransactionView.xaml
    /// </summary>
    public partial class BankTransactionView : UserControl
    {
        public BankTransactionView()
        {
            InitializeComponent();
            var vm = new BankTransactionVM();
            DataContext = vm;
        }
    }
}
