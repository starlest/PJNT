namespace ECRP.Views.Accounting
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for IncomeStatementView.xaml
    /// </summary>
    public partial class IncomeStatementView
    {
        public IncomeStatementView()
        {
            InitializeComponent();
            var vm = new IncomeStatementVM();
            DataContext = vm;
        }

        private void YearTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var exp = YearTextBox.GetBindingExpression(TextBox.TextProperty);
            exp?.UpdateSource();
        }
    }
}
