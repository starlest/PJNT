using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;

namespace PutraJayaNT.ViewModels.Master.Suppliers
{
    public class MasterSuppliersNewEntryVM : ViewModelBase
    {
        private MasterSuppliersVM _parentVM;

        private string _newEntryName;
        private string _newEntryAddress;
        private int? _newEntryGSTID;

        private ICommand _newEntryCommand;
        private ICommand _cancelEntryCommand;

        public MasterSuppliersNewEntryVM(MasterSuppliersVM parentVM)
        {
            _parentVM = parentVM;
        }

        #region Properties
        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, () => NewEntryName); }
        }

        public string NewEntryAddress
        {
            get { return _newEntryAddress; }
            set { SetProperty(ref _newEntryAddress, value, () => NewEntryAddress); }
        }

        public int? NewEntryGSTID
        {
            get { return _newEntryGSTID; }
            set { SetProperty(ref _newEntryGSTID, value, () => NewEntryGSTID); }
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
                    var newSupplier = CreateNewEntrySupplier();
                    AddSupplierAlongWithItsLedgerToDatabase(newSupplier);
                    ResetEntryFields();
                    _parentVM.UpdateSuppliers();
                    _parentVM.UpdateDisplayedSuppliers();
                    MessageBox.Show("Successfully added supplier!", "Success", MessageBoxButton.OK);
                }));
            }
        }

        public ICommand CancelEntryCommand => _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(ResetEntryFields));
        #endregion

        #region Helper Methods
        private bool IsNewEntryCommandChecksSuccessful()
        {
            return AreAllEntryFieldsFilled() && IsNewEntryCommandConfirmationYes();
        }

        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryName != null && _newEntryAddress != null) return true;
            MessageBox.Show("Please enter supplier's Name and Address", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private static bool IsNewEntryCommandConfirmationYes()
        {
            return MessageBox.Show("Confirm adding this supplier?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.No;
        }

        private Supplier CreateNewEntrySupplier()
        {
            return new Supplier { Name = _newEntryName, Address = _newEntryAddress, GSTID = _newEntryGSTID ?? 0 };
        }

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryAddress = null;
            NewEntryGSTID = null;
        }

        private static LedgerAccount CreateSupplierLedgerAccount(Supplier supplier)
        {
            return new LedgerAccount
            {
                Name = supplier.Name + " Accounts Payable",
                Notes = "Accounts Payable",
                Class = "Liability"
            };
        }

        private static LedgerGeneral CreateSupplierLedgerGeneral(LedgerAccount ledgerAccount)
        {
            return new LedgerGeneral
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
                Period = UtilityMethods.GetCurrentDate().Month,
            };
        }

        private static LedgerAccountBalance CreateSupplierLedgerAccountBalance(LedgerAccount ledgerAccount)
        {
            return new LedgerAccountBalance
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
            };
        }

        private static void CreateAndAddSupplierLedgerToDatabaseContext(ERPContext context, Supplier suppplier)
        {
            var ledgerAccount = CreateSupplierLedgerAccount(suppplier);
            context.Ledger_Accounts.Add(ledgerAccount);

            var ledgerGeneral = CreateSupplierLedgerGeneral(ledgerAccount);
            context.Ledger_General.Add(ledgerGeneral);

            var ledgerAccountBalance = CreateSupplierLedgerAccountBalance(ledgerAccount);
            context.Ledger_Account_Balances.Add(ledgerAccountBalance);
        }

        private static void AddSupplierToDatabaseContext(ERPContext context, Supplier supplier)
        {
            context.Suppliers.Add(supplier);
        }

        public static void AddSupplierAlongWithItsLedgerToDatabase(Supplier supplier)
        {
            using (var context = new ERPContext())
            {
                AddSupplierToDatabaseContext(context, supplier);
                CreateAndAddSupplierLedgerToDatabaseContext(context, supplier);
                context.SaveChanges();
            }
        }
        #endregion
    }
}
