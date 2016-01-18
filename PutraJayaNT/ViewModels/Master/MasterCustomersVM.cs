using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PUJASM.ERP.ViewModels.Master
{
    class MasterCustomersVM : ViewModelBase
    {
        ObservableCollection<CustomerGroup> _groups;
        ObservableCollection<CustomerVM> _customers;
        ObservableCollection<CustomerVM> _displayedCustomers;

        CustomerGroup _selectedGroup;
        CustomerVM _selectedCustomer;

        string _newEntryName;
        string _newEntryCity;
        string _newEntryAddress;
        string _newEntryTelephone;
        string _newEntryNPWP;
        int _newEntryCreditTerms;
        ObservableCollection<CustomerGroup> _newEntryGroups;
        CustomerGroup _newEntryGroup;
        ICommand _newEntryCommand;
        ICommand _cancelEntryCommand;

        int _editID;
        string _editName;
        string _editCity;
        string _editAddress;
        string _editTelephone;
        string _editNPWP;
        int _editCreditTerms;
        CustomerGroup _editGroup;
        ICommand _editCommand;
        ICommand _editCancelCommand;
        ICommand _editConfirmCommand;

        bool _isEditWindowNotOpen;
        Visibility _editWindowVisibility;

        CustomerVM _selectedLine;

        public MasterCustomersVM()
        {
            _groups = new ObservableCollection<CustomerGroup>();
            _newEntryGroups = new ObservableCollection<CustomerGroup>();
            _customers = new ObservableCollection<CustomerVM>();
            _displayedCustomers = new ObservableCollection<CustomerVM>();

            UpdateGroups();

            SelectedGroup = _groups.FirstOrDefault();
            SelectedCustomer = _customers.FirstOrDefault();

            _newEntryCreditTerms = 7;

            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;
        }

        public ObservableCollection<CustomerGroup> Groups
        {
            get { return _groups; }
        }

        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
        }

        public ObservableCollection<CustomerVM> DisplayedCustomers
        {
            get { return _displayedCustomers; }
        }

        public CustomerGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                SetProperty(ref _selectedGroup, value, "SelectedGroup");

                if (_selectedGroup == null) return;

                UpdateListedCustomers();
                SelectedCustomer = _customers.FirstOrDefault();
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

        public CustomerVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine");  }
        }

        private void UpdateGroups()
        {
            _groups.Clear();

            using (var context = new ERPContext())
            {
                var groups = context.CustomerGroups.ToList();

                _groups.Add(new CustomerGroup { ID = -1, Name = "All" });
                foreach (var group in groups)
                {
                    _groups.Add(group);
                    _newEntryGroups.Add(group);
                }
            }
        }

        private void UpdateListedCustomers()
        {
            _customers.Clear();
            _displayedCustomers.Clear();

            using (var context = new ERPContext())
            {
                if (_selectedGroup.Name != "All")
                {
                    var customers = context.Customers
                        .Include("Group")
                        .Where(e => e.Group.Name.Equals(_selectedGroup.Name))
                        .OrderBy(e => e.Name);

                    _customers.Add(new CustomerVM { Model = new Customer { ID = -1, Name = "All" } });
                    foreach (var customer in customers)
                        _customers.Add(new CustomerVM { Model = customer });
                }
                else
                {
                    var customers = context.Customers
                        .Include("Group")
                        .OrderBy(e => e.Name);

                    _customers.Add(new CustomerVM { Model = new Customer { ID = -1, Name = "All" } });
                    foreach (var customer in customers)
                        _customers.Add(new CustomerVM { Model = customer });
                }
            }
        }

        private void UpdateDisplayedCustomers()
        {
            _displayedCustomers.Clear();

            if (_selectedGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
            {
                using (var context = new ERPContext())
                {
                    var customers = context.Customers
                        .Include("Group")
                        .ToList();

                    foreach (var customer in customers)
                        _displayedCustomers.Add(new CustomerVM { Model = customer });
                }
            }

            else if (!_selectedGroup.Name.Equals("All") && _selectedCustomer.Name.Equals("All"))
            {
                using (var context = new ERPContext())
                {
                    var customers = context.Customers
                        .Where(e => e.Group.Name.Equals(_selectedGroup.Name))
                        .Include("Group")
                        .ToList();

                    foreach (var customer in customers)
                        _displayedCustomers.Add(new CustomerVM { Model = customer });

                }
            }

            else
            {
                using (var context = new ERPContext())
                {
                    var customer = context.Customers
                        .Where(e => e.ID == _selectedCustomer.ID)
                        .Include("Group")
                        .FirstOrDefault();

                    _displayedCustomers.Add(new CustomerVM { Model = customer });
                }
            }
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

        public ObservableCollection<CustomerGroup> NewEntryGroups
        {
            get { return _newEntryGroups; }
        }

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
                    if (_newEntryName == null || _newEntryGroup == null)
                    {
                        MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm adding this customer?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        var newCustomer = new Customer
                        {
                            Name = _newEntryName,
                            City = _newEntryCity,
                            Address = _newEntryAddress,
                            Telephone = _newEntryTelephone,
                            NPWP = _newEntryNPWP,
                            CreditTerms = _newEntryCreditTerms,
                            Group = _newEntryGroup
                        };

                        var context = new ERPContext();
                        try
                        {
                            context.CustomerGroups.Attach(_newEntryGroup);
                            context.Customers.Add(newCustomer);

                            // Add a ledger account for this customer in the database
                            LedgerAccount account = new LedgerAccount
                            {
                                Name = _newEntryName + " Accounts Receivable",
                                Notes = "Accounts Receivable",
                                Class = "Asset"
                            };

                            context.Ledger_Accounts.Add(account);
                            context.Ledger_General.Add(new LedgerGeneral
                            {
                                LedgerAccount = account,
                                PeriodYear = DateTime.Now.Year,
                                Period = DateTime.Now.Month,
                            });

                            // Add an account Balance for this customer in the database
                            context.Ledger_Account_Balances.Add(new LedgerAccountBalance
                            {
                                LedgerAccount = account,
                                PeriodYear = DateTime.Now.Year,
                            });

                            context.SaveChanges();
                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(_newEntryGroup, EntityState.Detached);
                        }

                        catch (Exception e)
                        {
                            MessageBox.Show(string.Format("There was an error while trying to add the customer. {0}", e), "Error Encountered", MessageBoxButton.OK);
                        }

                        ResetEntryFields();
                    }
                }));
            }
        }

        public ICommand CancelEntryCommand
        {
            get
            {
                return _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(() =>
                {
                    ResetEntryFields();
                }));
            }
        }

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

        // ------------------------- Edit Properties ------------------------- //

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
                    if (_selectedLine == null)
                    {
                        MessageBox.Show("Please select a line to edit.", "No Selection", MessageBoxButton.OK);
                        return;
                    }

                    EditID = _selectedLine.ID;
                    EditName = _selectedLine.Name;
                    EditCity = _selectedLine.City;
                    EditAddress = _selectedLine.Address;
                    EditTelephone = _selectedLine.Telephone;
                    EditNPWP = _selectedLine.NPWP;
                    EditCreditTerms = _selectedLine.CreditTerms;
                    EditGroup = _selectedLine.Group;
                    EditWindowVisibility = Visibility.Visible;
                    IsEditWindowNotOpen = false;
                }));
            }
        }

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // Update the item in the database
                        using (var context = new ERPContext())
                        {
                            var oldName = _selectedLine.Name;
                            _selectedLine.ID = _editID;
                            _selectedLine.Name = _editName;
                            _selectedLine.City = _editCity;
                            _selectedLine.Address = _editAddress;
                            _selectedLine.Telephone = _editTelephone;
                            _selectedLine.NPWP = _editNPWP;
                            _selectedLine.CreditTerms = _editCreditTerms;
                            _selectedLine.Group = _editGroup;

                            context.CustomerGroups.Attach(_editGroup);
                            context.Customers.Attach(_selectedLine.Model);
                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(_selectedLine.Model, EntityState.Modified);

                            // Change the Customer's Accounts Receivable name if there is a change to the name
                            if (!oldName.Equals(_editName))
                            {
                                var searchName = oldName + " Accounts Receivable";
                                var account = context.Ledger_Accounts
                                .Where(e => e.Name.Equals(searchName))
                                .FirstOrDefault();
                                account.Name = string.Format("{0} Accounts Receivable", _editName);
                            }

                            context.SaveChanges();
                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(_editGroup, EntityState.Detached);
                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(_selectedLine.Model, EntityState.Detached);
                        }

                        _selectedLine.Group = _editGroup;
                        EditWindowVisibility = Visibility.Collapsed;
                        IsEditWindowNotOpen = true;
                    }
                }));
            }
        }

        public ICommand EditCancelCommand
        {
            get
            {
                return _editCancelCommand ?? (_editCancelCommand = new RelayCommand(() =>
                {
                    EditWindowVisibility = Visibility.Collapsed;
                    IsEditWindowNotOpen = true;
                }));
            }
        }

        // ------------------------------------------------------------------- //
    }
}
