using PutraJayaNT.ViewModels.Inventory;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT.Views.Inventory
{
    /// <summary>
    /// Interaction logic for DecreaseStockView.xaml
    /// </summary>
    public partial class MoveStockView : UserControl
    {
        public MoveStockView()
        {
            InitializeComponent();
            var vm = new MoveStockVM();
            DataContext = vm;
        }
    }
}
