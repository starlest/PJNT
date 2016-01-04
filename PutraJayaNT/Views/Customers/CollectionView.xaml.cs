using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for CollectionView.xaml
    /// </summary>
    public partial class CollectionView : UserControl
    {
        public CollectionView()
        {
            InitializeComponent();
            var vm = new CollectionVM();
            DataContext = vm;
        }
    }
}
