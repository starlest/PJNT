namespace ECRP.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using FirstFloor.ModernUI.Windows.Controls;
    using MVVMFramework;
    using Utilities;

    internal class LoginVM : ViewModelBase
    {
        private string _userName;
        private string _password;
        private string _selectedServer;
        private string _ipAddress = "192.168.1.113";
        private ICommand _loginCommand;

        public LoginVM()
        {
            Servers = new ObservableCollection<string> {Constants.NESTLE, " "};
            SelectedServer = Servers.First();
            Application.Current.Resources.Add(Constants.IPADDRESS, _ipAddress);
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

        public ObservableCollection<string> Servers { get; }

        public string SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                if (value.Equals(" ")) value = Constants.MIX;
                SetProperty(ref _selectedServer, value, () => SelectedServer);
                Application.Current.Resources.Remove(Constants.SELECTEDSERVER);
                Application.Current.Resources.Add(Constants.SELECTEDSERVER, _selectedServer);
            }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                SetProperty(ref _ipAddress, value, () => IpAddress);
                Application.Current.Resources.Remove(Constants.IPADDRESS);
                Application.Current.Resources.Add(Constants.IPADDRESS, _ipAddress);
            }
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

                    var context = new ERPContext(UtilityMethods.GetDBName(), _ipAddress);
                    try
                    {
                        var user =
                            context.Users.FirstOrDefault(
                                e => e.Username.Equals(_userName) && e.Password.Equals(_password));

                        if (user == null)
                        {
                            MessageBox.Show("Wrong Username or Password", "Login Failed", MessageBoxButton.OK);
                            return;
                        }

                        // Login Successful
                        Application.Current.Resources.Add("CurrentUser", user);
                        var windows = Application.Current.Windows;
                        foreach (var window in windows.Cast<ModernWindow>().Where(window => window.Title == "Login"))
                        {
                            window.Close();
                            return;
                        }
                    }
                    catch (Exception e)
                    { 
                        MessageBox.Show($"Failed to connect to server. {e}", "Connection Failure", MessageBoxButton.OK);
                    }
                    finally
                    {
                        context.Dispose();
                    }
                }));
            }
        }
    }
}