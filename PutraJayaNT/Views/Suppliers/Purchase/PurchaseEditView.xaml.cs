namespace ECRP.Views.Suppliers.Purchase
{
    using ViewModels.Suppliers.Purchase;

    /// <summary>
    /// Interaction logic for PurchaseEditView.xaml
    /// </summary>
    public partial class PurchaseEditView
    {
        public PurchaseEditView(PurchaseEditVM vm)
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
