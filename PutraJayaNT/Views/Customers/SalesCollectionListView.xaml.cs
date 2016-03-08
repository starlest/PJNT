namespace PutraJayaNT.Views.Customers
{
    using ViewModels.Customers;

    /// <summary>
    /// Interaction logic for SalesCollectionListView.xaml
    /// </summary>
    public partial class SalesCollectionListView
    {
        public SalesCollectionListView()
        {
            InitializeComponent();
            var vm = new SalesCollectionListVM();
            DataContext = vm;
        }
    }
}
