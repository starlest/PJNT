namespace ECERP.Views
{
    using System.Windows.Input;
    using ViewModels;

    /// <summary>
    /// Interaction logic for VerificationWindow.xaml
    /// </summary>
    public partial class VerificationWindow
    {
        public VerificationWindow(bool checkForMasterAdmin)
        {
            InitializeComponent();
            var vm = new VerificationVM {CheckForMasterAdmin = checkForMasterAdmin};
            DataContext = vm;
            FocusManager.SetFocusedElement(this, UsernameBox);
        }
    }
}
