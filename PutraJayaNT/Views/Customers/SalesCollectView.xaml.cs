namespace ECRP.Views.Customers
{
    using System.Windows.Controls;
    using ViewModels.Customers;

    /// <summary>
    /// Interaction logic for SalesCollectView.xaml
    /// </summary>
    public partial class SalesCollectView : UserControl
    {
        public SalesCollectView()
        {
            InitializeComponent();
            var vm = new SalesCollectVM();
            DataContext = vm;
        }
    }
}
