namespace ECRP.Views.Test
{
    using System.Windows.Controls;
    using ViewModels.Test;

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
