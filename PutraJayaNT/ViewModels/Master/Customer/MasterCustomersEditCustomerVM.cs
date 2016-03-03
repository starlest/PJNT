namespace PutraJayaNT.ViewModels.Master.Customer
{
    using MVVMFramework;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models;
    using Models.Accounting;
    using Utilities;

    public class MasterCustomersEditCustomerVM : ViewModelBase
    {
        #region Backing Fields
        private bool _isEditWindowNotOpen;
        private Visibility _editWindowVisibility;
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
        private ICommand _activateCommand;
        #endregion

        private readonly MasterCustomersVM _parentVM;
        
        public MasterCustomersEditCustomerVM(MasterCustomersVM parentVM)
        {
            _parentVM = parentVM;

            _isEditWindowNotOpen = true;
            _editWindowVisibility = Visibility.Hidden;
        }

        #region Properties
        public CustomerVM EditCustomer
        {
            get { return _parentVM.SelectedLine; }
            set { _parentVM.SelectedLine = value; }
        }

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
                    if (IsEditCustomerSelected()) return;
                    SetDefaultEditProperties();
                    ShowEditWindow();
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

        public ICommand ActivateCommand
        {
            get
            {
                return _activateCommand ?? (_activateCommand = new RelayCommand(() =>
                {
                    if (IsEditCustomerSelected()) return;
                    if (EditCustomer.Active) DeactivateEditCustomer();
                    else ActivateEditCustomer();
                    _parentVM.UpdateDisplayedCustomers();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private static LedgerAccount GetCustomerLedgerAccountFromDatabaseContext(ERPContext context, Customer customer)
        {
            var searchName = customer.Name + " Accounts Receivable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }

        private Customer GetEditCustomerFromDatabaseContext(ERPContext context)
        {
            return context.Customers.First(customer => customer.ID.Equals(EditCustomer.ID));
        }

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

        private bool IsEditCustomerSelected()
        {
            if (EditCustomer != null) return false;
            MessageBox.Show("Please select a customer to edit.", "No Selection", MessageBoxButton.OK);
            return true;
        }

        private void SetDefaultEditProperties()
        {
            EditID = EditCustomer.ID;
            EditName = EditCustomer.Name;
            EditCity = EditCustomer.City;
            EditAddress = EditCustomer.Address;
            EditTelephone = EditCustomer.Telephone;
            EditNPWP = EditCustomer.NPWP;
            EditCreditTerms = EditCustomer.CreditTerms;
            EditMaxInvoices = EditCustomer.MaxInvoices;
            EditGroup = EditCustomer.Group;
        }

        private bool IsCustomerNameChanged()
        {
            return EditCustomer.Name != _editName;
        }

        private void ChangeCustomerLedgerAccountNameInDatabaseContext(ERPContext context)
        {
            var ledgerAccount = GetCustomerLedgerAccountFromDatabaseContext(context, EditCustomer.Model);
            ledgerAccount.Name = $"{_editName} Accounts Receivable";
        }

        private void SaveCustomerEditsToDatabaseContext(ERPContext context)
        {
            EditCustomer.Model = context.Customers.First(e => e.ID.Equals(EditCustomer.ID));
            EditCustomer.ID = _editID;
            EditCustomer.Name = _editName;
            EditCustomer.City = _editCity;
            EditCustomer.Address = _editAddress;
            EditCustomer.Telephone = _editTelephone;
            EditCustomer.NPWP = _editNPWP;
            EditCustomer.CreditTerms = _editCreditTerms;
            EditCustomer.MaxInvoices = _editMaxInvoices;
            EditCustomer.Group = context.CustomerGroups.First(e => e.ID.Equals(_editGroup.ID));
        }

        private void SaveCustomerEditsToDatabase()
        {
            using (var context = new ERPContext())
            {
                if (IsCustomerNameChanged())
                    ChangeCustomerLedgerAccountNameInDatabaseContext(context);
                SaveCustomerEditsToDatabaseContext(context);
                context.SaveChanges();
            }
        }

        private void DeactivateEditCustomer()
        {
            using (var context = new ERPContext())
            {
                var editCustomerReturnedFromDatabase = GetEditCustomerFromDatabaseContext(context);
                editCustomerReturnedFromDatabase.Active = false;
                context.SaveChanges();
            }   
        }

        private void ActivateEditCustomer()
        {
            using (var context = new ERPContext())
            {
                var editCustomerReturnedFromDatabase = GetEditCustomerFromDatabaseContext(context);
                editCustomerReturnedFromDatabase.Active = true;
                context.SaveChanges();
            }
        }
        #endregion
    }
}
