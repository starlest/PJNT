namespace ECERP.Views.Master.Inventory
{
    using System.Windows;
    using ViewModels.Master.Inventory;

    /// <summary>
    /// Interaction logic for MasterInventoryEditView.xaml
    /// </summary>
    public partial class MasterInventoryEditView
    {
        public MasterInventoryEditView(MasterInventoryEditVM vm)
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
