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

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
