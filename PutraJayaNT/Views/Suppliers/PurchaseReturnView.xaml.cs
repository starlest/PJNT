using PutraJayaNT.ViewModels.Suppliers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT.Views.Suppliers
{
    /// <summary>
    /// Interaction logic for PurchaseReturnView.xaml
    /// </summary>
    public partial class PurchaseReturnView : UserControl
    {
        public PurchaseReturnView()
        {
            InitializeComponent();
            var vm = new PurchaseReturnTransactionVM();
            DataContext = vm;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = PurchaseTransactionIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }
    }
}
