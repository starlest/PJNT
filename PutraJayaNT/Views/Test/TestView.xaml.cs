using PutraJayaNT.ViewModels.Test;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Test
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
            var vm = new TestVM();
            DataContext = vm;
        }
    }
}
