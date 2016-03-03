namespace PutraJayaNT.ViewModels.Master.Customer
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models;
    using Models.Accounting;
    using Utilities;

    public class MasterCustomersNewEntryVM : ViewModelBase
    {
        #region Backing Fields
        private string _newEntryName;
        private string _newEntryCity;
        private string _newEntryAddress;
        private string _newEntryTelephone;
        private string _newEntryNPWP;
        private CustomerGroup _newEntryGroup;
        private ICommand _newEntryCommand;
        private ICommand _cancelEntryCommand;
        #endregion

        private readonly MasterCustomersVM _parentVM;

        public MasterCustomersNewEntryVM(MasterCustomersVM parentVM)
        {
            _parentVM = parentVM;
            NewEntryGroups = new ObservableCollection<CustomerGroup>();
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
        #endregion

        #region Helper Methods
        private void UpdateDisplayedCustomers()
        {
            _parentVM.UpdateDisplayedCustomers();
        }

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryCity = null;
            NewEntryAddress = null;
            NewEntryTelephone = null;
            NewEntryNPWP = null;
            NewEntryGroup = null;
            UpdateDisplayedCustomers();
        }

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
    }
}
