namespace PutraJayaNT.ViewModels.Reports
{
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using PutraJayaNT.Reports.Windows;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Suppliers;
    using Utilities;
    using System;
    using Item;

    public class InventoryReportVM : ViewModelBase
    {
        #region Backing Fields
        private bool _isItemActiveChecked;
        private bool _isSupplierActiveChecked;
        private ItemVM _selectedItem;
        private CategoryVM _selectedCategory;
        private SupplierVM _selectedSupplier;
        private WarehouseVM _selectedWarehouse;
        private decimal _total;

        private ICommand _displayCommand;
        private ICommand _clearCommand;
        private ICommand _printCommand;
        #endregion

        public InventoryReportVM()
        {
            ListedItems = new ObservableCollection<ItemVM>();
            Categories = new ObservableCollection<CategoryVM>();
            Suppliers = new ObservableCollection<SupplierVM>();
            Warehouses = new ObservableCollection<WarehouseVM>();
            DisplayedLines = new ObservableCollection<InventoryReportLineVM>();

            _isItemActiveChecked = true;
            _isSupplierActiveChecked = true;

            UpdateWarehouses();
            UpdateCategories();
            UpdateSuppliers();
        }

        #region Collections
        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<CategoryVM> Categories { get; }

        public ObservableCollection<SupplierVM> Suppliers { get; }

        public ObservableCollection<ItemVM> ListedItems { get; }

        public ObservableCollection<InventoryReportLineVM> DisplayedLines { get; }
        #endregion

        #region Properties
        public bool IsItemActiveChecked
        {
            get { return _isItemActiveChecked; }
            set
            {
                SetProperty(ref _isItemActiveChecked, value, () => IsItemActiveChecked);
                if (ListedItems.Count > 0) UpdateListedItems();
            }
        }

        public bool IsSupplierActiveChecked
        {
            get { return _isSupplierActiveChecked; }
            set
            {
                SetProperty(ref _isSupplierActiveChecked, value, () => IsSupplierActiveChecked);
                if (_selectedSupplier != null) return;
                UpdateSuppliers();
                ListedItems.Clear();
            }
        }

        public WarehouseVM SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set { SetProperty(ref _selectedWarehouse, value, "SelectedWarehouse"); }
        }

