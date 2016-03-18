namespace PutraJayaNT.ViewModels.Master.Inventory
{
    using MVVMFramework;
    using Models.Inventory;
    using Utilities;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using ViewModels.Inventory;
    using ViewModels.Suppliers;
    using Views.Master.Inventory;

    public class MasterInventoryVM : ViewModelBase
    {
        #region Backing Fields
        private bool _isActiveChecked;
        private ItemVM _selectedItem;
        private CategoryVM _selectedCategory;
        private SupplierVM _selectedSupplier;
        private ItemVM _selectedLine;

        private ICommand _editItemCommand;
        private ICommand _activateItemCommand;
        private ICommand _searchCommand;
        #endregion

        public MasterInventoryVM()
        {
            Items = new ObservableCollection<ItemVM>();
            Categories = new ObservableCollection<CategoryVM>();
            CategoriesWithAll = new ObservableCollection<CategoryVM>();
            Suppliers = new ObservableCollection<SupplierVM>();
            DisplayedItems = new ObservableCollection<ItemVM>();
            UpdateItems();
            UpdateCategories();
            UpdateSuppliers();
            SelectedCategory = CategoriesWithAll.FirstOrDefault();
            _isActiveChecked = true;

            UpdateDisplayedItems();

            NewEntryVM = new MasterInventoryNewEntryVM(this);
        }

        public MasterInventoryNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<ItemVM> Items { get; }

        public ObservableCollection<SupplierVM> Suppliers { get; }

        public ObservableCollection<ItemVM> DisplayedItems { get; }

        public ObservableCollection<CategoryVM> CategoriesWithAll { get; }

        public ObservableCollection<CategoryVM> Categories { get; }
        #endregion

        #region Properties
        public bool IsActiveChecked
        {
            get { return _isActiveChecked; }
            set { SetProperty(ref _isActiveChecked, value, "IsActiveChecked"); }
        }

        public CategoryVM SelectedCategory
        {
            get { return _selectedCategory; }
            set
            { 
                SetProperty(ref _selectedCategory, value, "SelectedCategory");

                if (_selectedCategory == null) return;

                SelectedItem = null;
                SelectedSupplier = null;
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, "SelectedItem");

                if (_selectedItem == null) return;

                SelectedCategory = null;
                SelectedSupplier = null;
            }
        }

        public SupplierVM SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");

                if (_selectedSupplier == null) return;
                
                SelectedCategory = null;
                SelectedItem = null;
            }
        }

        public ItemVM SelectedLine
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
                    UpdateDisplayedItems();
                    UpdateCategories();
                    UpdateItems();
                    UpdateSuppliers();
                }));
            }
        }

        public ICommand ActivateItemCommand
        {
            get
            {
                return _activateItemCommand ?? (_activateItemCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected() || !IsConfirmationYes()) return;
                    if (_selectedLine.Active) DeactivateItemInDatabase(_selectedLine.Model);
                    else ActivateItemInDatabase(_selectedLine.Model);
                    _selectedLine.Active = !IsActiveChecked;
                }));
            }
        }

        public ICommand EditItemCommand
        {
            get
            {
                return _editItemCommand ?? (_editItemCommand = new RelayCommand(() =>
                {
                    if (!IsEditItemSelected()) return;
                    ShowEditWindow();
                    UpdateItems();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void ShowEditWindow()
        {
            var vm = new MasterInventoryEditVM(_selectedLine);
            var editWindow = new MasterInventoryEditView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editWindow.ShowDialog();
        }

        private bool IsEditItemSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select an item to edit.", "No Selection", MessageBoxButton.OK);
            return false;
        }

        public void UpdateCategories()
        {
            var oldSelectedCategory = _selectedCategory;

            Categories.Clear();
            CategoriesWithAll.Clear(); 
            CategoriesWithAll.Add(new CategoryVM { Model = new Category { Name = "All" } });

            using (var context = new ERPContext())
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name).ToList();
                foreach (
                    var categoryVM in
                        categoriesFromDatabase.Select(category => new CategoryVM {Model = category})
                            .Where(categoryVM => !CategoriesWithAll.Contains(categoryVM)))
                {
                    CategoriesWithAll.Add(categoryVM);
                    Categories.Add(categoryVM);
                }
            }

            UpdateSelectedCategory(oldSelectedCategory);
        }

        private void UpdateSelectedCategory(CategoryVM oldSelectedCategory)
        {
            if (oldSelectedCategory == null) return;
            SelectedCategory = CategoriesWithAll.FirstOrDefault(category => category.ID.Equals(oldSelectedCategory.ID));
        }

        public void UpdateItems()
        {
            var oldSelectedItem = _selectedItem;

            Items.Clear();

            using (var context = new ERPContext())
            {
                var itemsFromDatabase =
                    context.Inventory.Include("Suppliers").Include("AlternativeSalesPrices").Include("Category").OrderBy(item => item.Name);
                foreach (var item in itemsFromDatabase)
                    Items.Add(new ItemVM {Model = item, SelectedSupplier = item.Suppliers.FirstOrDefault()});
            }

            UpdateSelectedItem(oldSelectedItem);
        }

        private void UpdateSelectedItem(ItemVM oldSelectedItem)
        {
            if (oldSelectedItem == null) return;
            SelectedItem = Items.FirstOrDefault(item => item.ID == oldSelectedItem.ID);
        }

        public void UpdateSuppliers()
        {
            var oldSelectedSupplier = _selectedSupplier;

            Suppliers.Clear();
            using (var context = new ERPContext())
            {
                var suppliersFromDatabase =
                    context.Suppliers.Where(supplier => !supplier.Name.Equals("-")).OrderBy(supplier => supplier.Name);
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

        private void DisplayCategoryItems()
        {
            if (_selectedCategory.Name == "All")
            {
                foreach (var item in Items.Where(item => item.Active.Equals(_isActiveChecked)))
                    DisplayedItems.Add(item);
            }

            else
            {
                foreach (
                    var item in
                        Items.Where(item => item.Category.ID.Equals(_selectedCategory.ID) && item.Active.Equals(_isActiveChecked)))
                    DisplayedItems.Add(item);
            }
        }

        private void DisplaySupplierItems()
        {
            foreach (
                var item in
                    Items.Where(item => item.Suppliers.Contains(_selectedSupplier.Model) && item.Active.Equals(_isActiveChecked)))
                DisplayedItems.Add(item);
        }

        public void UpdateDisplayedItems()
        {
            UpdateItems();
            DisplayedItems.Clear();
            if (_selectedCategory != null) DisplayCategoryItems();
            else if (_selectedItem != null)
            {
                if (_selectedItem.Active.Equals(_isActiveChecked)) DisplayedItems.Add(_selectedItem);
            }
            else if (_selectedSupplier != null) DisplaySupplierItems();
        }

        public static void DeactivateItemInDatabase(Item item)
        {
            using (var context = new ERPContext())
            {
                context.Entry(item).State = EntityState.Modified;
                item.Active = false;
                context.SaveChanges();
            }
        }

        public static void ActivateItemInDatabase(Item item)
        {
            using (var context = new ERPContext())
            {
                context.Entry(item).State = EntityState.Modified;
                item.Active = true;
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
            return MessageBox.Show("Confirm activating/deactivating item?", "Confirmation", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
        #endregion
    }
}
