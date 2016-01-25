using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels;

namespace PutraJayaNT
{
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
