using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for SalesReturnView.xaml
    /// </summary>
    public partial class SalesReturnView : UserControl
    {
        public SalesReturnView()
        {
            InitializeComponent();
            var vm = new SalesReturnTransactionVM();
            DataContext = vm;
        }

        private void SalesIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = SalesIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }

        private void SalesReturnIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = SalesReturnIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }
    }
}
