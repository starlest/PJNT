using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.ViewModels.Customers;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for PrintListView.xaml
    /// </summary>
    public partial class PrintListView : ModernWindow
    {
        public PrintListView()
        {
            InitializeComponent();
            var vm = new PrintListVM();
            DataContext = vm;
        }

        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
