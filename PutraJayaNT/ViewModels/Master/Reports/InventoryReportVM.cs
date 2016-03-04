using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Reports.Windows;
using PutraJayaNT.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Reports
{
    public class InventoryReportVM : ViewModelBase
    {
        ObservableCollection<ItemVM> _items;
        ObservableCollection<Category> _categories;
        ObservableCollection<SupplierVM> _suppliers;
        ObservableCollection<WarehouseVM> _warehouses;

        ObservableCollection<InventoryReportLineVM> _lines;

        ItemVM _selectedItem;
        Category _selectedCategory;
        SupplierVM _selectedSupplier;
        WarehouseVM _selectedWarehouse;

        ICommand _printCommand;

        decimal _total;

        public InventoryReportVM()
        {
            _items = new ObservableCollection<ItemVM>();
            _categories = new ObservableCollection<Category>();
            _suppliers = new ObservableCollection<SupplierVM>();
            _warehouses = new ObservableCollection<WarehouseVM>();
            _lines = new ObservableCollection<InventoryReportLineVM>();

            UpdateWarehouses();
            UpdateCategories();
            UpdateSuppliers();
        }

        #region Collections
        public ObservableCollection<ItemVM> Items
        {
            get
            {
                return _items;
            }
        }

        public ObservableCollection<SupplierVM> Suppliers
        {
            get
            {
                return _suppliers;
            }
        }

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }

        public ObservableCollection<InventoryReportLineVM> Lines
        {
            get { return _lines; }
        }

        public ObservableCollection<Category> Categories
        {
            get
            {
                return _categories;
            }
        }
        #endregion

        public WarehouseVM SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set
            {
                if (value == null) return;

                UpdateWarehouses();
                SetProperty(ref _selectedWarehouse, value, "SelectedWarehouse");
            }
        }

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (value == null && _selectedSupplier == null) return;

                UpdateCategories();
                SetProperty(ref _selectedCategory, value, "SelectedCategory");

                if (_selectedCategory == null) return;

                SelectedSupplier = null;
                UpdateItems();
            }
        }

        public SupplierVM SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                if (value == null && _selectedCategory == null) return;

                UpdateSuppliers();
                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");

                if (_selectedSupplier == null) return;

                SelectedCategory = null;
                UpdateItems();
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, "SelectedItem");

                if (_selectedItem == null) return;

                UpdateLines();
            }
        }

        public decimal Total
        {
            get
            {
                _total = 0;
                foreach (var line in _lines)
                {
                    _total += line.InventoryValue;
                }
                return _total;
            }
            set { SetProperty(ref _total, value, "Total"); }
        }


        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (_lines.Count == 0) return;

                    var inventoryReportWindow = new InventoryReportWindow(this);
                    inventoryReportWindow.Owner = App.Current.MainWindow;
                    inventoryReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    inventoryReportWindow.Show();
                }));
            }
        }

        #region Helper Methods
        private void UpdateWarehouses()
        {
            _warehouses.Clear();

            var allWarehouse = new Warehouse { ID = -1, Name = "All" };
            _warehouses.Add(new WarehouseVM { Model = allWarehouse });

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.OrderBy(e => e.Name).ToList();

                foreach (var warehouse in warehouses)
                    _warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void UpdateCategories()
        {
            _categories.Clear();
            _categories.Add(new Category { ID = -1, Name = "All" });

            using (var context = new ERPContext())
            {
                var categories = context.Categories.ToList();

                foreach (var category in categories)
                    _categories.Add(category);
            }
        }

        private void UpdateSuppliers()
        {
            _suppliers.Clear();
            _suppliers.Add(new SupplierVM { Model = new Supplier { ID = -1, Name = "All" } });

            using (var context = new ERPContext())
            {
                var suppliers = context.Suppliers.ToList();

                foreach (var supplier in suppliers)
                    _suppliers.Add(new SupplierVM { Model = supplier });
            }
        }

        private void UpdateItems()
        {
            _items.Clear();
            _items.Add(new ItemVM { Model = new Item { ItemID = "All", Name = "All" } });
            using (var context = new ERPContext())
            {
                List<Item> items;

                if (_selectedCategory != null)
                {
                    if (_selectedCategory.Name.Equals("All"))
                    {
                        items = context.Inventory
                            .Include("Suppliers")
                            .Include("Category")
                            .Include("Stocks")
                            .Include("Stocks.Warehouse")
                            .Where(e => e.Active.Equals(true))
                            .OrderBy(e => e.ItemID)
                            .ToList();
                    }

                    else
                    {
                        items = context.Inventory
                            .Include("Suppliers")
                            .Include("Category")
                            .Include("Stocks")
                            .Include("Stocks.Warehouse")
                            .Where(e => e.Category.ID.Equals(_selectedCategory.ID) && e.Active.Equals(true))
                            .OrderBy(e => e.ItemID)
                            .ToList();
                    }
                }

                else
                {
                    var allItems = context.Inventory
                        .Include("Suppliers")
                        .Include("Category")
                        .Include("Stocks")
                        .Include("Stocks.Warehouse")
                        .Where(e => e.Active.Equals(true))
                        .OrderBy(e => e.ItemID)
                        .ToList();

                    if (_selectedSupplier.Name.Equals("All"))
                    {
                        items = allItems;
                    }

                    else
                    {
                        items = new List<Item>();
                        foreach (var i in allItems)
                        {
                            if (i.Suppliers.Contains(_selectedSupplier.Model))
                            {
                                items.Add(i);
                            }
                        }
                    }
                }

                foreach (var item in items)
                {
                    if (item.Active == true)
                        _items.Add(new ItemVM { Model = item, SelectedSupplier = item.Suppliers.FirstOrDefault() });
                }
            }
        }

        private void UpdateLines()
        {
            _lines.Clear();
            using (var context = new ERPContext())
            {
                if (_selectedWarehouse.Name.Equals("All") && _selectedItem.Name.Equals("All"))
                {
                    foreach (var item in _items)
                    {
                        if (!item.Name.Equals("All"))
                        {
                            var line = new InventoryReportLineVM(item.Model);
                            var stocks = context.Stocks.Where(e => e.ItemID.Equals(item.ID)).ToList();

                            if (stocks.Count > 0)
                            {
                                foreach (var stock in stocks)
                                {
                                    line.Quantity += stock.Pieces;
                                }
                            }
                            _lines.Add(line);
                        }
                    }
                }

                else if (_selectedWarehouse.Name.Equals("All") && !_selectedItem.Name.Equals("All"))
                {
                    var line = new InventoryReportLineVM(_selectedItem.Model);
                    var stocks = context.Stocks.Where(e => e.ItemID.Equals(_selectedItem.ID)).ToList();

                    if (stocks.Count > 0)
                    {
                        foreach (var stock in stocks)
                        {
                            line.Quantity += stock.Pieces;
                        }
                    }
                    _lines.Add(line);
                }

                else if (!_selectedWarehouse.Name.Equals("All") && _selectedItem.Name.Equals("All"))
                {
                    foreach (var item in _items)
                    {
                        if (!item.Name.Equals("All"))
                        {
                            var line = new InventoryReportLineVM(item.Model);
                            var stocks = context.Stocks.Where(e => e.ItemID.Equals(item.ID) && e.WarehouseID.Equals(_selectedWarehouse.ID)).ToList();

                            if (stocks.Count > 0)
                            {
                                foreach (var stock in stocks)
                                {
                                    line.Quantity += stock.Pieces;
                                }
                            }
                            _lines.Add(line);
                        }
                    }
                }

                else
                {
                    var line = new InventoryReportLineVM(_selectedItem.Model);
                    var stock = context.Stocks.Where(e => e.ItemID.Equals(_selectedItem.ID) && e.WarehouseID.Equals(_selectedWarehouse.ID)).FirstOrDefault();
                    if (stock != null) line.Quantity = stock.Pieces;
                    _lines.Add(line);
                }

                OnPropertyChanged("Total");
            }
        }
        #endregion
    }
}
