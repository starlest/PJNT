using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;
using PutraJayaNT.ViewModels.Master.Suppliers;

namespace PutraJayaNT.Views.Master.Suppliers
{
    /// <summary>
    /// Interaction logic for MasterSuppliersEditView.xaml
    /// </summary>
    public partial class MasterSuppliersEditView
    {
        public MasterSuppliersEditView(MasterSuppliersEditVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Cancel_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
