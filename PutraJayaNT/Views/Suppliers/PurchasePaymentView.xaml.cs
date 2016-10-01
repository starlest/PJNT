namespace ECRP.Views.Suppliers
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using ViewModels.Suppliers;

    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PurchasePaymentView
    {
        public PurchasePaymentView()
        {
            InitializeComponent();
            var vm = new PurchasePaymentVM();
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