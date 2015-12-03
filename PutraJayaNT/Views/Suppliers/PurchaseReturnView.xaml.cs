using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Suppliers
{
    /// <summary>
    /// Interaction logic for PurchaseReturnView.xaml
    /// </summary>
    public partial class PurchaseReturnView : UserControl
    {
        public PurchaseReturnView()
        {
            InitializeComponent();
            var vm = new PurchaseReturnTransactionVM();
            DataContext = vm;
        }
    }
}
