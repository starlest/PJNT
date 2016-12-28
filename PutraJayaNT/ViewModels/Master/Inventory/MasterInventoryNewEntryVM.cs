namespace ECERP.ViewModels.Master.Inventory
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.Inventory;
    using Models.Supplier;
    using MVVMFramework;
    using Utilities.ModelHelpers;

    public class MasterInventoryNewEntryVM : ViewModelBase
    {
        private readonly MasterInventoryVM _parentVM;

        #region Backing Fields

        private string _newEntryID;
        private string _newEntryName;
        private CategoryVM _newEntryCategory;
        private SupplierVM _newEntrySupplier;
        private int _newEntryPiecesPerUnit;
        private string _newEntryUnitName;
        private string _newEntrySecondaryUnitName;
        private int _newEntryPiecesPerSecondaryUnit;
        private decimal _newEntryPurchasePrice;
        private decimal _newEntrySalesPrice;
        private ICommand _newEntryCommand;
        private ICommand _cancelEntryCommand;

        #endregion

        public MasterInventoryNewEntryVM(MasterInventoryVM parentVM)
        {
            _parentVM = parentVM;
            _newEntryPiecesPerUnit = 1;
        }

        public ObservableCollection<CategoryVM> Categories => _parentVM.Categories;

        public ObservableCollection<SupplierVM> Suppliers => _parentVM.Suppliers;

        #region Properties

        public string NewEntryID
        {
            get { return _newEntryID; }
            set { SetProperty(ref _newEntryID, value, "NewEntryID"); }
        }

        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, "NewEntryName"); }
        }

        public CategoryVM NewEntryCategory
        {
            get { return _newEntryCategory; }
            set { SetProperty(ref _newEntryCategory, value, "NewEntryCategory"); }
        }

        public SupplierVM NewEntrySupplier
        {
            get { return _newEntrySupplier; }
            set { SetProperty(ref _newEntrySupplier, value, "NewEntrySupplier"); }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, "NewEntryUnitName"); }
        }

        public string NewEntrySecondaryUnitName
        {
            get { return _newEntrySecondaryUnitName; }
            set { SetProperty(ref _newEntrySecondaryUnitName, value, () => NewEntrySecondaryUnitName); }
        }

        public int NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, () => NewEntryPiecesPerUnit); }
        }

        public int NewEntryPiecesPerSecondaryUnit
        {
            get { return _newEntryPiecesPerSecondaryUnit; }
            set { SetProperty(ref _newEntryPiecesPerSecondaryUnit, value, () => NewEntryPiecesPerSecondaryUnit); }
        }

        public decimal NewEntrySalesPrice
        {
            get { return _newEntrySalesPrice; }
            set { SetProperty(ref _newEntrySalesPrice, value, "NewEntrySalesPrice"); }
        }

        public decimal NewEntryPurchasePrice
        {
            get { return _newEntryPurchasePrice; }
            set { SetProperty(ref _newEntryPurchasePrice, value, "NewEntryPurchasePrice"); }
        }

        #endregion

        #region Commands

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!AreAllEntryFieldsFilled() || !AreAllPriceEntryFieldsValid() || !AreAllQuantityFieldsValid())
                        return;
                    if (MessageBox.Show("Confirm adding this product?", "Confirmation", MessageBoxButton.YesNo) ==
                        MessageBoxResult.No) return;
                    var newEntryItem = MakeNewEntryItem();
                    InventoryHelper.AddItemToDatabase(newEntryItem);
                    ResetEntryFields();
                    _parentVM.UpdateItems();
                    _parentVM.UpdateDisplayedItems();
                }));
            }
        }

        public ICommand CancelEntryCommand
            => _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(ResetEntryFields));

        #endregion

        #region Helper Methods

        private Item MakeNewEntryItem()
        {
            var newEntryItem = new Item
            {
                ItemID = _newEntryID,
                Name = _newEntryName,
                Category = _newEntryCategory.Model,
                UnitName = _newEntryUnitName,
                SecondaryUnitName = _newEntrySecondaryUnitName,
                PiecesPerUnit = _newEntryPiecesPerUnit,
                PiecesPerSecondaryUnit = _newEntryPiecesPerSecondaryUnit,
                PurchasePrice = _newEntryPurchasePrice/_newEntryPiecesPerUnit,
                SalesPrice = _newEntrySalesPrice/_newEntryPiecesPerUnit,
                Suppliers = new ObservableCollection<Supplier>()
            };
            newEntryItem.Suppliers.Add(_newEntrySupplier.Model);
            return newEntryItem;
        }

        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryID != null && _newEntryName != null &&
                _newEntryCategory != null && _newEntrySupplier != null &&
                _newEntryUnitName != null && (
                    (_newEntryPiecesPerSecondaryUnit > 0 && _newEntrySecondaryUnitName != null)
                    || (_newEntryPiecesPerSecondaryUnit == 0)
                    ))
                return true;

            MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private bool AreAllPriceEntryFieldsValid()
        {
            if (_newEntrySalesPrice >= 0 && _newEntryPurchasePrice >= 0)
                return true;
            MessageBox.Show("Please check that all price fields are valid.", "Invalid Field(s)", MessageBoxButton.OK);
            return false;
        }

        private bool AreAllQuantityFieldsValid()
        {
            if (_newEntryPiecesPerUnit > 0 && _newEntryPiecesPerSecondaryUnit >= 0 &&
                _newEntryPiecesPerUnit > _newEntryPiecesPerSecondaryUnit)
            {
                if ((_newEntryPiecesPerSecondaryUnit > 0 && _newEntryPiecesPerUnit%_newEntryPiecesPerSecondaryUnit == 0) ||
                    _newEntryPiecesPerSecondaryUnit == 0)
                    return true;
            }
            MessageBox.Show("Please check that all quantity fields are valid. \n" +
                            "Pieces/Unit must be greater than Pieces/SecondaryUnit \n" +
                            "and the former must be divisible by the latter.", "Invalid Field(s)", MessageBoxButton.OK);
            return false;
        }

        private void ResetEntryFields()
        {
            NewEntryID = null;
            NewEntryName = null;
            NewEntryCategory = null;
            NewEntrySupplier = null;
            NewEntryPiecesPerSecondaryUnit = 0;
            NewEntryPiecesPerUnit = 1;
            NewEntryUnitName = null;
            NewEntrySecondaryUnitName = null;
            NewEntrySalesPrice = 0;
            NewEntryPurchasePrice = 0;
            _parentVM.UpdateSuppliers();
            _parentVM.UpdateCategories();
        }

        #endregion
    }
}
