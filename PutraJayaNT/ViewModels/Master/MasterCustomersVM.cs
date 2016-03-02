using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;

namespace PutraJayaNT.ViewModels.Master
{
    public class MasterCustomersVM : ViewModelBase
    {
        #region Selected Backing Fields
        private CustomerGroup _selectedGroup;
        private CustomerVM _selectedCustomer;
        private string _selectedCity;
        private CustomerVM _selectedLine;
        #endregion

        #region New Entry Backing Fields
        private string _newEntryName;
        private string _newEntryCity;
        private string _newEntryAddress;
        private string _newEntryTelephone;
        private string _newEntryNPWP;
        private int _newEntryCreditTerms;
        private CustomerGroup _newEntryGroup;
        private ICommand _newEntryCommand;
        private ICommand _cancelEntryCommand;
        #endregion

        #region Edit Backing Fields
        private int _editID;
        private string _editName;
        private string _editCity;
        private string _editAddress;
        private string _editTelephone;
        private string _editNPWP;
        private int _editCreditTerms;
        private int _editMaxInvoices;
        private CustomerGroup _editGroup;
        private ICommand _editCommand;
        private ICommand _editCancelCommand;
        private ICommand _editConfirmCommand;
        private bool _isEditWindowNotOpen;
        private Visibility _editWindowVisibility;
        #endregion

        public MasterCustomersVM()
        {
            Groups = new ObservableCollection<CustomerGroup>();
            NewEntryGroups = new ObservableCollection<CustomerGroup>();
            Customers = new ObservableCollection<CustomerVM>();
            Cities = new ObservableCollection<string>();
            DisplayedCustomers = new ObservableCollection<CustomerVM>();

            UpdateCities();
            UpdateGroups();

            SelectedGroup = Groups.FirstOrDefault();
            SelectedCustomer = Customers.FirstOrDefault();

            _newEntryCreditTerms = 7;

            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;
        }

        #region Collections
        public ObservableCollection<CustomerGroup> Groups { get; }

        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<string> Cities { get; }

        public ObservableCollection<CustomerVM> DisplayedCustomers { get; }
        #endregion

