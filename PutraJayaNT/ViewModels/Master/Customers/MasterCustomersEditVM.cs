namespace ECERP.ViewModels.Master.Customers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using Models.Customer;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;

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


        public Action CloseWindow { get; set; }
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
                    if (!IsEditConfirmationYes() || !AreEditFieldsValid() || !IsCustomerNameInDatabaseAlready()) return;
                    var editingCustomer = _editingCustomer.Model;
                    var editedCustomerCopy = MakeEditedCustomer();
                    CustomerHelper.SaveCustomerEditsToDatabase(editingCustomer, editedCustomerCopy);
                    UpdateEditingCustomerUIValues();
                    CloseWindow();
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
            using (var context = UtilityMethods.createContext())
            {
                var customerGroupsReturnedFromDatabase = context.CustomerGroups.OrderBy(customerGroup => customerGroup.Name);
                foreach (var customerGroup in customerGroupsReturnedFromDatabase)
                    EditGroups.Add(new CustomerGroupVM {Model = customerGroup});
            }
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
       
        private static bool IsEditConfirmationYes()
        {
            return MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private bool AreEditFieldsValid()
        {
            return _editName != null && _editAddress != null && _editCity != null;
        }

        private void UpdateEditingCustomerUIValues()
        {
            var editedCustomer = MakeEditedCustomer();
            var customerTo = _editingCustomer.Model;
            CustomerHelper.DeepCopyCustomerProperties(editedCustomer, ref customerTo);
            _editingCustomer.UpdatePropertiesToUI();
        }

        private bool IsCustomerNameInDatabaseAlready()
        {
            using (var context = UtilityMethods.createContext())
            {
                if (_editName.Equals(_editingCustomer.Name) ||
                    context.Customers.SingleOrDefault(customer => customer.Name.Equals(_editName)) == null) return true;
                MessageBox.Show("Customer name already exists!", "Invalid Name", MessageBoxButton.OK);
                return false;
            }
        }
        #endregion
    }
}
