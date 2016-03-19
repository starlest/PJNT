using PutraJayaNT.ViewModels.Suppliers;

namespace PutraJayaNT.Views.Suppliers
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PurchasePaymentView
    {
        public PurchasePaymentView()
        {
            InitializeComponent();
            var vm = new PurchasePaymentVM();
            DataContext = vm;
        }
    }
}
