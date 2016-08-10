using System.Windows;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using PutraJayaNT.ViewModels.Master.Customers;

namespace PutraJayaNT.Views.Master.Customers
{
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
