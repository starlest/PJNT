using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for CollectionView.xaml
    /// </summary>
    public partial class PaymentListView : UserControl
    {
        public PaymentListView()
        {
            InitializeComponent();
            var vm = new PaymentListVM();
            DataContext = vm;
        }
    }
}
