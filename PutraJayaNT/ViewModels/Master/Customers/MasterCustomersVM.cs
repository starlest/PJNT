using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using PutraJayaNT.Views.Master.Customers;

namespace PutraJayaNT.ViewModels.Master.Customers
{
    public class MasterCustomersVM : ViewModelBase
    {
        #region Backing Fields
        private bool _isActiveChecked;
        private CustomerGroupVM _selectedGroup;
        private CustomerVM _selectedCustomer;
        private string _selectedCity;
        private CustomerVM _selectedLine;

        private ICommand _searchCommand;
        private ICommand _editCustomerCommand;
        private ICommand _activateCustomerCommand;
        #endregion

        public MasterCustomersVM()
        {
            CustomerGroups = new ObservableCollection<CustomerGroupVM>();
            Customers = new ObservableCollection<CustomerVM>();
            Cities = new ObservableCollection<string>();
            DisplayedCustomers = new ObservableCollection<CustomerVM>();

            UpdateCities();
            UpdateGroups();

            _isActiveChecked = true;
            DisplayAllCustomers();

            NewEntryVM = new MasterCustomersNewEntryVM(this);
        }

        public MasterCustomersNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<CustomerGroupVM> CustomerGroups { get; }

        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<string> Cities { get; }

        public ObservableCollection<CustomerVM> DisplayedCustomers { get; }
        #endregion

        #region Properties
        public bool IsActiveChecked
        {
            get { return _isActiveChecked; }
            set { SetProperty(ref _isActiveChecked, value, "IsActiveChecked"); }
        }

        public CustomerGroupVM SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                SetProperty(ref _selectedGroup, value, "SelectedGroup");

                if (_selectedGroup == null) return;

                SelectedCity = null;
                UpdateListedCustomers();
                SelectedCustomer = Customers.FirstOrDefault();
            }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value, "SelectedCustomer"); }
        }

        public string SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                SetProperty(ref _selectedCity, value, "SelectedCity");

                if (_selectedCity == null) return;

                SelectedGroup = null;
                SelectedCustomer = null;
                Customers.Clear();
            }
        }

        public CustomerVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }
        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(() =>
                {
                    UpdateDisplayedCustomers();
                    UpdateCities();
                }));
            }
        }

        public ICommand EditCustomerCommand
        {
            get
            {
                return _editCustomerCommand ?? (_editCustomerCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected()) return;
                    ShowEditWindow();
                }));
            }
        }

        public ICommand ActivateCustomerCommand
        {
            get
            {
                return _activateCustomerCommand ?? (_activateCustomerCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected()) return;
                    if (_selectedLine.Active) DeactivateCustomerInDatabase(_selectedLine.Model);
                    else ActivateCustomerInDatabase(_selectedLine.Model);
                    UpdateDisplayedCustomers();
                }));
            }
        }
        #endregion

        #region Helper Methods 
        private void UpdateGroups()
        {
            CustomerGroups.Clear();
            CustomerGroups.Add(new CustomerGroupVM { Model = new CustomerGroup { ID = -1, Name = "All" }});
            var groupsReturnedFromDatabase = DatabaseHelper.LoadAllCustomerGroupsFromDatabase();
            foreach (var group in groupsReturnedFromDatabase)
                CustomerGroups.Add(new CustomerGroupVM { Model = group});
        }

        private void UpdateListedCustomers()
        {
            DisplayedCustomers.Clear();
            Customers.Clear();
            Customers.Add(new CustomerVM { Model = new Customer { ID = -1, Name = "All" } });
            var customersReturnedFromDatabase = _selectedGroup.Name.Equals("All") ? DatabaseHelper.LoadAllCustomersFromDatabase() : DatabaseHelper.LoadCustomersFromDatabase(customer => customer.Group.ID.Equals(_selectedGroup.ID));
            foreach (var customer in customersReturnedFromDatabase.OrderBy(customer => customer.Name))
                Customers.Add(new CustomerVM { Model = customer });
        }

        private void UpdateCities()
        {
            var customersReturnedFromDatabase = DatabaseHelper.LoadAllCustomersFromDatabase();
            foreach (var customer in customersReturnedFromDatabase.Where(customer => !Cities.Contains(customer.City)))
                Cities.Add(customer.City);
        }

        public void UpdateDisplayedCustomers()
        {
            DisplayedCustomers.Clear();
            var customersReturnedFromDatabase = LoadCustomersFromDatabaseAccordingToSelections();
            foreach (var customer in customersReturnedFromDatabase.Where(customer => customer.Active.Equals(_isActiveChecked)).OrderBy(customer => customer.Name))
                DisplayedCustomers.Add(new CustomerVM { Model = customer });
        }

        private void DisplayAllCustomers()
        {
            SelectedGroup = CustomerGroups.FirstOrDefault();
            SelectedCustomer = Customers.FirstOrDefault();
            UpdateDisplayedCustomers();
        }

        private static Customer GetCustomerFromDatabaseContext(ERPContext context, Customer searchCustomer)
        {
            return context.Customers.First(customer => customer.ID.Equals(searchCustomer.ID));
        }

        public static void DeactivateCustomerInDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                var customerReturnedFromDatabase = GetCustomerFromDatabaseContext(context, customer);
                customerReturnedFromDatabase.Active = false;
                context.SaveChanges();
            }
        }

        public static void ActivateCustomerInDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                var customerReturnedFromDatabase = GetCustomerFromDatabaseContext(context, customer);
                customerReturnedFromDatabase.Active = true;
                context.SaveChanges();
            }
        }

        private void ShowEditWindow()
        {
            var vm = new MasterCustomersEditVM(_selectedLine);
            var editWindow = new MasterCustomersEditView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editWindow.ShowDialog();
        }

        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "No Selection", MessageBoxButton.OK);
            return false;
        }

        private IEnumerable<Customer> LoadCustomersFromDatabaseAccordingToSelections()
        {
            if (_selectedCity != null) return DatabaseHelper.LoadCustomersFromDatabase(customer => customer.City.Equals(_selectedCity));

            if (_selectedGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
                return DatabaseHelper.LoadAllCustomersFromDatabase();

            if (!_selectedGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
                return DatabaseHelper.LoadCustomersFromDatabase(customer => customer.Group.ID.Equals(_selectedGroup.ID));

            return DatabaseHelper.LoadCustomersFromDatabase(customer => customer.ID.Equals(_selectedCustomer.ID));
        }
        #endregion
    }
}
