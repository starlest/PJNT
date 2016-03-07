using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Customers;

namespace PutraJayaNT.ViewModels.Master.Inventory
{
    public class MasterInventoryEditAddAlternativeSalesPriceVM : ViewModelBase
    { 
        private string _editAddAlternativeSalesPriceName;
        private decimal? _editAddAlternativeSalesPriceSalesPrice;
        private ICommand _editAddAlternativeSalesPriceConfirmCommand;

        private readonly MasterInventoryEditVM _parentVM;
        readonly ObservableCollection<AlternativeSalesPriceVM> _editAlternativeSalesPrices;

        public MasterInventoryEditAddAlternativeSalesPriceVM(MasterInventoryEditVM parentVM)
        {
            _parentVM = parentVM;
            _editAlternativeSalesPrices = _parentVM.EditAlternativeSalesPrices;
        }

        public string EditAddAlternativeSalesPriceName
        {
            get { return _editAddAlternativeSalesPriceName; }
            set { SetProperty(ref _editAddAlternativeSalesPriceName, value, "EditAddAlternativeSalesPriceName"); }
        }

        public decimal? EditAddAlternativeSalesPriceSalesPrice
        {
            get { return _editAddAlternativeSalesPriceSalesPrice; }
            set { SetProperty(ref _editAddAlternativeSalesPriceSalesPrice, value, "EditAddAlternativeSalesPriceSalesPrice"); }
        }

        public ICommand EditAddAlternativeSalesPriceConfirmCommand
        {
            get
            {
                return _editAddAlternativeSalesPriceConfirmCommand ?? (_editAddAlternativeSalesPriceConfirmCommand = new RelayCommand(() =>
                {
                    if (!AreAllFieldsFilled() || !IsAlternativeSalesPriceValid()) return;
                    var newAltSalesPrice = CreateNewAlternativeSalesPrice();
                    _editAlternativeSalesPrices.Add(new AlternativeSalesPriceVM { Model = newAltSalesPrice });
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        #region Helper Methods
        private bool AreAllFieldsFilled()
        {
            if (_editAddAlternativeSalesPriceName != null || _editAddAlternativeSalesPriceSalesPrice != null)
                return true;
            MessageBox.Show("Please enter all fields.", "Missing Field(s)", MessageBoxButton.OK);
            return false;
        }

        private bool IsAlternativeSalesPriceValid()
        {
            if (_editAddAlternativeSalesPriceSalesPrice >= 0) return true;
            MessageBox.Show("Please enter a valid price.", "Invalid Value", MessageBoxButton.OK);
            return false;
        }

        private AlternativeSalesPrice CreateNewAlternativeSalesPrice()
        {
            Debug.Assert(_editAddAlternativeSalesPriceSalesPrice != null, "_editAddAlternativeSalesPriceSalesPrice != null");
            return new AlternativeSalesPrice
            {
                Name = _editAddAlternativeSalesPriceName,
                Item = _parentVM.EditingItem.Model,
                SalesPrice = (decimal)_editAddAlternativeSalesPriceSalesPrice
            };
        }

        #endregion
    }
}
