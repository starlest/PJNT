namespace ECERP.ViewModels.Master.Customers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using Models.Customer;
    using MVVMFramework;
    using Utilities;
    using Views.Master.Customers;

    public class MasterCustomersVM : ViewModelBase
    {
        #region Backing Fields
        private bool _isActiveChecked;
        private CustomerGroupVM _selectedCustomerGroup;
        private CustomerVM _selectedCustomer;
        private CityVM _selectedCity;
        private CustomerVM _selectedLine;

        private ICommand _searchCommand;
        private ICommand _editCustomerCommand;
        private ICommand _activateCustomerCommand;
        #endregion

        public MasterCustomersVM()
        {
            CustomerGroups = new ObservableCollection<CustomerGroupVM>();
            Customers = new ObservableCollection<CustomerVM>();
            Cities = new ObservableCollection<CityVM>();
            DisplayedCustomers = new ObservableCollection<CustomerVM>();

            UpdateCities();
            UpdateCustomerGroups();

            _isActiveChecked = true;
            SelectedCustomerGroup = CustomerGroups.FirstOrDefault();
            SelectedCustomer = Customers.FirstOrDefault();
            UpdateDisplayedCustomers();

            NewEntryVM = new MasterCustomersNewEntryVM(this);
        }

        public MasterCustomersNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<CustomerGroupVM> CustomerGroups { get; }

        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<CityVM> Cities { get; }

        public ObservableCollection<CustomerVM> DisplayedCustomers { get; }
        #endregion

        #region Properties
        public bool IsActiveChecked
        {
            get { return _isActiveChecked; }
            set { SetProperty(ref _isActiveChecked, value, () => IsActiveChecked); }
        }

        public CustomerGroupVM SelectedCustomerGroup
        {
            get { return _selectedCustomerGroup; }
            set
            {
                SetProperty(ref _selectedCustomerGroup, value,
                    () => SelectedCustomerGroup);

                if (_selectedCustomerGroup == null) return;

                SelectedCity = null;
                UpdateListedCustomers();
                SelectedCustomer = Customers.FirstOrDefault();
            }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value, () => SelectedCustomer); }
        }

        public CityVM SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                SetProperty(ref _selectedCity, value, () => SelectedCity);

                if (_selectedCity == null) return;

                SelectedCustomerGroup = null;
                SelectedCustomer = null;
                Customers.Clear();
            }
        }

        public CustomerVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }
        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ??
                       (_searchCommand = new RelayCommand(() =>
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
                return _editCustomerCommand ??
                       (_editCustomerCommand = new RelayCommand(() =>
                       {
                           if (!IsThereLineSelected()) return;
                           ShowEditWindow();
                           UpdateListedCustomers();
                       }));
            }
        }

        public ICommand ActivateCustomerCommand
        {
            get
            {
                return _activateCustomerCommand ??
                       (_activateCustomerCommand = new RelayCommand(() =>
                       {
                           if (!IsThereLineSelected() || !IsConfirmationYes())
                               return;
                           if (_selectedLine.Active)
                               DeactivateCustomerInDatabase(_selectedLine.Model);
                           else ActivateCustomerInDatabase(_selectedLine.Model);
                           _selectedLine.Active = !_isActiveChecked;
                       }));
            }
        }
        #endregion

        #region Helper Methods 
        private void UpdateCustomerGroups()
        {
            var oldSelectedCustomerGroup = _selectedCustomerGroup;

            CustomerGroups.Clear();
            CustomerGroups.Add(new CustomerGroupVM
            {
                Model = new CustomerGroup { ID = -1, Name = "All" }
            });
            using (var context = UtilityMethods.createContext())
            {
                var groupsReturnedFromDatabase =
                    context.CustomerGroups
                        .OrderBy(customerGroup => customerGroup.Name);
                foreach (var group in groupsReturnedFromDatabase)
                    CustomerGroups.Add(new CustomerGroupVM { Model = group });
            }
            UpdateSelectedCustomerGroup(oldSelectedCustomerGroup);
        }

        private void UpdateSelectedCustomerGroup(
            CustomerGroupVM oldSelectedCustomerGroup)
        {
            if (oldSelectedCustomerGroup == null) return;
            SelectedCustomerGroup =
                CustomerGroups.SingleOrDefault(
                    customerGroup =>
                            customerGroup.ID.Equals(oldSelectedCustomerGroup.ID));
        }

        public void UpdateListedCustomers()
        {
            if (_selectedCity != null) return;

            var oldSelectedCustomer = _selectedCustomer;
            Customers.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var customersReturnedFromDatabase =
                    _selectedCustomerGroup.Name.Equals(Constants.ALL)
                        ? context.Customers.Include("Group")
                            .Include("City")
                            .OrderBy(customer => customer.Name)
                        : context.Customers.Include("Group")
                            .Include("City")
                            .Where(
                                customer =>
                                    customer.Group.ID.Equals(
                                        _selectedCustomerGroup.ID))
                            .OrderBy(customer => customer.Name);
                Customers.Add(new CustomerVM
                {
                    Model = new Customer { ID = -1, Name = Constants.ALL }
                });
                foreach (
                    var customer in
                    customersReturnedFromDatabase.OrderBy(
                        customer => customer.Name))
                    Customers.Add(new CustomerVM { Model = customer });
            }
            UpdateSelectedCustomer(oldSelectedCustomer);
        }

        private void UpdateSelectedCustomer(CustomerVM oldSelectedCustomer)
        {
            if (oldSelectedCustomer == null) return;
            SelectedCustomer =
                Customers.FirstOrDefault(
                    customer => customer.ID.Equals(oldSelectedCustomer.ID));
        }

        private void UpdateCities()
        {
            var oldSelectedCity = _selectedCity;

            Cities.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var citiesFromDatabase =
                    context.Cities.OrderBy(city => city.Name).ToList();

                foreach (var city in citiesFromDatabase)
                    Cities.Add(new CityVM { Model = city });
            }

            UpdateSelectedCity(oldSelectedCity);
        }

        private void UpdateSelectedCity(CityVM oldSelectedCity)
        {
            if (oldSelectedCity == null) return;
            SelectedCity =
                Cities.SingleOrDefault(
                    city => city.ID.Equals(oldSelectedCity.ID));
        }

        public void UpdateDisplayedCustomers()
        {
            DisplayedCustomers.Clear();

            using (var context = UtilityMethods.createContext())
            {
                var customersReturnedFromDatabase =
                    LoadCustomersFromDatabaseContextAccordingToSelections(
                        context);
                foreach (
                    var customer in
                    customersReturnedFromDatabase.Where(
                            customer => customer.Active.Equals(_isActiveChecked))
                        .OrderBy(customer => customer.Name))
                    DisplayedCustomers.Add(new CustomerVM { Model = customer });
            }
        }

        public static void DeactivateCustomerInDatabase(Customer customer)
        {
            using (var context = UtilityMethods.createContext())
            {
                context.Entry(customer).State = EntityState.Modified;
                customer.Active = false;
                context.SaveChanges();
            }
        }

        public static void ActivateCustomerInDatabase(Customer customer)
        {
            using (var context = UtilityMethods.createContext())
            {
                context.Entry(customer).State = EntityState.Modified;
                customer.Active = true;
                context.SaveChanges();
            }
        }

        private IEnumerable<Customer>
            LoadCustomersFromDatabaseContextAccordingToSelections(
                ERPContext context)
        {
            if (_selectedCity != null)
                return
                    context.Customers.Include("Group").Include("City")
                        .Where(
                            customer =>
                                    customer.City.ID.Equals(_selectedCity.ID))
                        .OrderBy(customer => customer.Name);

            if (_selectedCustomerGroup.Name.Equals(Constants.ALL) &&
                _selectedCustomer.Name.Equals(Constants.ALL))
                return
                    context.Customers.Include("Group")
                        .Include("City")
                        .OrderBy(customer => customer.Name);

            if (!_selectedCustomerGroup.Name.Equals(Constants.ALL) &&
                _selectedCustomer.Name.Equals(Constants.ALL))
                return
                    context.Customers.Include("Group").Include("City")
                        .Where(
                            customer =>
                                customer.Group.ID.Equals(
                                    _selectedCustomerGroup.ID))
                        .OrderBy(customer => customer.Name);

            return
                context.Customers.Include("Group").Include("City")
                    .Where(customer => customer.ID.Equals(_selectedCustomer.ID))
                    .OrderBy(customer => customer.Name);
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
            MessageBox.Show("Please select a line.", "No Selection",
                MessageBoxButton.OK);
            return false;
        }

        private static bool IsConfirmationYes()
        {
            return MessageBox.Show("Confirm activating/deactivating customer?",
                       "Confirmation", MessageBoxButton.YesNo,
                       MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
        #endregion
    }
}