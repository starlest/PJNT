namespace ECRP.Views.Suppliers
{
    using ViewModels.Suppliers;

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