        public CustomerGroup SelectedGroup
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
            set
            {
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                UpdateDisplayedCustomers();
            }
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
                UpdateCities();
                UpdateDisplayedCustomers();
            }
        }

        public CustomerVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine");  }
        }

        #region New Entry Properties
        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, "NewEntryName"); }
        }

        public string NewEntryCity
        {
            get { return _newEntryCity; }
            set { SetProperty(ref _newEntryCity, value, "NewEntryCity"); }
        }

        public string NewEntryAddress
        {
            get { return _newEntryAddress; }
            set { SetProperty(ref _newEntryAddress, value, "NewEntryAddress"); }
        }

        public string NewEntryTelephone
        {
            get { return _newEntryTelephone; }
            set { SetProperty(ref _newEntryTelephone, value, "NewEntryTelephone"); }
        }

        public string NewEntryNPWP
        {
            get { return _newEntryNPWP; }
            set { SetProperty(ref _newEntryNPWP, value, "NewEntryNPWP"); }
        }

        public int NewEntryCreditTerms
        {
            get { return _newEntryCreditTerms; }
            set { SetProperty(ref _newEntryCreditTerms, value, "NewEntryCreditTerms"); }
        }

        public ObservableCollection<CustomerGroup> NewEntryGroups { get; }

        public CustomerGroup NewEntryGroup
        {
            get { return _newEntryGroup; }
            set { SetProperty(ref _newEntryGroup, value, "NewEntryGroup"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!IsNewEntryCommandChecksSuccessful()) return;
                    var newCustomer = MakeNewEntryCustomer();
                    AddCustomerAlongWithItsLedgerToDatabase(newCustomer);
                    ResetEntryFields();
                }));
            }
        }

        public ICommand CancelEntryCommand => _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(ResetEntryFields));

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryCity = null;
            NewEntryAddress = null;
            NewEntryTelephone = null;
            NewEntryNPWP = null;
            NewEntryCreditTerms = 7;
            NewEntryGroup = null;
            UpdateDisplayedCustomers();
        }
        #endregion

        #region Edit Properties
        public int EditID
        {
            get { return _editID; }
            set { SetProperty(ref _editID, value, "EditID"); }
        }

        public string EditName
        {
            get { return _editName; }
            set { SetProperty(ref _editName, value, "EditName"); }
        }

        public string EditCity
        {
            get { return _editCity; }
            set { SetProperty(ref _editCity, value, "EditCity"); }
        }

        public string EditAddress
        {
            get { return _editAddress; }
            set { SetProperty(ref _editAddress, value, "EditAddress"); }
        }

        public string EditTelephone
        {
            get { return _editTelephone; }
            set { SetProperty(ref _editTelephone, value, "EditTelephone"); }
        }

        public string EditNPWP
        {
            get { return _editNPWP; }
            set { SetProperty(ref _editNPWP, value, "EditNPWP"); }
        }

        public int EditCreditTerms
        {
            get { return _editCreditTerms; }
            set { SetProperty(ref _editCreditTerms, value, "EditCreditTerms"); }
        }

        public int EditMaxInvoices
        {
            get { return _editMaxInvoices; }
            set { SetProperty(ref _editMaxInvoices, value, "EditMaxInvoices"); }
        }

        public CustomerGroup EditGroup
        {
            get { return _editGroup; }
            set { SetProperty(ref _editGroup, value, "EditGroup"); }
        }

        public Visibility EditWindowVisibility
        {
            get { return _editWindowVisibility; }
            set { SetProperty(ref _editWindowVisibility, value, "EditWindowVisibility"); }
        }

        public bool IsEditWindowNotOpen
        {
            get { return _isEditWindowNotOpen; }
            set { SetProperty(ref _isEditWindowNotOpen, value, "IsEditWindowNotOpen"); }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(() =>
                {
                    if (IsSelectedLineNull()) return;
                    SetEditProperties();
                }));
            }
        }

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                    SaveCustomerEditsToDatabase();
                    HideEditWindow();
                }));
            }
        }

        public ICommand EditCancelCommand => _editCancelCommand ?? (_editCancelCommand = new RelayCommand(HideEditWindow));
        #endregion

        #region General Helper Methods 
        private static LedgerAccount GetCustomerLedgerAccountFromDatabaseContext(ERPContext context, Customer customer)
        {
            var searchName = customer.Name + " Accounts Receivable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }
        #endregion

        #region Update Collections Helper Methods 
        private void UpdateGroups()
        {
            Groups.Clear();

            using (var context = new ERPContext())
            {
                var groupsReturnedFromDatabase = context.CustomerGroups.ToList();

                Groups.Add(new CustomerGroup { ID = -1, Name = "All" });
                foreach (var group in groupsReturnedFromDatabase)
                {
                    Groups.Add(group);
                    NewEntryGroups.Add(group);
                }
            }
        }

        private static IEnumerable<Customer> GetAllCustomersFromDatabase()
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").ToList();
        }
    
        private static IEnumerable<Customer> GetGroupCustomersFromDatabase(CustomerGroup group)
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").Where(customer => customer.Group.Name.Equals(group.Name)).ToList();
        }

        private static IEnumerable<Customer> GetCityCustomersFromDatabase(string city)
        {
            using (var context = new ERPContext())
                return context.Customers.Include("Group").Where(customer => customer.City.Equals(city)).ToList();
        }

        private void UpdateCities()
        {
            Cities.Clear();
            var customersReturnedFromDatabase = GetAllCustomersFromDatabase();
            foreach (var customer in customersReturnedFromDatabase.Where(customer => !Cities.Contains(customer.City)))
                Cities.Add(customer.City);
        }

        private void UpdateListedCustomers()
        {
            Customers.Clear();
            DisplayedCustomers.Clear();

            var customersReturnedFromDatabase = _selectedGroup.Name != "All" ? GetAllCustomersFromDatabase() : GetGroupCustomersFromDatabase(_selectedGroup);
            Customers.Add(new CustomerVM { Model = new Customer { ID = -1, Name = "All" } });
            foreach (var customer in customersReturnedFromDatabase.OrderBy(customer => customer.Name))
                Customers.Add(new CustomerVM { Model = customer });
        }

        private IEnumerable<Customer> GetCustomersFromDatabaseAccordingToSelections()
        {
            using (var context = new ERPContext())
            {
                IEnumerable<Customer> customersReturnedFromDatabase;

                if (_selectedCity == null)
                {
                    if (_selectedGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
                        customersReturnedFromDatabase = GetAllCustomersFromDatabase();

                    else if (!_selectedGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
                        customersReturnedFromDatabase = GetGroupCustomersFromDatabase(_selectedGroup);

                    else
                        customersReturnedFromDatabase = context.Customers.Where(e => e.ID == _selectedCustomer.ID).Include("Group").ToList();
                }

                else
                    customersReturnedFromDatabase = GetCityCustomersFromDatabase(_selectedCity);

                return customersReturnedFromDatabase;
            }
        }

        private void UpdateDisplayedCustomers()
        {
            DisplayedCustomers.Clear();
            var customersReturnedFromDatabase = GetCustomersFromDatabaseAccordingToSelections();
            foreach (var customer in customersReturnedFromDatabase.OrderBy(customer => customer.Name))
                DisplayedCustomers.Add(new CustomerVM { Model = customer });
        }
        #endregion

        #region New Entry Command Helper Methods
        private bool IsAllNewEntryFieldsFilled()
        {
            if (_newEntryName != null && _newEntryGroup != null) return true;
            MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private static bool IsNewEntryCommandConfirmationYes()
        {
            return MessageBox.Show("Confirm adding this customer?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.No;
        }

        private bool IsNewEntryCommandChecksSuccessful()
        {
            return IsAllNewEntryFieldsFilled() && IsNewEntryCommandConfirmationYes();
        }

        private Customer MakeNewEntryCustomer()
        {
            return new Customer
            {
                Name = _newEntryName,
                City = _newEntryCity,
                Address = _newEntryAddress,
                Telephone = _newEntryTelephone,
                NPWP = _newEntryNPWP,
                CreditTerms = _newEntryGroup.CreditTerms,
                MaxInvoices = _newEntryGroup.MaxInvoices,
                Group = _newEntryGroup
            };
        }

        private static LedgerAccount CreateCustomerLedgerAccount(Customer customer)
        {
            return new LedgerAccount
            {
                Name = customer.Name + " Accounts Receivable",
                Notes = "Accounts Receivable",
                Class = "Asset"
            };
        }

        private static LedgerGeneral CreateCustomerLedgerGeneral(LedgerAccount ledgerAccount)
        {
            return new LedgerGeneral
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
                Period = UtilityMethods.GetCurrentDate().Month,
            };
        }

        private static LedgerAccountBalance CreateCustomerLedgerAccountBalance(LedgerAccount ledgerAccount)
        {
            return new LedgerAccountBalance
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
            };
        }

        private static void CreateAndAddCustomerLedgerToDatabaseContext(ERPContext context, Customer customer)
        {
            var ledgerAccount = CreateCustomerLedgerAccount(customer);
            context.Ledger_Accounts.Add(ledgerAccount);

            var ledgerGeneral = CreateCustomerLedgerGeneral(ledgerAccount);
            context.Ledger_General.Add(ledgerGeneral);

            var ledgerAccountBalance = CreateCustomerLedgerAccountBalance(ledgerAccount);
            context.Ledger_Account_Balances.Add(ledgerAccountBalance);
        }

        private static void AddCustomerToDatabaseContext(ERPContext context, Customer customer)
        {
            customer.Group = context.CustomerGroups.First(e => e.ID.Equals(customer.Group.ID));
            context.Customers.Add(customer);
        }

        public static void AddCustomerAlongWithItsLedgerToDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                AddCustomerToDatabaseContext(context, customer);
                CreateAndAddCustomerLedgerToDatabaseContext(context, customer);
                context.SaveChanges();
            }
        }
        #endregion

        #region Edit Line Helper Methods
        private void ShowEditWindow()
        {
            EditWindowVisibility = Visibility.Visible;
            IsEditWindowNotOpen = false;
        }

        private void HideEditWindow()
        {
            EditWindowVisibility = Visibility.Collapsed;
            IsEditWindowNotOpen = true;
        }

        private bool IsSelectedLineNull()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line to edit.", "No Selection", MessageBoxButton.OK);
            return false;
        }

        private void SetEditProperties()
        {
            EditID = _selectedLine.ID;
            EditName = _selectedLine.Name;
            EditCity = _selectedLine.City;
            EditAddress = _selectedLine.Address;
            EditTelephone = _selectedLine.Telephone;
            EditNPWP = _selectedLine.NPWP;
            EditCreditTerms = _selectedLine.CreditTerms;
            EditMaxInvoices = _selectedLine.MaxInvoices;
            EditGroup = _selectedLine.Group;
            ShowEditWindow();
        }

        private bool IsCustomerNameChanged()
        {
            return _selectedLine.Name != _editName;
        }

        private void ChangeCustomerLedgerAccountNameInDatabaseContext(ERPContext context)
        {
            var ledgerAccount = GetCustomerLedgerAccountFromDatabaseContext(context, _selectedLine.Model);
            ledgerAccount.Name = $"{_editName} Accounts Receivable";
        }

        private void SaveCustomerEditsToDatabaseContext(ERPContext context)
        {
            _selectedLine.Model = context.Customers.First(e => e.ID.Equals(_selectedLine.ID));  // Set selected line to customer from database's context
            _selectedLine.ID = _editID;
            _selectedLine.Name = _editName;
            _selectedLine.City = _editCity;
            _selectedLine.Address = _editAddress;
            _selectedLine.Telephone = _editTelephone;
            _selectedLine.NPWP = _editNPWP;
            _selectedLine.CreditTerms = _editCreditTerms;
            _selectedLine.MaxInvoices = _editMaxInvoices;
            _selectedLine.Group = context.CustomerGroups.First(e => e.ID.Equals(_editGroup.ID));
        }

        private void SaveCustomerEditsToDatabase()
        {
            // Update the item in the database
            using (var context = new ERPContext())
            {
                if (IsCustomerNameChanged())
                    ChangeCustomerLedgerAccountNameInDatabaseContext(context);
                SaveCustomerEditsToDatabaseContext(context);
                context.SaveChanges();
            }
        }
        #endregion
    }
}
