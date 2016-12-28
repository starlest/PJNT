namespace ECERP.Views.Inventory
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using ViewModels.Inventory;

    /// <summary>
    /// Interaction logic for  StockAdjustmentView.xaml
    /// </summary>
    public partial class StockAdjustmentView
    {
        public StockAdjustmentView()
        {
            InitializeComponent();
            var vm = new StockAdjustmentVM();
            DataContext = vm;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = TransactionIDTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }
    }
}
