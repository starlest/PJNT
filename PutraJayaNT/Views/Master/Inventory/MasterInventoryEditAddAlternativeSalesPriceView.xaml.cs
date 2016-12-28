namespace ECERP.Views.Master.Inventory
{
    using ViewModels.Master.Inventory;

    /// <summary>
    /// Interaction logic for MasterInventoryEditAddAlternativeSalesPriceView.xaml
    /// </summary>
    public partial class MasterInventoryEditAddAlternativeSalesPriceView
    {
        public MasterInventoryEditAddAlternativeSalesPriceView(MasterInventoryEditAddAlternativeSalesPriceVM vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.CloseWindow = Close;
        }

        private void Cancel_Button_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
