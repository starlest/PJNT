using PutraJayaNT.ViewModels.Suppliers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Suppliers
{
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
