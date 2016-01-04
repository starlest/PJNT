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

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
