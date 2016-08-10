using System.Windows;
using PutraJayaNT.ViewModels.Master.Inventory;

namespace PutraJayaNT.Views.Master.Inventory
{
    /// <summary>
    /// Interaction logic for MasterInventoryEditAddSupplierView.xaml
    /// </summary>
    public partial class MasterInventoryEditAddSupplierView
    {
        public MasterInventoryEditAddSupplierView(MasterInventoryEditAddSupplierVM vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.CloseWindow = Close;
        }

        private void Cancel_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
