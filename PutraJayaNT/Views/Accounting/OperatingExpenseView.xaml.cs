using PutraJayaNT.ViewModels.Accounting;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for OperationsExpenseView.xaml
    /// </summary>
    public partial class OperatingExpenseView : UserControl
    {
        public OperatingExpenseView()
        {
            InitializeComponent();
            var vm = new OperatingExpenseVM();
            DataContext = vm;
        }
    }
}
