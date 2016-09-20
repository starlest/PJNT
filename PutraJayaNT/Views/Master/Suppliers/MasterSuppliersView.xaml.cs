namespace ECRP.Views.Master.Suppliers
{
    using System.Windows.Controls;
    using ViewModels.Master.Suppliers;

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
