using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;

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
