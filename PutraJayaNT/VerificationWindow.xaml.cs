using FirstFloor.ModernUI.Windows.Controls;
using PutraJayaNT.ViewModels;

namespace PutraJayaNT
{
    /// <summary>
    /// Interaction logic for VerificationWindow.xaml
    /// </summary>
    public partial class VerificationWindow : ModernWindow
    {
        public VerificationWindow()
        {
            InitializeComponent();
            var vm = new VerificationVM();
            DataContext = vm;
        }
    }
}
