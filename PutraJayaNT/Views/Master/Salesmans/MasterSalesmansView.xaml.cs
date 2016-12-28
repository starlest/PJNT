namespace ECERP.Views.Master.Salesmans
{
    using ViewModels.Master.Salesmans;

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
