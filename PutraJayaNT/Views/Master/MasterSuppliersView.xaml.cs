using PutraJayaNT.ViewModels.Master;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Master
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
