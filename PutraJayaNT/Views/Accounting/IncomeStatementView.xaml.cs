namespace ECRP.Views.Accounting
{
    using System.Windows.Controls;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for IncomeStatementView.xaml
    /// </summary>
    public partial class IncomeStatementView : UserControl
    {
        public IncomeStatementView()
        {
            InitializeComponent();
            var vm = new IncomeStatementVM();
            DataContext = vm;
        }
    }
}
