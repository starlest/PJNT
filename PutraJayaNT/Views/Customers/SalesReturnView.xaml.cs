namespace ECRP.Views.Customers
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using ViewModels.Customers.SalesReturn;

    /// <summary>
    /// Interaction logic for SalesReturnView.xaml
    /// </summary>
    public partial class SalesReturnView
    {
        public SalesReturnView()
        {
            InitializeComponent();
            var vm = new SalesReturnVM();
            DataContext = vm;
        }

        private void SalesIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = SalesIDTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }

        private void SalesReturnIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = SalesReturnIDTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }
    }
}
