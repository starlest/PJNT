using PutraJayaNT.ViewModels.Suppliers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Suppliers
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView()
        {
            InitializeComponent();
            var vm = new PurchasePaymentVM();
            DataContext = vm;
        }
    }
}
