using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using PutraJayaNT.Utilities.Database;

namespace PutraJayaNT.ViewModels.Master.Suppliers
{
    using ViewModels.Suppliers;

    public class MasterSuppliersEditVM : ViewModelBase
    { 
        private readonly SupplierVM _editingSupplier;

        private ICommand _confirmEditCommand;
        private string _editName;
        private string _editAddress;
        private int _editGSTID;

        public MasterSuppliersEditVM(SupplierVM editingSupplier)
        {
            _editingSupplier = editingSupplier;
            SetDefaultEditProperties();
        }

        #region Edit Properties
        public string EditName
        {
            get { return _editName; }
            set { SetProperty(ref _editName, value, () => EditName); }
        }

        public string EditAddress
        {
            get { return _editAddress; }
            set { SetProperty(ref _editAddress, value, () => EditAddress); }
        }

        public int EditGSTID
        {
            get { return _editGSTID; }
            set { SetProperty(ref _editGSTID, value, () => EditGSTID); }
        }
        #endregion

        public ICommand ConfirmEditCommand
        {
            get
            {
                return _confirmEditCommand ?? (_confirmEditCommand = new RelayCommand(() =>
                {
                    if (!IsEditConfirmationYes() && !AreEditFieldsValid()) return;
                    var editingSupplier = _editingSupplier.Model;
                    var editedSupplierCopy = MakeEditedSupplier();
                    SaveSupplierEditsToDatabase(editingSupplier, editedSupplierCopy);
                    UpdateEditingSupplierUIValues();
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        #region Helper Methods
        private Supplier MakeEditedSupplier()
        {
            return new Supplier
            {
                Active = true,
                Address = _editAddress,
                GSTID = _editGSTID,
                Name = _editName
            };
        }

        private void SetDefaultEditProperties()
        {
            _editName = _editingSupplier.Name;
            _editAddress = _editingSupplier.Address;
            _editGSTID = _editingSupplier.GSTID;
        }

        private static bool IsEditConfirmationYes()
        {
            return MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private bool AreEditFieldsValid()
        {
            return _editName != null && _editAddress != null;
        }

        private static void DeepCopySupplierProperties(Supplier fromSupplier, ref Supplier toSupplier)
        {
            toSupplier.Active = fromSupplier.Active;
            toSupplier.Name = fromSupplier.Name;
            toSupplier.Address = fromSupplier.Address;
            toSupplier.GSTID = fromSupplier.GSTID;
        }

        public static void SaveSupplierEditsToDatabase(Supplier editingSupplier, Supplier editedSupplier)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                if (!IsSupplierNameChange(editingSupplier, editedSupplier))
                    ChangeSupplierLedgerAccountNameInDatabaseContext(context, editingSupplier, editedSupplier);
                SaveSupplierEditsToDatabaseContext(context, editingSupplier, editedSupplier);
                ts.Complete();
            }
        }

        private static void SaveSupplierEditsToDatabaseContext(ERPContext context, Supplier editingSupplier, Supplier editedSupplier)
        {
            DatabaseSupplierHelper.AttachToDatabaseContext(context, ref editingSupplier);
            DeepCopySupplierProperties(editedSupplier, ref editingSupplier);
            context.SaveChanges();
        }

        private static LedgerAccount GetSupplierLedgerAccountFromDatabaseContext(ERPContext context, Supplier supplier)
        {
            var searchName = supplier.Name + " Accounts Payable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }

        private static void ChangeSupplierLedgerAccountNameInDatabaseContext(ERPContext context, Supplier editingSupplier, Supplier editedSupplier)
        {
            var ledgerAccountFromDatabase = GetSupplierLedgerAccountFromDatabaseContext(context, editingSupplier);
            ledgerAccountFromDatabase.Name = $"{editedSupplier.Name} Accounts Payable";
            context.SaveChanges();
        }

        private static bool IsSupplierNameChange(Supplier editingSupplier, Supplier editedSupplier)
        {
            return editingSupplier.Name == editedSupplier.Name;
        }

        private void UpdateEditingSupplierUIValues()
        {
            var editedSupplier = MakeEditedSupplier();
            var supplierTo = _editingSupplier.Model;
            DeepCopySupplierProperties(editedSupplier, ref supplierTo);
            _editingSupplier.UpdatePropertiesToUI();
        }
        #endregion
    }
}
