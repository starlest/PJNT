namespace PutraJayaNT.ViewModels.Master.Suppliers
{
    using ViewModels.Suppliers;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models;
    using Utilities;
    using Utilities.ModelHelpers;

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
                    SupplierHelper.SaveSupplierEditsToDatabase(editingSupplier, editedSupplierCopy);
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

        private void UpdateEditingSupplierUIValues()
        {
            var editedSupplier = MakeEditedSupplier();
            var supplierTo = _editingSupplier.Model;
            SupplierHelper.DeepCopySupplierProperties(editedSupplier, ref supplierTo);
            _editingSupplier.UpdatePropertiesToUI();
        }
        #endregion
    }
}
