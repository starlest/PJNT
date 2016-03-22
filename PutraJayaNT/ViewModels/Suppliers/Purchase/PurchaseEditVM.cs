namespace PutraJayaNT.ViewModels.Suppliers.Purchase
{
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Utilities;
    using ViewModels.Purchase;

    public class PurchaseEditVM : ViewModelBase
    {
        private PurchaseTransactionLineVM _editingLine;

        decimal _editLinePurchasePrice;
        decimal _editLineDiscount;
        ICommand _editLineConfirmCommand;

        public PurchaseEditVM(PurchaseTransactionLineVM editingLine)
        {
            _editingLine = editingLine;
            SetEditPropertiesToEditingLineProperties();
        }

        public decimal EditLinePurchasePrice
        {
            get { return _editLinePurchasePrice; }
            set { SetProperty(ref _editLinePurchasePrice, value, () => EditLinePurchasePrice); }
        }

        public decimal EditLineDiscount
        {
            get { return _editLineDiscount; }
            set { SetProperty(ref _editLineDiscount, value, () => EditLineDiscount); }
        }

        public ICommand EditLineConfirmCommand
        {
            get
            {
                return _editLineConfirmCommand ?? (_editLineConfirmCommand = new RelayCommand(() =>
                {
                    if (!AreFieldsValid()) return;
                    SetEditingLinePropertiesToEditProperties();
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        private bool AreFieldsValid()
        {
            if (_editLinePurchasePrice >= 0 && _editLineDiscount >= 0 && _editLineDiscount <= _editLinePurchasePrice)
                return true;
            MessageBox.Show("Please check that all fields are valid.", "Invalid Field(s)", MessageBoxButton.OK);
            return false;
        }

        #region Helper Methods
        private void SetEditPropertiesToEditingLineProperties()
        {
            _editLinePurchasePrice = _editingLine.PurchasePrice;
            _editLineDiscount = _editingLine.Discount;
        }

        private void SetEditingLinePropertiesToEditProperties()
        {
            _editingLine.PurchasePrice = _editLinePurchasePrice;
            _editingLine.Discount = _editLineDiscount;
        }
        #endregion
    }
}
