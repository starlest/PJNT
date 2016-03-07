using PutraJayaNT.ViewModels.Master.Inventory;

namespace PutraJayaNT.Views.Master.Inventory
{
    /// <summary>
    /// Interaction logic for MasterInventoryEditAddAlternativeSalesPriceView.xaml
    /// </summary>
    public partial class MasterInventoryEditAddAlternativeSalesPriceView
    {
        public MasterInventoryEditAddAlternativeSalesPriceView(MasterInventoryEditAddAlternativeSalesPriceVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Cancel_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
