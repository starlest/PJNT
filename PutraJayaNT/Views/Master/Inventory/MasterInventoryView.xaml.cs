using System.Windows.Controls;
using PutraJayaNT.ViewModels.Master.Inventory;

namespace PutraJayaNT.Views.Master.Inventory
{
    /// <summary>
    /// Interaction logic for MasterInventoryView.xaml
    /// </summary>
    public partial class MasterInventoryView
    {
        public MasterInventoryView()
        {
            InitializeComponent();
            var vm = new MasterInventoryVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
