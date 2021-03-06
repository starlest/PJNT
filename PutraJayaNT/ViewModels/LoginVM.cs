﻿namespace ECERP.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using FirstFloor.ModernUI.Windows.Controls;
    using Models;
    using MVVMFramework;
    using Utilities;

    internal class LoginVM : ViewModelBase
    {
        private string _userName;
        private string _password;
        private Server _selectedServer;
        private ICommand _loginCommand;

        public LoginVM(IEnumerable<Server> servers)
        {
            Servers = new ObservableCollection<Server>();
            foreach (var server in servers)
                Servers.Add(server);
            SelectedServer = Servers.First();
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

        public ObservableCollection<Server> Servers { get; }

        public Server SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                SetProperty(ref _selectedServer, value, () => SelectedServer);
                Application.Current.Resources.Remove(Constants.SELECTEDSERVER);
                Application.Current.Resources.Add(Constants.SELECTEDSERVER, _selectedServer);
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

                           var context = new ERPContext(_selectedServer.DatabaseName, UtilityMethods.GetIpAddress());
                           try
                           {
                               var user =
                                   context.Users.SingleOrDefault(
                                       e => e.Username.Equals(_userName) && e.Password.Equals(_password));

                               if (user == null)
                               {
                                   MessageBox.Show("Wrong Username or Password", "Login Failed", MessageBoxButton.OK);
                                   return;
                               }

                               // Login Successful
                               Application.Current.Resources.Add(Constants.CURRENTUSER, user);
                               var windows = Application.Current.Windows;
                               foreach (
                                   var window in windows.Cast<ModernWindow>().Where(window => window.Title == "Login"))
                               {
                                   window.Close();
                                   return;
                               }
                           }
                           catch (Exception e)
                           {
                               MessageBox.Show($"Failed to connect to server. {e}", "Connection Failure",
                                   MessageBoxButton.OK);
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