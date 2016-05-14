namespace PutraJayaNT.ViewModels.Master.Inventory
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using Utilities;
    using Customer;
    using Item;
    using Utilities.ModelHelpers;
    using ViewModels.Suppliers;
    using Views.Master.Inventory;

    public class MasterInventoryEditVM : ViewModelBase
    {
        #region Backing Fields
        private string _editID;
        private string _editName;
        private CategoryVM _editCategory;
        private decimal _editPurchasePrice;
        private decimal _editSalesPrice;
        private decimal _editSalesExpense;
        private SupplierVM _editSelectedSupplier;
        AlternativeSalesPriceVM _editSelectedAlternativeSalesPrice;
        private string _editUnitName;
        private string _editSecondaryUnitName;
        private int _editPiecesPerUnit;
        private int _editPiecesPerSecondaryUnit;

        private ICommand _editAddAlternativeSalesPriceCommand;
        private ICommand _editDeleteAlternativeSalesPriceCommand;
        private ICommand _editAddSupplierCommand;
        private ICommand _editDeleteSupplierCommand;
        private ICommand _editConfirmCommand;
        #endregion

        public MasterInventoryEditVM(ItemVM editingItem)
        {
            EditingItem = editingItem;

            CustomerGroups = new ObservableCollection<CustomerGroupVM>();
            Categories = new ObservableCollection<CategoryVM>();
            EditSuppliers = new ObservableCollection<SupplierVM>();
            EditAlternativeSalesPrices = new ObservableCollection<AlternativeSalesPriceVM>();

            UpdateCustomerGroups();
            UpdateCategories();
            SetDefaultEditProperties();
        }

        #region Collections
        public ObservableCollection<CustomerGroupVM> CustomerGroups { get; }

        public ObservableCollection<CategoryVM> Categories { get; }

        public ObservableCollection<SupplierVM> EditSuppliers { get; }

        public ObservableCollection<AlternativeSalesPriceVM> EditAlternativeSalesPrices { get; }
        #endregion

        #region Properties
        public ItemVM EditingItem { get; }

        public string EditID
        {
            get { return _editID; }
            set { SetProperty(ref _editID, value, () => EditID); }
        }

        public string EditName
        {
            get { return _editName; }
            set { SetProperty(ref _editName, value, () => EditName); }
        }

        public CategoryVM EditCategory
        {
            get { return _editCategory; }
            set { SetProperty(ref _editCategory, value, "EditCategory"); }
        }

        public decimal EditSalesPrice
        {
            get { return _editSalesPrice; }
            set { SetProperty(ref _editSalesPrice, value, () => EditSalesPrice); }
        }

        public decimal EditPurchasePrice
        {
            get { return _editPurchasePrice; }
            set { SetProperty(ref _editPurchasePrice, value, () => EditPurchasePrice); }
        }

        public decimal EditSalesExpense
        {
            get { return _editSalesExpense; }
            set { SetProperty(ref _editSalesExpense, value, () => EditSalesExpense); }
        }

        public string EditUnitName
        {
            get { return _editUnitName; }
            set { SetProperty(ref _editUnitName, value, () => EditUnitName); }
        }

        public int EditPiecesPerUnit
        {
            get { return _editPiecesPerUnit; }
            set { SetProperty(ref _editPiecesPerUnit, value, () => EditPiecesPerUnit); }
        }

        public string EditSecondaryUnitName
        {
            get { return _editSecondaryUnitName; }
            set { SetProperty(ref _editSecondaryUnitName, value, () => EditSecondaryUnitName); }
        }

        public int EditPiecesPerSecondaryUnit
        {
            get { return _editPiecesPerSecondaryUnit; }
            set { SetProperty(ref _editPiecesPerSecondaryUnit, value, () => _editPiecesPerSecondaryUnit); }
        }

        public SupplierVM EditSelectedSupplier
        {
            get { return _editSelectedSupplier; }
            set { SetProperty(ref _editSelectedSupplier, value, "EditSelectedSupplier"); }
        }

        public AlternativeSalesPriceVM EditSelectedAlternativeSalesPrice
        {
            get { return _editSelectedAlternativeSalesPrice; }
            set { SetProperty(ref _editSelectedAlternativeSalesPrice, value, "EditSelectedAlternativeSalesPrice"); }
        }
        #endregion

        #region Commands
        public ICommand EditAddSupplierCommand => _editAddSupplierCommand ?? (_editAddSupplierCommand = new RelayCommand(ShowEditAddSupplierWindow));

        public ICommand EditDeleteSupplierCommand
        {
            get
            {
                return _editDeleteSupplierCommand ?? (_editDeleteSupplierCommand = new RelayCommand(() =>
                {
                    EditSuppliers.Remove(_editSelectedSupplier);
                }));
            }
        }

        public ICommand EditAddAlternativeSalesPriceCommand => _editAddAlternativeSalesPriceCommand ?? (_editAddAlternativeSalesPriceCommand = new RelayCommand(ShowEditAddAlternativeSalesPriceWindow));

        public ICommand EditDeleteAlternativeSalesPriceCommand
        {
            get
            {
                return _editDeleteAlternativeSalesPriceCommand ?? (_editDeleteAlternativeSalesPriceCommand = new RelayCommand(() =>
                {
                    if (!IsEditAlternativeSalesPriceSelected()) return;
                    EditAlternativeSalesPrices.Remove(_editSelectedAlternativeSalesPrice);
                }));
            }
        }

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    if (!IsEditConfirmationYes() || !AreEditFieldsValid() || !IsItemNameInDatabaseAlready()) return;
                    var originalItem = EditingItem.Model;
                    var editedItemCopy = MakeEditedItem();
                    InventoryHelper.SaveItemEditsToDatabase(originalItem, editedItemCopy);
                    UpdateEditingItemUIValues();
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private Item MakeEditedItem()
        {
            var editedItem = new Item
            {
                ItemID = _editID,
                Category = _editCategory.Model,
                Name = _editName,
                PiecesPerUnit = _editPiecesPerUnit,
                PurchasePrice = _editPurchasePrice/_editPiecesPerUnit,
                SalesPrice = _editSalesPrice/_editPiecesPerUnit,
                SalesExpense = _editSalesExpense/_editPiecesPerUnit,
                UnitName = _editUnitName,
                SecondaryUnitName = _editSecondaryUnitName,
                PiecesPerSecondaryUnit = _editPiecesPerSecondaryUnit,
                Suppliers = new ObservableCollection<Supplier>(),
                AlternativeSalesPrices = new ObservableCollection<AlternativeSalesPrice>()
            };

            foreach (var supplier in EditSuppliers.ToList())
                editedItem.Suppliers.Add(supplier.Model);

            foreach (var altPrice in EditAlternativeSalesPrices.ToList())
                editedItem.AlternativeSalesPrices.Add(altPrice.Model);

            return editedItem;
        }

        private void SetDefaultEditProperties()
        {
            EditID = EditingItem.ID;
            EditName = EditingItem.Name;
            EditCategory = Categories.SingleOrDefault(category => category.ID.Equals(EditingItem.Category.ID));
            EditSalesPrice = EditingItem.SalesPrice;
            EditPurchasePrice = EditingItem.PurchasePrice;
            EditUnitName = EditingItem.UnitName;
            EditSecondaryUnitName = EditingItem.SecondaryUnitName;
            EditPiecesPerUnit = EditingItem.PiecesPerUnit;
            EditPiecesPerSecondaryUnit = EditingItem.PiecesPerSecondaryUnit;
            EditSalesExpense = EditingItem.SalesExpense;
            UpdateEditAlternativeSalesPrices();
            UpdateEditSuppliers();
            EditSelectedSupplier = EditSuppliers.FirstOrDefault();
        }

        private void UpdateCustomerGroups()
        {
            CustomerGroups.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var customerGroupsFromDatabase = context.CustomerGroups;
                foreach (var group in customerGroupsFromDatabase)
                    CustomerGroups.Add(new CustomerGroupVM {Model = group});
            }
        }

        private void UpdateCategories()
        {
            Categories.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categoriesFromDatabase)
                    Categories.Add(new CategoryVM {Model = category});
            }
        }

        private void UpdateEditSuppliers()
        {
            EditSuppliers.Clear();
            foreach (var supplier in EditingItem.Suppliers.OrderBy(supplier => supplier.Name))
                EditSuppliers.Add(new SupplierVM { Model = supplier });
        }

        private void UpdateEditAlternativeSalesPrices()
        {
            EditAlternativeSalesPrices.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var editItemAlternativeSalesPrices = context.AlternativeSalesPrices
                    .Where(altSalesPrice => altSalesPrice.ItemID.Equals(EditingItem.ID));
                foreach (var alternateSalesPrice in editItemAlternativeSalesPrices)
                    EditAlternativeSalesPrices.Add(new AlternativeSalesPriceVM {Model = alternateSalesPrice});
            }
        }

        private bool IsEditAlternativeSalesPriceSelected()
        {
            if (_editSelectedAlternativeSalesPrice != null) return true;
            MessageBox.Show("Please select an alternative sales price first.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private void ShowEditAddSupplierWindow()
        {
            var vm = new MasterInventoryEditAddSupplierVM(EditSuppliers);
            var editAddSupplierWindow = new MasterInventoryEditAddSupplierView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editAddSupplierWindow.ShowDialog();
        }

        private void ShowEditAddAlternativeSalesPriceWindow()
        {
            var vm = new MasterInventoryEditAddAlternativeSalesPriceVM(this);
            var editAddSupplierWindow = new MasterInventoryEditAddAlternativeSalesPriceView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editAddSupplierWindow.ShowDialog();
        }

        private static bool IsEditConfirmationYes()
        {
            return MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private bool AreEditFieldsValid()
        {
            return _editID != null && _editName != null && _editUnitName != null;
        }

        private bool IsItemNameInDatabaseAlready()
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                if (_editName.Equals(EditingItem.Name) ||
                    context.Inventory.SingleOrDefault(item => item.Name.Equals(_editName)) == null) return true;
                MessageBox.Show("Item name already exists!", "Invalid Name", MessageBoxButton.OK);
                return false;
            }
        }

        private void UpdateEditingItemUIValues()
        {
            var editedItem = MakeEditedItem();
            var itemTo = EditingItem.Model;
            InventoryHelper.DeepCopyItemProperties(editedItem, itemTo);
            EditingItem.Suppliers.Clear();
            foreach (var supplier in EditSuppliers)
                EditingItem.Suppliers.Add(supplier.Model);
            EditingItem.UpdatePropertiesToUI();
        }
        #endregion
    }
}
