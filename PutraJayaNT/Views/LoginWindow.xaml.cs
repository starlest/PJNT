namespace ECERP.Views
{
    using System.Collections.Generic;
    using System.Windows.Documents;
    using System.Windows.Input;
    using Models;
    using ViewModels;

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow(List<Server> servers)
        {
            InitializeComponent();
            var vm = new LoginVM(servers);
            DataContext = vm;
            FocusManager.SetFocusedElement(this, UsernameBox);
        }
    }
}
