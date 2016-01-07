using PutraJayaNT.ViewModels.Inventory;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT.Views.Inventory
{
    /// <summary>
    /// Interaction logic for AdjustStockView.xaml
    /// </summary>
    public partial class AdjustStockView : UserControl
    {
        public AdjustStockView()
        {
            InitializeComponent();
            var vm = new AdjustStockTransactionVM();
            DataContext = vm;
        }


        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = TransactionIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }
    }
}
