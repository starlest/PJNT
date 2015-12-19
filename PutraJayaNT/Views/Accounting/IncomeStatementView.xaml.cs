using PutraJayaNT.ViewModels.Accounting;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
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
