using PutraJayaNT.ViewModels;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Master
{
    /// <summary>
    /// Interaction logic for SuppliersView.xaml
    /// </summary>
    public partial class SuppliersView : UserControl
    {
        public SuppliersView()
        {
            InitializeComponent();
            var vm = new MasterSuppliersVM();
            DataContext = vm;
        }
    }
}
