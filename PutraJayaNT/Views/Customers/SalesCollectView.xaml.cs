using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for SalesCollectView.xaml
    /// </summary>
    public partial class SalesCollectView : UserControl
    {
        public SalesCollectView()
        {
            InitializeComponent();
            var vm = new SalesCollectVM();
            DataContext = vm;
        }
    }
}
