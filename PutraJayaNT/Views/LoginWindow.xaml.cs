namespace ECRP.Views
{
    using System.Windows.Input;
    using ViewModels;

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginVM();
            DataContext = vm;
            FocusManager.SetFocusedElement(this, UsernameBox);
        }
    }
}
