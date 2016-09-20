namespace ECRP.Views.Master.Customers
{
    using System.Windows;
    using System.Windows.Media;
    using FirstFloor.ModernUI.Presentation;
    using ViewModels.Master.Customers;

    /// <summary>
    /// Interaction logic for MasterCustomersEditView.xaml
    /// </summary>
    public partial class MasterCustomersEditView
    {
        public MasterCustomersEditView(MasterCustomersEditVM vm)
        {
            AppearanceManager.Current.AccentColor = Colors.Blue;
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
