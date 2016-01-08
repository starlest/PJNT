using PutraJayaNT.ViewModels.Suppliers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Suppliers
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