        public CategoryVM SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                SetProperty(ref _selectedCategory, value, "SelectedCategory");
                if (_selectedCategory == null) return;
                SelectedSupplier = null;
                UpdateListedItems();
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
                UpdateListedItems();
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value, "SelectedItem"); }
        }

        public decimal Total
        {
            get
            {
                _total = 0;
                foreach (var line in DisplayedLines)
                {
                    _total += line.InventoryValue;
                }
                return _total;
            }
            set { SetProperty(ref _total, value, "Total"); }
        }
        #endregion

        #region Commands
        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (_selectedItem != null) UpdateDisplayedLines();
                }));
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new RelayCommand(() =>
                {
                    ClearSelections();
                    DisplayedLines.Clear();
                    UpdateWarehouses();
                    UpdateCategories();
                    UpdateSuppliers();
                }));
            }
        }

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (DisplayedLines.Count == 0) return;

                    var inventoryReportWindow = new InventoryReportWindow(this)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    inventoryReportWindow.Show();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateWarehouses()
        {
            var oldSelectedWarehouse = _selectedWarehouse;

            Warehouses.Clear();
            var allWarehouse = new Warehouse { ID = -1, Name = "All" };
            Warehouses.Add(new WarehouseVM { Model = allWarehouse });

            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var warehousesFromDatabase = context.Warehouses.OrderBy(warehouse => warehouse.Name);
                foreach (var warehouse in warehousesFromDatabase)
                    Warehouses.Add(new WarehouseVM {Model = warehouse});
            }

            UpdateSelectedWarehouse(oldSelectedWarehouse);
        }

        private void UpdateSelectedWarehouse(WarehouseVM oldSelectedWarehouse)
        {
            if (oldSelectedWarehouse == null) return;
            SelectedWarehouse = oldSelectedWarehouse;
        }

        private void UpdateCategories()
        {
            var oldSelectedCategory = _selectedCategory;

            Categories.Clear();
            var allCategory = new Category {ID = -1, Name = "All"};
            Categories.Add(new CategoryVM { Model = allCategory });

            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
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

        private void UpdateSuppliers()
        {
            var oldSelectedSupplier = _selectedSupplier;

            Suppliers.Clear();
            Suppliers.Add(new SupplierVM { Model = new Supplier { ID = -1, Name = "All" } });

            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var suppliersFromDatabase = context.Suppliers.Where(supplier => !supplier.Name.Equals("-")).OrderBy(supplier => supplier.Name);
                foreach (
                    var supplier in
                        suppliersFromDatabase.Where(supplier => supplier.Active.Equals(_isSupplierActiveChecked)))
                    Suppliers.Add(new SupplierVM {Model = supplier});
            }

            UpdateSelectedSupplier(oldSelectedSupplier);
        }

        private void UpdateSelectedSupplier(SupplierVM oldSelectedSupplier)
        {
            if (oldSelectedSupplier == null) return;
            SelectedSupplier = Suppliers.FirstOrDefault(supplier => supplier.ID.Equals(oldSelectedSupplier.ID));
        }

        private void UpdateListedItems()
        {
            ListedItems.Clear();
            var allItem = new Item { ItemID = "All", Name = "All" };
            ListedItems.Add(new ItemVM { Model = allItem });

            var searchCondition = _selectedCategory != null ? GetSearchConditionAccordingToSelectedCategory() : GetSearchConditionAccordingToSelectedSupplier();

            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var itemsFromDatabase = context.Inventory.Include("Category").Include("Suppliers").Where(searchCondition).OrderBy(item => item.Name);
                foreach (var item in itemsFromDatabase)
                    ListedItems.Add(new ItemVM {Model = item, SelectedSupplier = item.Suppliers.FirstOrDefault()});
            }
        }

        private Func<Item, bool> GetSearchConditionAccordingToSelectedCategory()
        {
            if (_selectedCategory.Name.Equals("All"))
                return item => item.Active.Equals(_isItemActiveChecked);
            return item => item.Category.ID.Equals(_selectedCategory.ID) && item.Active.Equals(_isItemActiveChecked);
        }

        private Func<Item, bool> GetSearchConditionAccordingToSelectedSupplier()
        {
            if (_selectedSupplier.Name.Equals("All"))
                return item => item.Active.Equals(_isItemActiveChecked);
            return item => item.Suppliers.Contains(_selectedSupplier.Model) && item.Active.Equals(_isItemActiveChecked);
        }

        private void UpdateDisplayedLines()
        {
            DisplayedLines.Clear();

            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                if (_selectedWarehouse.Name.Equals("All") && _selectedItem.Name.Equals("All"))
                    DisplayAllItems(context);

                else if (_selectedWarehouse.Name.Equals("All") && !_selectedItem.Name.Equals("All"))
                    DisplaySelectedItemFromAllWarehouses(context);

                else if (!_selectedWarehouse.Name.Equals("All") && _selectedItem.Name.Equals("All"))
                    DisplayListedItemsFromSelectedWarehouse(context);

                else
                    DisplaySelectedItemFromSelectedWarehouse(context);
            }

            UpdateUITotal();
        }

        private void DisplayAllItems(ERPContext context)
        {
                foreach (var item in ListedItems)
                {
                    if (item.Name.Equals("All")) continue;
                    var line = new InventoryReportLineVM(item.Model);

                    var stocks = context.Stocks.Where(stock => stock.ItemID.Equals(item.ID));
                    if (stocks.Any())
                    {
                        foreach (var stock in stocks)
                            line.Quantity += stock.Pieces;
                    }

                    DisplayedLines.Add(line);
                }
        }

        private void DisplaySelectedItemFromAllWarehouses(ERPContext context)
        {
            var line = new InventoryReportLineVM(_selectedItem.Model);
            var stocks = context.Stocks.Where(stock => stock.ItemID.Equals(_selectedItem.ID));
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                    line.Quantity += stock.Pieces;
            }
            DisplayedLines.Add(line);
        }

        private void DisplayListedItemsFromSelectedWarehouse(ERPContext context)
        {
            foreach (var item in ListedItems)
            {
                if (item.Name.Equals("All")) continue;
                var line = new InventoryReportLineVM(item.Model);
                var stocks = context.Stocks.Where(stock => stock.ItemID.Equals(item.ID) && stock.WarehouseID.Equals(_selectedWarehouse.ID));
                if (stocks.Any())
                {
                    foreach (var stock in stocks)
                        line.Quantity += stock.Pieces;
                }
                DisplayedLines.Add(line);
            }
        }

        private void DisplaySelectedItemFromSelectedWarehouse(ERPContext context)
        {
            var line = new InventoryReportLineVM(_selectedItem.Model);
            var stocks = context.Stocks.Where(stock => stock.ItemID.Equals(_selectedItem.ID) && stock.WarehouseID.Equals(_selectedWarehouse.ID));
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                    line.Quantity += stock.Pieces;
            }
            DisplayedLines.Add(line);
        }

        private void ClearSelections()
        {
            SelectedWarehouse = null;
            SelectedSupplier = null;
            SelectedCategory = null;
            SelectedItem = null;
        }

        private void UpdateUITotal()
        {
            OnPropertyChanged("Total");
        }
        #endregion
    }
}
