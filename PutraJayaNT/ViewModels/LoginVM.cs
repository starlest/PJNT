using FirstFloor.ModernUI.Windows.Controls;
using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    class LoginVM : ViewModelBase
    {
        string _userName;
        string _password;

        ICommand _loginCommand;

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
                        var user = context.Users.Where(e => e.Username.Equals(_userName) && e.Password.Equals(_password)).FirstOrDefault();

                        if (user == null)
                        {
                            MessageBox.Show("Wrong Username or Password", "Login Failed", MessageBoxButton.OK);
                            return;
                        }

                        // Login Successful
                        App.Current.Resources.Add("CurrentUser", user);
                        var windows = App.Current.Windows;
                        foreach (ModernWindow window in windows)
                        {
                            if (window.Title == "Login")
                            {
                                window.Close();
                                return;
                            }
                        }
                    }
                }));
            }
        }
    }
}
