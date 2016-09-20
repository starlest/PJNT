namespace ECRP.Views.Master.Salesmans
{
    using FirstFloor.ModernUI.Windows.Controls;
    using ViewModels.Master.Salesmans;

    /// <summary>
    /// Interaction logic for MasterSalesmansEditView.xaml
    /// </summary>
    public partial class MasterSalesmansEditView : ModernWindow
    {
        public MasterSalesmansEditView(MasterSalesmansEditVM vm)
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
