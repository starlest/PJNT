namespace PutraJayaNT.Views
{
    using ViewModels;

    /// <summary>
    /// Interaction logic for VerificationWindow.xaml
    /// </summary>
    public partial class VerificationWindow
    {
        public VerificationWindow()
        {
            InitializeComponent();
            var vm = new VerificationVM();
            DataContext = vm;
        }
    }
}
