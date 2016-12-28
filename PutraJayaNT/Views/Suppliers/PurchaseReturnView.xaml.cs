namespace ECERP.Views.Suppliers
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using ViewModels.Suppliers.PurchaseReturn;

    /// <summary>
    /// Interaction logic for PurchaseReturnView.xaml
    /// </summary>
    public partial class PurchaseReturnView
    {
        public PurchaseReturnView()
        {
            InitializeComponent();
            var vm = new PurchaseReturnVM();
            DataContext = vm;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = PurchaseTransactionIDTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }

        private void PurchaseReturnIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = PurchaseReturnIDTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }
    }
}
