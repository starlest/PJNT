using System.Collections.ObjectModel;
using System.Transactions;
using PutraJayaNT.Utilities.Database.Customer;
using PutraJayaNT.ViewModels.Customers;

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
        private readonly CustomerVM _editingCustomer;

        private int _editID;
        private string _editName;
        private string _editCity;
        private string _editAddress;
        private string _editTelephone;
        private string _editNPWP;
        private int _editCreditTerms;
        private int _editMaxInvoices;
        private CustomerGroupVM _editCustomerGroup;
        private ICommand _editConfirmCommand;
        #endregion

        public MasterCustomersEditVM(CustomerVM editingCustomer)
        {
            _editingCustomer = editingCustomer;
            EditGroups = new ObservableCollection<CustomerGroupVM>();
            UpdateEditGroups();
            SetDefaultEditProperties();
        }

        public ObservableCollection<CustomerGroupVM> EditGroups { get; }
         
        #region Properties
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

        public CustomerGroupVM EditCustomerGroup
        {
            get { return _editCustomerGroup; }
            set { SetProperty(ref _editCustomerGroup, value, "EditCustomerGroup"); }
        }
        #endregion

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    if (!IsEditConfirmationYes() && !AreEditFieldsValid()) return;
                    var editingCustomer = _editingCustomer.Model;
                    var editedCustomerCopy = MakeEditedCustomer();
                    SaveCustomerEditsToDatabase(editingCustomer, editedCustomerCopy);
                    UpdateEditingCustomerUIValues();
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        #region Helper Methods
        private void SetDefaultEditProperties()
        {
            EditID = _editingCustomer.ID;
            EditName = _editingCustomer.Name;
            EditCity = _editingCustomer.City;
            EditAddress = _editingCustomer.Address;
            EditTelephone = _editingCustomer.Telephone;
            EditNPWP = _editingCustomer.NPWP;
            EditCreditTerms = _editingCustomer.CreditTerms;
            EditMaxInvoices = _editingCustomer.MaxInvoices;
            EditCustomerGroup = EditGroups.FirstOrDefault();
        }

        private void UpdateEditGroups()
        {
            EditGroups.Clear();
            var customerGroupsReturnedFromDatabase = DatabaseCustomerGroupHelper.GetAll();
            foreach (var customerGroup in customerGroupsReturnedFromDatabase)
                EditGroups.Add(new CustomerGroupVM { Model = customerGroup });
        }

        private Customer MakeEditedCustomer()
        {
            return new Customer
            {
                ID = _editID,
                Address = _editAddress,
                City = _editCity,
                CreditTerms = _editCreditTerms,
                Group = _editCustomerGroup.Model,
                MaxInvoices = _editMaxInvoices,
                Telephone = _editTelephone,
                Name = _editName,
                NPWP = _editNPWP
            };
        }

        private static LedgerAccount GetCustomerLedgerAccountFromDatabaseContext(ERPContext context, Customer customer)
        {
            var searchName = customer.Name + " Accounts Receivable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }

        private static bool IsCustomerNameChanged(Customer editingCustomer, Customer editedCustomer)
        {
            return editingCustomer.Name == editedCustomer.Name;
        }

        private static void ChangeCustomerLedgerAccountNameInDatabaseContext(ERPContext context, Customer editingCustomer, Customer editedCustomer)
        {
            var ledgerAccountFromDatabase = GetCustomerLedgerAccountFromDatabaseContext(context, editingCustomer);
            ledgerAccountFromDatabase.Name = $"{editedCustomer.Name} Accounts Receivable";
            context.SaveChanges();
        }

        private static void DeepCopyCustomerProperties(Customer fromCustomer, ref Customer toCustomer)
        {
            toCustomer.Name = fromCustomer.Name;
            toCustomer.City = fromCustomer.City;
            toCustomer.Address = fromCustomer.Address;
            toCustomer.Telephone = fromCustomer.Telephone;
            toCustomer.NPWP = fromCustomer.NPWP;
            toCustomer.CreditTerms = fromCustomer.CreditTerms;
            toCustomer.MaxInvoices = fromCustomer.MaxInvoices;
            toCustomer.Group = fromCustomer.Group;
        }

        private static void SaveCustomerEditsToDatabaseContext(ERPContext context, Customer editingCustomer, Customer editedCustomer)
        {
            DatabaseCustomerHelper.AttachToObjectFromDatabaseContext(context, ref editingCustomer);
            DeepCopyCustomerProperties(editedCustomer, ref editingCustomer);

            var customerGroupToBeAttachedToDatabaseContext = editingCustomer.Group;
            DatabaseCustomerGroupHelper.AttachToObjectFromDatabaseContext(context, ref customerGroupToBeAttachedToDatabaseContext);
            editingCustomer.Group = customerGroupToBeAttachedToDatabaseContext;

            context.SaveChanges();
        }

        public static void SaveCustomerEditsToDatabase(Customer editingCustomer, Customer editedCustomer)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                if (!IsCustomerNameChanged(editingCustomer, editedCustomer))
                    ChangeCustomerLedgerAccountNameInDatabaseContext(context, editingCustomer, editedCustomer);
                SaveCustomerEditsToDatabaseContext(context, editingCustomer, editedCustomer);
                ts.Complete();
            }
        }

        private static bool IsEditConfirmationYes()
        {
            return MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private bool AreEditFieldsValid()
        {
            return _editName != null && _editAddress != null && _editCity != null && _editAddress != null;
        }

        private void UpdateEditingCustomerUIValues()
        {
            var editedCustomer = MakeEditedCustomer();
            var customerTo = _editingCustomer.Model;
            DeepCopyCustomerProperties(editedCustomer, ref customerTo);
            _editingCustomer.UpdatePropertiesToUI();
        }
        #endregion
    }
}
