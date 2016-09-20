namespace ECRP.ViewModels.Master.Suppliers
{
    using System.Windows;
    using System.Windows.Input;
    using Models.Supplier;
    using MVVMFramework;
    using Utilities.ModelHelpers;

    public class MasterSuppliersNewEntryVM : ViewModelBase
    {
        private readonly MasterSuppliersVM _parentVM;

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
                    SupplierHelper.AddSupplierAlongWithItsLedgerToDatabase(newSupplier);
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
        #endregion
    }
}
