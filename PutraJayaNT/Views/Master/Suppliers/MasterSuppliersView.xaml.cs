using System.Windows.Controls;
using PutraJayaNT.ViewModels.Master;
using PutraJayaNT.ViewModels.Master.Suppliers;

namespace PutraJayaNT.Views.Master.Suppliers
{
    /// <summary>
    /// Interaction logic for MasterSuppliersView.xaml
    /// </summary>
    public partial class MasterSuppliersView : UserControl
    {
        public MasterSuppliersView()
        {
            InitializeComponent();
            var vm = new MasterSuppliersVM();
            DataContext = vm;
        }
    }
}
