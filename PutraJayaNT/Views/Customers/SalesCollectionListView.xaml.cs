using System.Windows.Controls;
using PutraJayaNT.ViewModels.Sales;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for SalesCollectionListView.xaml
    /// </summary>
    public partial class SalesCollectionListView : UserControl
    {
        public SalesCollectionListView()
        {
            InitializeComponent();
            var vm = new SalesCollectionListVM();
            DataContext = vm;
        }
    }
}
