namespace PutraJayaNT.Views
{
    using FirstFloor.ModernUI.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : ModernWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginVM();
            DataContext = vm;
        }
    }
}
