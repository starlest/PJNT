using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PutraJayaNT.Views.Master.Inventory;

namespace PutraJayaNT.ViewModels.Master.Inventory
{
    public class MasterInventoryVM : ViewModelBase
    {
        private ItemVM _selectedItem;
        private CategoryVM _selectedCategory;
        private Supplier _selectedSupplier;
        private ItemVM _selectedLine;

        private ICommand _editItemCommand;
        private ICommand _activateCommand;

        public MasterInventoryVM()
        {
            Items = new ObservableCollection<ItemVM>();
            CategoriesWithoutAll = new ObservableCollection<CategoryVM>();
            Categories = new ObservableCollection<CategoryVM>();
            Suppliers = new ObservableCollection<Supplier>();
            DisplayedItems = new ObservableCollection<ItemVM>();
            UpdateItems();
            UpdateCategories();
            UpdateSuppliers();
            SelectedCategory = Categories.FirstOrDefault();

            NewEntryVM = new MasterInventoryNewEntryVM(this);
        }

        public MasterInventoryNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<ItemVM> Items { get; }

        public ObservableCollection<Supplier> Suppliers { get; }

        public ObservableCollection<ItemVM> DisplayedItems { get; }

        public ObservableCollection<CategoryVM> Categories { get; }

        public ObservableCollection<CategoryVM> CategoriesWithoutAll { get; }
        #endregion

        public CategoryVM SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedCategory, null, () => SelectedCategory);
                    return;
                }

                UpdateCategories();
                SelectedItem = null;
                SelectedSupplier = null;

                UpdateItems();

                foreach (var category in Categories)
                {
                    if (value.ID.Equals(category.ID))
                    {
                        value = category;
                        break;
                    }
                }

                SetProperty(ref _selectedCategory, value, () => SelectedCategory);

                DisplayedItems.Clear();

                if (_selectedCategory.Name == "All")
                {
                    foreach (var item in Items)
                    {
                        DisplayedItems.Add(item);
                    }
                }

                else
                {
                    foreach (var item in Items)
                    {
                        if (item.Category.Name == _selectedCategory.Name) DisplayedItems.Add(item);
                    }
                }
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedItem, null, () => SelectedItem);
                    return;
                }

                SelectedCategory = null;
                SelectedSupplier = null;
                UpdateItems();

                if (Items.Contains(value))
                {
                    DisplayedItems.Clear();
                    foreach (var item in Items)
                    {
                        if (item.ID == value.ID)
                        {
                            DisplayedItems.Add(item);
                            SetProperty(ref _selectedItem, item, () => SelectedItem);
                            break;
                        }
                    }
                }
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedSupplier, null, () => SelectedSupplier);
                    return;
                }
                SelectedCategory = null;
                SelectedItem = null;
                UpdateSuppliers();

                if (Suppliers.Contains(value))
                {
                    UpdateItems();
                    SetProperty(ref _selectedSupplier, value, () => SelectedSupplier);
                    DisplayedItems.Clear();

                    foreach (var item in Items)
                    {
                        if (item.Suppliers.Contains(value))
                            DisplayedItems.Add(item);
                    }
                }
            }
        }

        public ItemVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        public ICommand EditItemCommand
        {
            get
            {
                return _editItemCommand ?? (_editItemCommand = new RelayCommand(() =>
                {
                    if (!IsEditItemSelected()) return;
                    ShowEditWindow();
                }));
            }
        }

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

        public ICommand ActivateCommand
        {
            get
            {
                return _activateCommand ?? (_activateCommand = new RelayCommand(() =>
                {
                    if (_selectedLine != null && MessageBox.Show(string.Format("Confirm activation/deactivation of {0}?", SelectedLine.Name), "Confirmation", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                    {
                        using (var context = new ERPContext())
                        {
                            var item = context.Inventory
                            .FirstOrDefault(e => e.ItemID.Equals(_selectedLine.ID));

                            if (_selectedLine.Active == true)
                            {
                                _selectedLine.Active = false;
                                _selectedLine.Suppliers.Clear();
                                item.Active = false;
                                foreach (var supplier in item.Suppliers.ToList())
                                    item.Suppliers.Remove(supplier);
                            }
                            else
                            {
                                _selectedLine.Active = true;
                                item.Active = true;
                            }

                            context.SaveChanges();
                        }
                    }
                }));
            }
        }

        private void UpdateCategories()
        {
            Categories.Clear();
            CategoriesWithoutAll.Clear();

            // Load all categories for selection
            using (var uow = new UnitOfWork())
            {
                Categories.Add(new CategoryVM { Model= new Category { Name = "All" }});

                var categories = uow.CategoryRepository.GetAll().OrderBy(e => e.Name);
                foreach (var category in categories)
                {
                    var categoryVM = new CategoryVM {Model = category};
                    if (!Categories.Contains(categoryVM))
                    {
                        Categories.Add(categoryVM);
                        CategoriesWithoutAll.Add(categoryVM);
                    }
                }
            }
        }

        public void UpdateItems()
        {
            // Update items from database
            Items.Clear();

            using (var context = new ERPContext())
            {
                var items = context.Inventory
                    .Include("AlternativeSalesPrices")
                    .Include("Category")
                    .Include("Suppliers")
                    .OrderBy(e => e.Category.Name)
                    .ThenBy(e => e.ItemID)
                    .ToList();

                foreach (var item in items)
                {
                    Items.Add(new ItemVM { Model = item, SelectedSupplier = item.Suppliers.FirstOrDefault() });
                }

                UpdateSuppliers();
            }
        }

        private void UpdateSuppliers()
        {
            // Update suppliers from database
            Suppliers.Clear();

            using (var uow = new UnitOfWork())
            {
                var suppliers = uow.SupplierRepository.GetAll().OrderBy(supplier => supplier.Name);
                foreach (var supplier in suppliers)
                {
                    if (supplier.Active == true && supplier.Name != "-")
                        Suppliers.Add(supplier);
                }
            }
        }
        #endregion
    }
}
