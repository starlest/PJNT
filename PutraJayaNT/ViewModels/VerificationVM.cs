using FirstFloor.ModernUI.Windows.Controls;
using MVVMFramework;
using PutraJayaNT.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    class VerificationVM : ViewModelBase
    {
        string _userName;
        string _password;

        ICommand _submitCommand;

        public VerificationVM()
        {
            if (App.Current.TryFindResource("IsVerified") != null) App.Current.Resources.Remove("IsVerified");
        }

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

        public ICommand SubmitCommand
        {
            get
            {
                return _submitCommand ?? (_submitCommand = new RelayCommand(() =>
                {
                    if (_userName == null || _password == null)
                    {
                        MessageBox.Show("Please enter all fields.", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    using (var context = new ERPContext(UtilityMethods.GetDBName()))
                    {
                        var user = context.Users.Where(e => e.Username.Equals(_userName) && e.Password.Equals(_password) && e.IsAdmin.Equals(true)).FirstOrDefault();

                        if (user == null)
                        {
                            MessageBox.Show("Wrong Username or Password", "Verification Failed", MessageBoxButton.OK);
                            return;
                        }

                        // Verification Successful
                        App.Current.Resources.Add("IsVerified", true);
                        var windows = App.Current.Windows;
                        foreach (ModernWindow window in windows)
                        {
                            if (window.Title == "Verification")
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
