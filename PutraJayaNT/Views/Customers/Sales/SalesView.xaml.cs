namespace ECRP.Views.Customers.Sales
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
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

        private void ComboBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) MoveToNextUIElement(e);
        }

        static void MoveToNextUIElement(KeyEventArgs e)
        {
            // Creating a FocusNavigationDirection object and setting it to a
            // local field that contains the direction selected.
            const FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;

            // MoveFocus takes a TraveralReqest as its argument.
            var request = new TraversalRequest(focusDirection);

            // Gets the element with keyboard focus.
            var elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus == null) return;
            if (elementWithFocus.MoveFocus(request)) e.Handled = true;
        }
    }
}
