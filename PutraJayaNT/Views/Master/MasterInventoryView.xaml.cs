using PutraJayaNT.ViewModels.Master;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Master
{
    /// <summary>
    /// Interaction logic for MasterInventoryView.xaml
    /// </summary>
    public partial class MasterInventoryView : UserControl
    {
        public MasterInventoryView()
        {
            InitializeComponent();
            var vm = new MasterInventoryVM();
            DataContext = vm;
        }
    }
}
