namespace PutraJayaNT.ViewModels.Master.Suppliers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using Utilities;
    using ViewModels.Inventory;
    using Views.Master.Suppliers;
    using ViewModels.Suppliers;

    public class MasterSuppliersVM : ViewModelBase
    {
        private bool _isActiveChecked;
        private SupplierVM _selectedSupplier;
        private CategoryVM _selectedCategory;
        private ItemVM _selectedCategoryItem;
        private SupplierVM _selectedLine;

        private ICommand _searchCommand;
        private ICommand _editSupplierCommand;
        private ICommand _activateSupplierCommand;

        public MasterSuppliersVM()
        {
            Suppliers = new ObservableCollection<SupplierVM>();
            Categories = new ObservableCollection<CategoryVM>();
            CategoryItems = new ObservableCollection<ItemVM>();
            DisplayedSuppliers = new ObservableCollection<SupplierVM>();

            UpdateCategories();
            UpdateSuppliers();
            _selectedSupplier = Suppliers.FirstOrDefault();
            _isActiveChecked = true;
            UpdateDisplayedSuppliers();

            NewEntryVM = new MasterSuppliersNewEntryVM(this);
        }

        public MasterSuppliersNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<SupplierVM> Suppliers { get; }

        public ObservableCollection<CategoryVM> Categories { get; }

        public ObservableCollection<ItemVM> CategoryItems { get; }

        public ObservableCollection<SupplierVM> DisplayedSuppliers { get; }
        #endregion

        #region Properties
        public bool IsActiveChecked
        {
            get { return _isActiveChecked; }
            set { SetProperty(ref _isActiveChecked, value, "IsActiveChecked"); }
        }

        public SupplierVM SelectedSupplier
        {
            get {  return _selectedSupplier; }
            set
            {
                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");

                if (_selectedSupplier == null) return;

                SelectedCategory = null;
                SelectedCategoryItem = null;
                CategoryItems.Clear();
            }
        }

        public CategoryVM SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                SetProperty(ref _selectedCategory, value, "SelectedCategory");

                if (_selectedCategory == null) return;

                SelectedSupplier = null;
                UpdateCategoryItems();
            }
        }

        public ItemVM SelectedCategoryItem
        {
            get { return _selectedCategoryItem; }
            set { SetProperty(ref _selectedCategoryItem, value, "SelectedCategoryItem"); }
        }

        public SupplierVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }
        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(() =>
                {
                    UpdateDisplayedSuppliers();
                    UpdateCategories();
                    UpdateSuppliers();
                }));
            }
        }

        public ICommand EditSupplierCommand
        {
            get
            {
                return _editSupplierCommand ?? (_editSupplierCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected()) return;
                    ShowEditWindow();
                    UpdateSuppliers();
                }));
            }
        }

        public ICommand ActivateSupplierCommand
        {
            get
            {
                return _activateSupplierCommand ?? (_activateSupplierCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected() || !IsConfirmationYes()) return;
                    if (_selectedLine.Active) DeactivateSupplierInDatabase(_selectedLine.Model);
                    else ActivateSupplierInDatabase(_selectedLine.Model);
                    _selectedLine.Active = !_isActiveChecked;
                }));
            }
        }
        #endregion

        #region Helper Methods
        public void UpdateSuppliers()
        {
            var oldSelectedSupplier = _selectedSupplier;

            Suppliers.Clear();
            var allSupplier = new Supplier { ID = -1, Name = "All" };
            Suppliers.Add(new SupplierVM { Model = allSupplier });
            using (var context = new ERPContext())
            {
                var suppliersFromDatabase = context.Suppliers.Where(supplier => !supplier.Name.Equals("-")).OrderBy(supplier => supplier.Name);
                foreach (var supplier in suppliersFromDatabase)
                    Suppliers.Add(new SupplierVM {Model = supplier});
            }

            UpdateSelectedSupplier(oldSelectedSupplier);
        }

        private void UpdateSelectedSupplier(SupplierVM oldSelectedSupplier)
        {
            if (oldSelectedSupplier == null) return;
            SelectedSupplier = Suppliers.FirstOrDefault(supplier => supplier.ID.Equals(oldSelectedSupplier.ID));
        }

        private void UpdateCategories()
        {
            var oldSelectedCategory = _selectedCategory;

            Categories.Clear();
            var allCategory = new Category { ID = -1, Name = "All" };
            Categories.Add(new CategoryVM { Model = allCategory });
            using (var context = new ERPContext())
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categoriesFromDatabase)
                    Categories.Add(new CategoryVM {Model = category});
            }

            UpdateSelectedCategory(oldSelectedCategory);
        }

        private void UpdateSelectedCategory(CategoryVM oldSelectedCategory)
        {
            if (oldSelectedCategory == null) return;
            SelectedCategory = Categories.FirstOrDefault(category => category.ID.Equals(oldSelectedCategory.ID));
        }

        private void UpdateCategoryItems()
        {
            var oldSelectedCategoryItem = _selectedCategoryItem;

            CategoryItems.Clear();
            var allCategoryItem = new Item {ItemID = "-1", Name = "All"};
            CategoryItems.Add(new ItemVM {Model = allCategoryItem});
            using (var context = new ERPContext())
            {
                var categoryItemsFromDatabase = _selectedCategory.Name.Equals("All")
                    ? context.Inventory.Include("Suppliers").OrderBy(item => item.Name)
                    : context.Inventory.Include("Suppliers")
                        .Where(item => item.Category.ID.Equals(_selectedCategory.ID))
                        .OrderBy(item => item.Name);

                foreach (var item in categoryItemsFromDatabase)
                    CategoryItems.Add(new ItemVM { Model = item });
            }

            UpdateSelectedCategoryItem(oldSelectedCategoryItem);
        }

        private void UpdateSelectedCategoryItem(ItemVM oldSelectedCategoryItem)
        {
            if (oldSelectedCategoryItem == null) return;
            SelectedCategoryItem = CategoryItems.FirstOrDefault(item => item.ID.Equals(oldSelectedCategoryItem.ID));
        }

        public void UpdateDisplayedSuppliers()
        {
            DisplayedSuppliers.Clear();
            if (_selectedSupplier != null) LoadDisplayedSuppliersAccordingToSelectedSupplier();
            else if (_selectedCategoryItem != null) LoadDisplayedSuppliersAccordingToSelectedCategoryItem();
        }

        private void LoadDisplayedSuppliersAccordingToSelectedSupplier()
        {
            if (_selectedSupplier.Name.Equals("All"))
            {
                foreach (
                    var supplier in
                        Suppliers.ToList()
                            .Where(supplier => !supplier.Name.Equals("All") && supplier.Active.Equals(_isActiveChecked))
                    )
                    DisplayedSuppliers.Add(supplier);
            }

            else
            {
                if (_selectedSupplier.Active.Equals(IsActiveChecked)) DisplayedSuppliers.Add(_selectedSupplier);
            }
        }

        private void LoadDisplayedSuppliersAccordingToSelectedCategoryItem()
        {
            var suppliersFromDatabase = new List<Supplier>();

            using (var context = new ERPContext())
            {
                if (_selectedCategoryItem.Name.Equals("All") && _selectedCategory.Name.Equals("All"))
                    suppliersFromDatabase = context.Suppliers.Include("Items").Where(supplier => !supplier.Name.Equals("-")).ToList();

                else if (_selectedCategoryItem.Name.Equals("All") && !_selectedCategory.Name.Equals("All"))
                {
                    foreach (
                        var supplier in
                            CategoryItems.Where(item => item.Suppliers != null)
                                .SelectMany(
                                    item => item.Suppliers.Where(supplier => !suppliersFromDatabase.Contains(supplier)))
                        )
                        suppliersFromDatabase.Add(supplier);
                }

                else
                {
                    foreach (var categoryItem in CategoryItems.Where(item => !item.Name.Equals("All")))
                    {
                        foreach (var supplier in categoryItem.Suppliers)
                        {
                            if (!suppliersFromDatabase.Contains(supplier))
                                suppliersFromDatabase.Add(supplier);
                        }

                    }
                }
            }

            foreach (var supplier in suppliersFromDatabase.Where(supplier => supplier.Active.Equals(_isActiveChecked)).OrderBy(supplier => supplier.Name))
                DisplayedSuppliers.Add(new SupplierVM { Model = supplier });         
        }

        private void ShowEditWindow()
        {
            var vm = new MasterSuppliersEditVM(_selectedLine);
            var editWindow = new MasterSuppliersEditView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editWindow.ShowDialog();
        }

        public static void DeactivateSupplierInDatabase(Supplier supplier)
        {
            using (var context = new ERPContext())
            {
                context.Entry(supplier).State = EntityState.Modified;
                supplier.Active = false;
                context.SaveChanges();
            }
        }

        public static void ActivateSupplierInDatabase(Supplier supplier)
        {
            using (var context = new ERPContext())
            {
                context.Entry(supplier).State = EntityState.Modified;
                supplier.Active = true;
                context.SaveChanges();
            }
        }

        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "No Selection", MessageBoxButton.OK);
            return false;
        }

        private static bool IsConfirmationYes()
        {
            return MessageBox.Show("Confirm activating/deactivating supplier?", "Confirmation", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
        #endregion
    }
}
