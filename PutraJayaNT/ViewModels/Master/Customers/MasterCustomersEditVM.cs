using System.Collections.ObjectModel;
using System.Transactions;

namespace PutraJayaNT.ViewModels.Master.Customers
{
    using MVVMFramework;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models;
    using Models.Accounting;
    using Utilities;

    public class MasterCustomersEditVM : ViewModelBase
    {
        #region Backing Fields
        private int _editID;
        private string _editName;
        private string _editCity;
        private string _editAddress;
        private string _editTelephone;
        private string _editNPWP;
        private int _editCreditTerms;
        private int _editMaxInvoices;
        private CustomerGroup _editGroup;
        private ICommand _editConfirmCommand;
        #endregion

        public MasterCustomersEditVM(CustomerVM editingCustomer)
        {
            EditingCustomer = editingCustomer;
            EditGroups = new ObservableCollection<CustomerGroupVM>();
            UpdateEditGroups();
            SetDefaultEditProperties();
        }

        public ObservableCollection<CustomerGroupVM> EditGroups { get; }
         
        #region Properties
        public CustomerVM EditingCustomer { get; }

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

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                    var originalCustomer = EditingCustomer.Model;
                    var editedCustomerCopy = MakeEditedCustomer();
                    SaveCustomerEditsToDatabase(originalCustomer, editedCustomerCopy);
                    UpdateEditingCustomerUIValues();
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateEditGroups()
        {
            EditGroups.Clear();
            var customerGroupsReturnedFromDatabase = DatabaseHelper.LoadAllCustomerGroupsFromDatabase();
            foreach (var customerGroup in customerGroupsReturnedFromDatabase)
                EditGroups.Add(new CustomerGroupVM { Model = customerGroup });
        }

        private static LedgerAccount GetCustomerLedgerAccountFromDatabaseContext(ERPContext context, Customer customer)
        {
            var searchName = customer.Name + " Accounts Receivable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }

        private void SetDefaultEditProperties()
        {
            EditID = EditingCustomer.ID;
            EditName = EditingCustomer.Name;
            EditCity = EditingCustomer.City;
            EditAddress = EditingCustomer.Address;
            EditTelephone = EditingCustomer.Telephone;
            EditNPWP = EditingCustomer.NPWP;
            EditCreditTerms = EditingCustomer.CreditTerms;
            EditMaxInvoices = EditingCustomer.MaxInvoices;
            EditGroup = EditingCustomer.Group;
        }

        private static bool IsCustomerNameChanged(Customer originalCustomer, Customer editedCustomer)
        {
            return originalCustomer.Name == editedCustomer.Name;
        }

        private static void ChangeCustomerLedgerAccountNameInDatabaseContext(ERPContext context, Customer originalCustomer, Customer editedCustomer)
        {
            var ledgerAccountFromDatabase = GetCustomerLedgerAccountFromDatabaseContext(context, originalCustomer);
            ledgerAccountFromDatabase.Name = $"{editedCustomer.Name} Accounts Receivable";
            context.SaveChanges();
        }

        private static void SaveCustomerEditsToDatabaseContext(ERPContext context, Customer originalCustomer, Customer editedCustomer)
        {
            var editingCustomer = originalCustomer;
            DatabaseHelper.AttachCustomerToDatabaseContext(context, ref editingCustomer);
            editingCustomer.Name = editedCustomer.Name;
            editingCustomer.City = editedCustomer.City;
            editingCustomer.Address = editedCustomer.Address;
            editingCustomer.Telephone = editedCustomer.Telephone;
            editingCustomer.NPWP = editedCustomer.NPWP;
            editingCustomer.CreditTerms = editedCustomer.CreditTerms;
            editingCustomer.MaxInvoices = editedCustomer.MaxInvoices;

            var customerGroupToBeAttachedToDatabaseContext = editedCustomer.Group;
            DatabaseHelper.AttachCustomerGroupToDatabaseContext(context, ref customerGroupToBeAttachedToDatabaseContext);
            editingCustomer.Group = customerGroupToBeAttachedToDatabaseContext;

            context.SaveChanges();
        }

        public static void SaveCustomerEditsToDatabase(Customer originalCustomer, Customer editedCustomer)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                if (!IsCustomerNameChanged(originalCustomer, editedCustomer))
                    ChangeCustomerLedgerAccountNameInDatabaseContext(context, originalCustomer, editedCustomer);
                SaveCustomerEditsToDatabaseContext(context, originalCustomer, editedCustomer);
                ts.Complete();
            }
        }

        private Customer MakeEditedCustomer()
        {
            return new Customer
            {
                ID = _editID,
                Active = true,
                Address = _editAddress,
                City = _editCity,
                CreditTerms = _editCreditTerms,
                Group = _editGroup,
                MaxInvoices = _editMaxInvoices,
                Name = _editName,
                NPWP = _editNPWP
            };
        }

        private void UpdateEditingCustomerUIValues()
        {
            EditingCustomer.Model = MakeEditedCustomer();
            EditingCustomer.UpdatePropertiesToUI();
        }
        #endregion
    }
}
