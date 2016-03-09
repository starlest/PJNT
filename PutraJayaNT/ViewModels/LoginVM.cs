using FirstFloor.ModernUI.Windows.Controls;
using MVVMFramework;
using PutraJayaNT.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    internal class LoginVM : ViewModelBase
    {
        private string _userName;
        private string _password;

        private ICommand _loginCommand;

        public string Username
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value, "Username"); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, "Password"); }
        }

        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommand(() =>
                {
                    if (_userName == null || _password == null)
                    {
                        MessageBox.Show("Please enter all fields.", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    using (var context = new ERPContext())
                    {
                        var user = context.Users.FirstOrDefault(e => e.Username.Equals(_userName) && e.Password.Equals(_password));

                        if (user == null)
                        {
                            MessageBox.Show("Wrong Username or Password", "Login Failed", MessageBoxButton.OK);
                            return;
                        }

                        // Login Successful
                        Application.Current.Resources.Add("CurrentUser", user);
                        var windows = Application.Current.Windows;
                        foreach (ModernWindow window in windows.Cast<ModernWindow>().Where(window => window.Title == "Login"))
                        {
                            window.Close();
                            return;
                        }
                    }
                }));
            }
        }
    }
}
