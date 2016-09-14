namespace PutraJayaNT.ViewModels.Master.Customers
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Customer;
    using Models.Customer;
    using Utilities;
    using Utilities.ModelHelpers;

    public class MasterCustomersNewEntryVM : ViewModelBase
    {
        #region Backing Fields
        private string _newEntryName;
        private string _newEntryCity;
        private string _newEntryAddress;
        private string _newEntryTelephone;
        private string _newEntryNPWP;
        private CustomerGroupVM _newEntryGroup;
        private ICommand _newEntryCommand;
        private ICommand _cancelEntryCommand;
        #endregion

        private readonly MasterCustomersVM _parentVM;

        public MasterCustomersNewEntryVM(MasterCustomersVM parentVM)
        {
            _parentVM = parentVM;
            NewEntryGroups = new ObservableCollection<CustomerGroupVM>();
            UpdateNewEntryGroups();
        }

        #region Properties
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

        public ObservableCollection<CustomerGroupVM> NewEntryGroups { get; }

        public CustomerGroupVM NewEntryGroup
        {
            get { return _newEntryGroup; }
            set { SetProperty(ref _newEntryGroup, value, "NewEntryGroup"); }
        }
        #endregion

        #region Commands
        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!IsNewEntryCommandChecksSuccessful()) return;
                    var newCustomer = MakeNewEntryCustomer();
                    CustomerHelper.AddCustomerAlongWithItsLedgerToDatabase(newCustomer);
                    ResetEntryFields();
                    _parentVM.UpdateListedCustomers();
                    _parentVM.UpdateDisplayedCustomers();
                }));
            }
        }

        public ICommand CancelEntryCommand => _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(ResetEntryFields));
        #endregion

        #region Helper Methods
        private void UpdateNewEntryGroups()
        {
            NewEntryGroups.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var groupsReturnedFromDatabase = context.CustomerGroups.OrderBy(customerGroup => customerGroup.Name);
                foreach (var group in groupsReturnedFromDatabase)
                    NewEntryGroups.Add(new CustomerGroupVM {Model = group});
            }
        }

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryCity = null;
            NewEntryAddress = null;
            NewEntryTelephone = null;
            NewEntryNPWP = null;
            NewEntryGroup = null;
            UpdateNewEntryGroups();
        }

        private bool AreAllNewEntryFieldsFilled()
        {
            if (_newEntryName != null && _newEntryCity != null && _newEntryAddress != null && _newEntryGroup != null) return true;
            MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private static bool IsNewEntryCommandConfirmationYes()
        {
            return MessageBox.Show("Confirm adding this customer?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.No;
        }

        private bool IsNewEntryCommandChecksSuccessful()
        {
            return AreAllNewEntryFieldsFilled() && IsNewEntryCommandConfirmationYes();
        }

        private Customer MakeNewEntryCustomer()
        {
            return new Customer
            {
                Name = _newEntryName,
                City = _newEntryCity ?? "",
                Address = _newEntryAddress ?? "",
                Telephone = _newEntryTelephone ?? "",
                NPWP = _newEntryNPWP ?? "",
                CreditTerms = _newEntryGroup.CreditTerms,
                MaxInvoices = _newEntryGroup.MaxInvoices,
                Group = _newEntryGroup.Model
            };
        }
        #endregion
    }
}
