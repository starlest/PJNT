using FirstFloor.ModernUI.Windows.Controls;
using MVVMFramework;
using PutraJayaNT.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    internal class VerificationVM : ViewModelBase
    {
        private string _userName;
        private string _password;
        private ICommand _submitCommand;

        public VerificationVM()
        {
            if (Application.Current.TryFindResource("IsVerified") != null) Application.Current.Resources.Remove("IsVerified");
        }

        public string Username
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value, () => Username); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value, () => Password); }
        }

        public bool CheckForMasterAdmin { get; set; }

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
                        var userFromDatabase = context.Users.SingleOrDefault(user => user.Username.Equals(_userName) && user.Password.Equals(_password) && user.IsAdmin);

                        if (userFromDatabase == null)
                        {
                            MessageBox.Show("Wrong Username or Password.", "Verification Failed", MessageBoxButton.OK);
                            return;
                        }

                        if (CheckForMasterAdmin && !userFromDatabase.CanDeleteInvoice)
                        {
                            MessageBox.Show("You are not authorised to perform this operation!", "Invalid User",
                                MessageBoxButton.OK);
                            return;
                        }

                        // Verification Successful
                        Application.Current.Resources.Add("IsVerified", true);
                        var windows = Application.Current.Windows;
                        foreach (ModernWindow window in windows)
                        {
                            if (window.Title != "Verification") continue;
                            window.Close();
                            return;
                        }
                    }
                }));
            }
        }
    }
}
