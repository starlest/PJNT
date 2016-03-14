using PutraJayaNT.ViewModels.Customers;
using System.Windows.Controls;
using System.Windows.Input;

namespace PutraJayaNT.Views.Customers
{
    using ViewModels.Customers.Sales;

    /// <summary>
    /// Interaction logic for SalesView.xaml
    /// </summary>
    public partial class SalesView
    {
        public SalesView()
        {
            InitializeComponent();
            var vm = new SalesVM();
            DataContext = vm;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = SalesIDTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }

        private void DiscountPercentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = DiscountPercentTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }

        private void OverallDiscountPercentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = OverallDiscountPercentTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }


        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
