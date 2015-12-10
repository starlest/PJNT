using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PutraJayaNT.ViewModels.Reports
{
    class InventoryReportVM : ViewModelBase
    {
        ObservableCollection<ItemVM> _items;
        List<Category> _categories;
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<Warehouse> _warehouses;

        ObservableCollection<ItemVM> _displayedItems;

        ItemVM _selectedItem;
        Category _selectedCategory;
        Supplier _selectedSupplier;
        Warehouse _selectedWarehouse;

        public InventoryReportVM()
        {
            _items = new ObservableCollection<ItemVM>();
            _categories = new List<Category>();
            _suppliers = new ObservableCollection<Supplier>();
            _warehouses = new ObservableCollection<Warehouse>();
            _displayedItems = new ObservableCollection<ItemVM>();

            UpdateWarehouses();
            UpdateCategories();
            SelectedCategory = _categories.Find(e => e.Name == "All");
        }

        public ObservableCollection<ItemVM> Items
        {
            get
            {
                UpdateItems();
                return _items;
            }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get
            {
                UpdateSuppliers();
                return _suppliers;
            }
        }

        public ObservableCollection<Warehouse> Warehouses
        {
            get { return _warehouses; }
        }

        public ObservableCollection<ItemVM> DisplayedItems
        {
            get { return _displayedItems; }
        }

        public List<Category> Categories
        {
            get
            {
                UpdateCategories();
                return _categories;
            }
        }

        public Category SelectedCategory
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
                SelectedWarehouse = null;

                UpdateItems();

                SetProperty(ref _selectedCategory, value, () => SelectedCategory);

                _displayedItems.Clear();

                if (_selectedCategory.Name == "All")
                {
                    foreach (var item in _items)
                    {
                        _displayedItems.Add(item);
                    }
                }

                else
                {
                    foreach (var item in _items)
                    {
                        if (item.Category.Name == _selectedCategory.Name) _displayedItems.Add(item);
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
                SelectedWarehouse = null;
                UpdateItems();

                if (_items.Contains(value))
                {
                    _displayedItems.Clear();
                    foreach (var item in _items)
                    {
                        if (item.ID == value.ID)
                        {
                            _displayedItems.Add(item);
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
                SelectedWarehouse = null;
                UpdateSuppliers();

                if (_suppliers.Contains(value))
                {
                    UpdateItems();
                    SetProperty(ref _selectedSupplier, value, () => SelectedSupplier);
                    _displayedItems.Clear();

                    foreach (var item in _items)
                    {
                        if (item.Suppliers.Contains(value))
                            _displayedItems.Add(item);
                    }
                }
            }
        }

        public Warehouse SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedWarehouse, null, () => SelectedWarehouse);
                    return;
                }
                SelectedSupplier = null;
                SelectedCategory = null;
                SelectedItem = null;

                if (_warehouses.Contains(value))
                {
                    UpdateItems();
                    SetProperty(ref _selectedWarehouse, value, () => SelectedWarehouse);
                    _displayedItems.Clear();

                    foreach (var item in _items)
                    {
                        foreach (var stock in item.Stocks)
                        {
                            if (stock.Warehouse.Equals(value))
                                _displayedItems.Add(item);
                        }     
                    }
                }
            }
        }

        private void UpdateCategories()
        {
            _categories.Clear();

            // Load all categories for selection
            using (var uow = new UnitOfWork())
            {
                _categories.Add(new Category { Name = "All" });

                var categories = uow.CategoryRepository.GetAll();
                foreach (var category in categories)
                {
                    if (!_categories.Contains(category))
                    {
                        _categories.Add(category);
                    }
                }
            }
        }

        private void UpdateItems()
        {
            // Update items from database
            _items.Clear();

            using (var context = new ERPContext())
            {
                var items = context.Inventory
                    .Include("Suppliers")
                    .Include("Category")
                    .Include("Stocks")
                    .Include("Stocks.Warehouse");

                foreach (var item in items)
                {
                    if (item.Active == true)
                        _items.Add(new ItemVM { Model = item, SelectedSupplier = item.Suppliers.FirstOrDefault() });
                }
            }
        }

        private void UpdateSuppliers()
        {
            // Update suppliers from database
            _suppliers.Clear();

            using (var uow = new UnitOfWork())
            {
                var suppliers = uow.SupplierRepository.GetAll();
                foreach (var supplier in suppliers)
                {
                    _suppliers.Add(supplier);
                }
            }
        }

        private void UpdateWarehouses()
        {
            // Update warehouses from database
            _warehouses.Clear();

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();

                foreach (var warehouse in warehouses)
                {
                    _warehouses.Add(warehouse);
                }
            }
        }
    }
}
