namespace ECRP.Views.Master.Suppliers
{
    using System.Windows;
    using ViewModels.Master.Suppliers;

    /// <summary>
    /// Interaction logic for MasterSuppliersEditView.xaml
    /// </summary>
    public partial class MasterSuppliersEditView
    {
        public MasterSuppliersEditView(MasterSuppliersEditVM vm)
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
