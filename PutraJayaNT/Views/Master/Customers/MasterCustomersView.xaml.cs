using System.Windows.Controls;
using PutraJayaNT.ViewModels.Master.Customers;

namespace PutraJayaNT.Views.Master.Customers
{
    /// <summary>
    /// Interaction logic for MasterCustomersView.xaml
    /// </summary>
    public partial class MasterCustomersView
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
