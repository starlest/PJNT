namespace ECERP.Views.Inventory
{
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using ViewModels.Inventory;

    /// <summary>
    /// Interaction logic for DecreaseStockView.xaml
    /// </summary>
    public partial class StockMovementView : UserControl
    {
        public StockMovementView()
        {
            InitializeComponent();
            var vm = new StockMovementVM();
            DataContext = vm;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = TransactionIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

    }
}
