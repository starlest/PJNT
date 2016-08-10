namespace PutraJayaNT.Views.Customers.Sales
{
    using ViewModels.Customers.Sales;

    /// <summary>
    /// Interaction logic for SalesEditView.xaml
    /// </summary>
    public partial class SalesEditView
    {
        public SalesEditView(SalesEditVM vm)
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
