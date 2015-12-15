using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView()
        {
            InitializeComponent();
            var vm = new SalesPaymentVM();
            DataContext = vm;
        }
    }
}
