namespace PutraJayaNT.ViewModels.Master.Customers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models.Customer;
    using Utilities;
    using Utilities.Database.Customer;
    using Customer;
    using Views.Master.Customers;

    public class MasterCustomersVM : ViewModelBase
    {
        #region Backing Fields
        private bool _isActiveChecked;
        private CustomerGroupVM _selectedCustomerGroup;
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

        public ObservableCollection<string> Cities { get; }

        public ObservableCollection<CustomerVM> DisplayedCustomers { get; }
        #endregion

        #region Properties
        public bool IsActiveChecked
        {
            get { return _isActiveChecked; }
            set { SetProperty(ref _isActiveChecked, value, "IsActiveChecked"); }
        }

        public CustomerGroupVM SelectedCustomerGroup
        {
            get { return _selectedCustomerGroup; }
            set
            {
                SetProperty(ref _selectedCustomerGroup, value, "SelectedCustomerGroup");

                if (_selectedCustomerGroup == null) return;

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

                SelectedCustomerGroup = null;
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
                    UpdateListedCustomers();
                }));
            }
        }

        public ICommand ActivateCustomerCommand
        {
            get
            {
                return _activateCustomerCommand ?? (_activateCustomerCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected() || !IsConfirmationYes()) return;
                    if (_selectedLine.Active) DeactivateCustomerInDatabase(_selectedLine.Model);
                    else ActivateCustomerInDatabase(_selectedLine.Model);
                    _selectedLine.Active = !_selectedLine.Active;
                }));
            }
        }
        #endregion

        #region Helper Methods 
        private void UpdateCustomerGroups()
        {
            var oldSelectedCustomerGroup = _selectedCustomerGroup;

            CustomerGroups.Clear();
            CustomerGroups.Add(new CustomerGroupVM { Model = new CustomerGroup { ID = -1, Name = "All" }});
            var groupsReturnedFromDatabase = DatabaseCustomerGroupHelper.GetAll();
            foreach (var group in groupsReturnedFromDatabase)
                CustomerGroups.Add(new CustomerGroupVM { Model = group});

            UpdateSelectedCustomerGroup(oldSelectedCustomerGroup);
        }

        private void UpdateSelectedCustomerGroup(CustomerGroupVM oldSelectedCustomerGroup)
        {
            if (oldSelectedCustomerGroup == null) return;
            SelectedCustomerGroup = CustomerGroups.FirstOrDefault(customerGroup => customerGroup.ID.Equals(oldSelectedCustomerGroup.ID));
        }

        public void UpdateListedCustomers()
        {
            var oldSelectedCustomer = _selectedCustomer;

            Customers.Clear();
            Customers.Add(new CustomerVM { Model = new Customer { ID = -1, Name = "All" } });
            var customersReturnedFromDatabase = _selectedCustomerGroup.Name.Equals("All") ? DatabaseCustomerHelper.GetAll() : DatabaseCustomerHelper.Get(customer => customer.Group.ID.Equals(_selectedCustomerGroup.ID));
            foreach (var customer in customersReturnedFromDatabase.OrderBy(customer => customer.Name))
                Customers.Add(new CustomerVM { Model = customer });

            UpdateSelectedCustomer(oldSelectedCustomer);
        }

        private void UpdateSelectedCustomer(CustomerVM oldSelectedCustomer)
        {
            if (oldSelectedCustomer == null) return;
            SelectedCustomer = Customers.FirstOrDefault(customer => customer.ID.Equals(oldSelectedCustomer.ID));
        }

        private void UpdateCities()
        {
            var oldSelectedCity = _selectedCity;

            var customersReturnedFromDatabase = DatabaseCustomerHelper.GetAll();
            foreach (var customer in customersReturnedFromDatabase.Where(customer => !Cities.Contains(customer.City)))
                Cities.Add(customer.City);
            ArrangeCitiesAlphabetically();

            UpdateSelectedCity(oldSelectedCity);
        }

        private void ArrangeCitiesAlphabetically()
        {
            var arragedCities = Cities.OrderBy(city => city).ToList();
            Cities.Clear();
            foreach (var city in arragedCities)
                Cities.Add(city);
        }

        private void UpdateSelectedCity(string oldSelectedCity)
        {
            if (oldSelectedCity == null) return;
            SelectedCity = Cities.FirstOrDefault(city => city.Equals(oldSelectedCity));
        }

        public void UpdateDisplayedCustomers()
        {
            DisplayedCustomers.Clear();
            var customersReturnedFromDatabase = LoadCustomersFromDatabaseAccordingToSelections();
            foreach (var customer in customersReturnedFromDatabase.Where(customer => customer.Active.Equals(_isActiveChecked)).OrderBy(customer => customer.Name))
                DisplayedCustomers.Add(new CustomerVM { Model = customer });
        }

        public static void DeactivateCustomerInDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                DatabaseCustomerHelper.AttachToObjectFromDatabaseContext(context, ref customer);
                customer.Active = false;
                context.SaveChanges();
            }
        }

        public static void ActivateCustomerInDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                DatabaseCustomerHelper.AttachToObjectFromDatabaseContext(context, ref customer);
                customer.Active = true;
                context.SaveChanges();
            }
        }

        private IEnumerable<Customer> LoadCustomersFromDatabaseAccordingToSelections()
        {
            if (_selectedCity != null) return DatabaseCustomerHelper.Get(customer => customer.City.Equals(_selectedCity));

            if (_selectedCustomerGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
                return DatabaseCustomerHelper.GetAll();

            if (!_selectedCustomerGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
                return DatabaseCustomerHelper.Get(customer => customer.Group.ID.Equals(_selectedCustomerGroup.ID));

            return DatabaseCustomerHelper.Get(customer => customer.ID.Equals(_selectedCustomer.ID));
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

        private static bool IsConfirmationYes()
        {
            return MessageBox.Show("Confirm activating/deactivating customer?", "Confirmation", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
        #endregion
    }
}
