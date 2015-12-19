using PUJASM.ERP.ViewModels.Master;
using System.Windows.Controls;

namespace PUJASM.ERP.Views.Master
{
    /// <summary>
    /// Interaction logic for MasterCustomersView.xaml
    /// </summary>
    public partial class MasterCustomersView : UserControl
    {
        public MasterCustomersView()
        {
            InitializeComponent();
            var vm = new MasterCustomersVM();
            DataContext = vm;
        }
    }
}
