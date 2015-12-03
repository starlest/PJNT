using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for SalesReturnView.xaml
    /// </summary>
    public partial class SalesReturnView : UserControl
    {
        public SalesReturnView()
        {
            InitializeComponent();
            var vm = new SalesReturnTransactionVM();
            DataContext = vm;
        }
    }
}
