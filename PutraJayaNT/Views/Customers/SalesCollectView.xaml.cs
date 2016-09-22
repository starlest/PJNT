namespace ECRP.Views.Customers
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using ViewModels.Customers;

    /// <summary>
    /// Interaction logic for SalesCollectView.xaml
    /// </summary>
    public partial class SalesCollectView
    {
        public SalesCollectView()
        {
            InitializeComponent();
            var vm = new SalesCollectVM();
            DataContext = vm;
        }

        private void PaymentModeComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;
            var ct = cb.Template;
            var pop = ct.FindName("PART_Popup", cb) as Popup;
            if (pop != null) pop.Placement = PlacementMode.Top;
        }
    }
}
