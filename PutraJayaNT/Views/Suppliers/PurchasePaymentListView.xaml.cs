namespace ECERP.Views.Suppliers
{
    using ViewModels.Suppliers;

    /// <summary>
    /// Interaction logic for CollectionView.xaml
    /// </summary>
    public partial class PurchasePaymentListView
    {
        public PurchasePaymentListView()
        {
            InitializeComponent();
            var vm = new PurchasePaymentListVM();
            DataContext = vm;
        }
    }
}
