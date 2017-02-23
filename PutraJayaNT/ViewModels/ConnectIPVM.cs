namespace ECERP.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using FirstFloor.ModernUI.Windows.Controls;
    using MVVMFramework;
    using Utilities;
    using Views;

    internal class ConnectIPVM : ViewModelBase
    {
        private string _iPAddress;
        private ICommand _connectCommand;

        public ConnectIPVM()
        {
            _iPAddress = "192.168.100.18";
        }

        public string IPAddress
        {
            get { return _iPAddress; }
            set { SetProperty(ref _iPAddress, value, () => IPAddress); }
        }

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommand(() =>
                {
                    try
                    {
                        var initialContext = new ERPInitialContext(_iPAddress);
                        var servers = initialContext.Servers.ToList();
                        Application.Current.Resources.Add(Constants.IPADDRESS, _iPAddress);
                        var windows = Application.Current.Windows;
                        foreach (var window in windows.Cast<ModernWindow>().Where(window => window.Title == "Connect IP"))
                        {
                            window.Close();
                            var loginWindow = new LoginWindow(servers);
                            loginWindow.ShowDialog();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Unable to connect to IP Address. \n {e}", "Invalid IP Address", MessageBoxButton.OK);
                    }
                }));
            }
        }
    }
}
