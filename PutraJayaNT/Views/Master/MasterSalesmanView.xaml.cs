using PutraJayaNT.ViewModels.Master;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Master
{
    /// <summary>
    /// Interaction logic forMasterSalesmanView.xaml
    /// </summary>
    public partial class MasterSalesmanView : UserControl
    {
        public MasterSalesmanView()
        {
            InitializeComponent();
            var vm = new MasterSalesmanVM();
            DataContext = vm;
        }
    }
}
