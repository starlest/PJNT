using PutraJayaNT.ViewModels.Master.Salesmans;

namespace PutraJayaNT.Views.Master.Salesmans
{
    /// <summary>
    /// Interaction logic forMasterSalesmanView.xaml
    /// </summary>
    public partial class MasterSalesmansView
    {
        public MasterSalesmansView()
        {
            InitializeComponent();
            var vm = new MasterSalesmansVM();
            DataContext = vm;
        }
    }
}
