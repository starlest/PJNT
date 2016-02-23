using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Master
{
    class MasterInventoryVM : ViewModelBase
    {
        ObservableCollection<ItemVM> _items;
        ObservableCollection<Category> _categories;
        ObservableCollection<Category> _categoriesWithoutAll;
        ObservableCollection<Supplier> _suppliers;

        ObservableCollection<ItemVM> _displayedItems;

        ItemVM _selectedItem;
        Category _selectedCategory;
        Supplier _selectedSupplier;
        ItemVM _selectedLine;

        string _newEntryID;
        string _newEntryName;
        Category _newEntryCategory;
        Supplier _newEntrySupplier;
        int _newEntryPiecesPerUnit;
        string _newEntryUnitName;
        decimal? _newEntryPurchasePrice;
        decimal? _newEntrySalesPrice;
        ICommand _newEntryCommand;
        ICommand _cancelEntryCommand;

        ICommand _activateCommand;

        // Edit Window Properties
        ICommand _editCommand;

        ObservableCollection<Supplier> _editAddSuppliers;
        Supplier _editAddSelectedSupplier;
        ICommand _editAddSupplierCommand;
        ICommand _editDeleteSupplierCommand;
        ICommand _editAddSupplierConfirmCommand;
        ICommand _editAddSupplierCancelCommand;
        Visibility _editAddSupplierWindowVisibility;
        bool _isEditAddSupplierWindowNotOpen;

        Visibility _editAddAlternativeSalesPriceWindowVisibility;
        ObservableCollection<AlternativeSalesPrice> _editAlternativeSalesPrices;
        AlternativeSalesPrice _editSelectedAlternativeSalesPrice;
        string _editAddAlternativeSalesPriceName;
        decimal? _editAddAlternativeSalesPriceSalesPrice;
        ICommand _editAddAlternativeSalesPriceCommand;
        ICommand _editDeleteAlternativeSalesPriceCommand;
        ICommand _editAddAlternativeSalesPriceCancelCommand;
        ICommand _editAddAlternativeSalesPriceConfirmCommand;

        ICommand _cancelEditCommand;
        ICommand _confirmEditCommand;

        ObservableCollection<Supplier> _editSuppliers;
        string _editID;
        string _editName;
        Category _editCategory;
        decimal _editPurchasePrice;
        decimal _editSalesPrice;
        decimal _editSalesExpense;
        Supplier _editSelectedSupplier;
        string _editUnitName;
        int _editPiecesPerUnit;

        bool _isEditWindowNotOpen;
        Visibility _editWindowVisibility;

        public MasterInventoryVM()
        {
            _items = new ObservableCollection<ItemVM>();
            _categoriesWithoutAll = new ObservableCollection<Category>();
            _categories = new ObservableCollection<Category>();
            _suppliers = new ObservableCollection<Supplier>();
            _displayedItems = new ObservableCollection<ItemVM>();

            UpdateCategories();
            SelectedCategory = _categories.FirstOrDefault();

            _editSuppliers = new ObservableCollection<Supplier>();
            _editWindowVisibility = Visibility.Collapsed;
            _isEditWindowNotOpen = true;

            _editAddAlternativeSalesPriceWindowVisibility = Visibility.Collapsed;

            _editAddSuppliers = new ObservableCollection<Supplier>();
            _editAddSupplierWindowVisibility = Visibility.Collapsed;
            _isEditAddSupplierWindowNotOpen = true;

            _editAlternativeSalesPrices = new ObservableCollection<AlternativeSalesPrice>();

            _newEntryPiecesPerUnit = 1;
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

        public ObservableCollection<ItemVM> DisplayedItems
        {
            get { return _displayedItems; }
        }

        public ObservableCollection<Category> Categories
        {
            get
            {
                UpdateCategories();
                return _categories;
            }
        }

        public ObservableCollection<Category> CategoriesWithoutAll
        {
            get
            {
                UpdateCategories();
                return _categoriesWithoutAll;
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

                UpdateItems();

                foreach (var category in _categories)
                {
                    if (value.ID.Equals(category.ID))
                    {
                        value = category;
                        break;
                    }
                }

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

        public ItemVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        #region New Entry Properties
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

        public Category NewEntryCategory
        {
            get { return _newEntryCategory; }
            set { SetProperty(ref _newEntryCategory, value, "NewEntryCategory"); }
        }

        public Supplier NewEntrySupplier
        {
            get { return _newEntrySupplier; }
            set { SetProperty(ref _newEntrySupplier, value, "NewEntrySupplier"); }
        }

        public int NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, "NewEntryPiecesPerUnit"); }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, "NewEntryUnitName"); }
        }

        public decimal? NewEntrySalesPrice
        {
            get { return _newEntrySalesPrice; }
            set { SetProperty(ref _newEntrySalesPrice, value, "NewEntrySalesPrice"); }
        }

        public decimal? NewEntryPurchasePrice
        {
            get { return _newEntryPurchasePrice; }
            set { SetProperty(ref _newEntryPurchasePrice, value, "NewEntryPurchasePrice"); }
        }

        public ICommand CancelEntryCommand
        {
            get
            {
                return _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(() =>
                {
                    ResetEntryFields();
                    UpdateCategories();
                    UpdateItems();
                    UpdateSuppliers();
                }));
            }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (_newEntryID == null || _newEntryName == null ||
                    _newEntryCategory == null || _newEntrySupplier == null ||
                    _newEntrySalesPrice == null || _newEntryPurchasePrice == null ||
                    _newEntryPiecesPerUnit <= 0 || _newEntryUnitName == null)
                    {
                        MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm adding this product?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var context = new ERPContext())
                        {
                            var newItem = new Item
                            {
                                ItemID = _newEntryID,
                                Name = _newEntryName,
                                Category = context.Categories.Where(e => e.ID.Equals(_newEntryCategory.ID)).FirstOrDefault(),
                                UnitName = _newEntryUnitName,
                                PiecesPerUnit = _newEntryPiecesPerUnit,
                                PurchasePrice = (decimal)_newEntryPurchasePrice / _newEntryPiecesPerUnit,
                                SalesPrice = (decimal)_newEntrySalesPrice / _newEntryPiecesPerUnit
                            };

                            var newSupplier = context.Suppliers.Where(e => e.ID.Equals(_newEntrySupplier.ID)).FirstOrDefault();
                            newItem.Suppliers.Add(newSupplier);

                            context.Inventory.Add(newItem);

                            context.SaveChanges();
                        }
                        UpdateItems();
                        SelectedCategory = _selectedCategory;
                        ResetEntryFields();
                    }
                }));
            }
        }
        #endregion

        #region Edit Properties
        public ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(() =>
                {
                    if (_selectedLine == null)
                    {
                        MessageBox.Show("Please select a line to edit.", "No Selection", MessageBoxButton.OK);
                        return;
                    }

                    EditID = _selectedLine.ID;
                    EditName = _selectedLine.Name;
                    EditCategory = _categories.Where(e => e.ID == _selectedLine.Category.ID).FirstOrDefault();
                    EditSalesPrice = _selectedLine.SalesPrice;
                    EditPurchasePrice = _selectedLine.PurchasePrice;
                    EditUnitName = _selectedLine.UnitName;
                    EditPiecesPerUnit = _selectedLine.PiecesPerUnit;
                    EditSalesExpense = _selectedLine.SalesExpense;
                    _editSuppliers.Clear();
                    foreach (var supplier in _selectedLine.Suppliers)
                        _editSuppliers.Add(supplier);
                    EditSelectedSupplier = _selectedLine.SelectedSupplier;
                    EditWindowVisibility = Visibility.Visible;
                    IsEditWindowNotOpen = false;
                    UpdateEditAlternativeSalesPrices();
                }));
            }
        }

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

        public Category EditCategory
        {
            get { return _editCategory; }
            set { SetProperty(ref _editCategory, value, () => EditCategory); }
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

        public ObservableCollection<Supplier> EditSuppliers
        {
            get { return _editSuppliers; }
        }

        public Supplier EditSelectedSupplier
        {
            get { return _editSelectedSupplier; }
            set { SetProperty(ref _editSelectedSupplier, value, () => EditSelectedSupplier); }
        }

        #region Edit Add Suppliers Properties
        public ObservableCollection<Supplier> EditAddSuppliers
        {
            get { return _editAddSuppliers; }
        }

        public Supplier EditAddSelectedSupplier
        {
            get { return _editAddSelectedSupplier; }
            set { SetProperty(ref _editAddSelectedSupplier, value, () => EditAddSelectedSupplier); }
        }

        public Visibility EditAddSupplierWindowVisibility
        {
            get { return _editAddSupplierWindowVisibility; }
            set { SetProperty(ref _editAddSupplierWindowVisibility, value, "EditAddSupplierWindowVisibility"); }
        }

        public bool IsEditAddSupplierWindowNotOpen
        {
            get { return _isEditAddSupplierWindowNotOpen; }
            set { SetProperty(ref _isEditAddSupplierWindowNotOpen, value, "IsEditAddSupplierWindowNotOpen"); }
        }

        public ICommand EditAddSupplierCancelCommand
        {
            get
            {
                return _editAddSupplierCancelCommand ?? (_editAddSupplierCancelCommand = new RelayCommand(() =>
                {
                    EditAddSupplierWindowVisibility = Visibility.Collapsed;
                    IsEditAddSupplierWindowNotOpen = true;
                }));
            }
        }

        public ICommand EditAddSupplierConfirmCommand
        {
            get
            {
                return _editAddSupplierConfirmCommand ?? (_editAddSupplierConfirmCommand = new RelayCommand(() =>
                {
                    _editSuppliers.Add(_editAddSelectedSupplier);
                    EditAddSupplierWindowVisibility = Visibility.Collapsed;
                    IsEditAddSupplierWindowNotOpen = true;
                }));
            }
        }
        #endregion

        public ObservableCollection<AlternativeSalesPrice> EditAlternativeSalesPrices
        {
            get { return _editAlternativeSalesPrices; }
        }

        public AlternativeSalesPrice EditSelectedAlternativeSalesPrice
        {
            get { return _editSelectedAlternativeSalesPrice; }
            set { SetProperty(ref _editSelectedAlternativeSalesPrice, value, "EditSelectedAlternativeSalesPrice"); }
        }

        #region Edit Add Alternative Sales Price Properties
        public Visibility EditAddAlternativeSalesPriceWindowVisibility
        {
            get { return _editAddAlternativeSalesPriceWindowVisibility; }
            set { SetProperty(ref _editAddAlternativeSalesPriceWindowVisibility, value, "EditAddAlternativeSalesPriceWindowVisibility"); }
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
                    if (_editAddAlternativeSalesPriceName == null || _editAddAlternativeSalesPriceSalesPrice == null)
                    {
                        MessageBox.Show("Please enter all fields.", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    if (_editAddAlternativeSalesPriceSalesPrice < 0)
                    {
                        MessageBox.Show("Please enter a valid price.", "Invalid Value", MessageBoxButton.OK);
                        return;
                    }

                    using (var context = new ERPContext())
                    {
                        var newAlternativeSalesPrice = new AlternativeSalesPrice
                        {
                            Name = _editAddAlternativeSalesPriceName,
                            Item = context.Inventory.Where(e => e.ItemID.Equals(_selectedLine.ID)).FirstOrDefault(),
                            SalesPrice = (decimal) _editAddAlternativeSalesPriceSalesPrice
                        };

                        _editAlternativeSalesPrices.Add(newAlternativeSalesPrice);
                    }

                    EditAddAlternativeSalesPriceName = null;
                    EditAddAlternativeSalesPriceSalesPrice = null;
                    EditAddAlternativeSalesPriceWindowVisibility = Visibility.Collapsed;
                }));
            }
        }

        public ICommand EditAddAlternativeSalesPriceCancelCommand
        {
            get
            {
                return _editAddAlternativeSalesPriceCancelCommand ?? (_editAddAlternativeSalesPriceCancelCommand = new RelayCommand(() =>
                {
                    EditAddAlternativeSalesPriceWindowVisibility = Visibility.Collapsed;
                }));
            }
        }
        #endregion

        public ICommand EditAddSupplierCommand
        {
            get
            {
                return _editAddSupplierCommand ?? (_editAddSupplierCommand = new RelayCommand(() =>
                {
                    _editAddSuppliers.Clear();
                    foreach (var supplier in _suppliers)
                    {
                        if (!_editSuppliers.Contains(supplier))
                            _editAddSuppliers.Add(supplier);
                    }
                    EditAddSelectedSupplier = _editAddSuppliers.FirstOrDefault();
                    EditAddSupplierWindowVisibility = Visibility.Visible;
                    IsEditAddSupplierWindowNotOpen = false;
                }));
            }
        }

        public ICommand EditDeleteSupplierCommand
        {
            get
            {
                return _editDeleteSupplierCommand ?? (_editDeleteSupplierCommand = new RelayCommand(() =>
                {
                    _editSuppliers.Remove(_editSelectedSupplier);
                }));
            }
        }

        public ICommand EditAddAlternativeSalesPriceCommand
        {
            get
            {
                return _editAddAlternativeSalesPriceCommand ?? (_editAddAlternativeSalesPriceCommand = new RelayCommand(() =>
                {
                    EditAddAlternativeSalesPriceWindowVisibility = Visibility.Visible;
                }));
            }
        }

        public ICommand EditDeleteAlternativeSalesPriceCommand
        {
            get
            {
                return _editDeleteAlternativeSalesPriceCommand ?? (_editDeleteAlternativeSalesPriceCommand = new RelayCommand(() =>
                {
                    if (_editSelectedAlternativeSalesPrice == null)
                    {
                        MessageBox.Show("Please select an alternative sales price first.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    EditAlternativeSalesPrices.Remove(_editSelectedAlternativeSalesPrice);
                }));
            }
        }

        public ICommand ConfirmEditCommand
        {
            get
            {
                return _confirmEditCommand ?? (_confirmEditCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // Update the item in the database
                        using (var context = new ERPContext())
                        {
                            var item = context.Inventory
                            .Where(e => e.ItemID.Equals(_selectedLine.ID))
                            .Include("AlternativeSalesPrices")
                            .Include("Category")
                            .Include("Suppliers")
                            .FirstOrDefault<Item>();

                            item.ItemID = _editID;
                            item.Name = _editName;
                            item.Category = context.Categories
                            .Where(e => e.ID == _editCategory.ID)
                            .FirstOrDefault();
                            item.PurchasePrice = _editPurchasePrice / _editPiecesPerUnit;
                            item.SalesPrice = _editSalesPrice / _editPiecesPerUnit;
                            item.SalesExpense = _editSalesExpense;
                            item.UnitName = _editUnitName;
                            item.PiecesPerUnit = _editPiecesPerUnit;

                            // Adjust the item's suppliers to the edited list of suppliers
                            item.Suppliers.ToList().ForEach(e => item.Suppliers.Remove(e));
                            foreach (var supplier in _editSuppliers.ToList())
                            {
                                var s = context.Suppliers.Where(e => e.ID == supplier.ID).FirstOrDefault();
                                item.Suppliers.Add(s);
                            }

                            // Adjust the item's alt sales prices to the edited list
                            item.AlternativeSalesPrices.ToList().ForEach(e => item.AlternativeSalesPrices.Remove(e));
                            foreach (var altSalesPrice in _editAlternativeSalesPrices)
                            {
                                altSalesPrice.Item = item;
                                context.AlternativeSalesPrices.Add(altSalesPrice);
                            }

                            context.SaveChanges();

                            _selectedLine.Suppliers.Clear();
                            foreach (var supplier in _editSuppliers)
                                _selectedLine.Suppliers.Add(supplier);

                            _selectedLine.ID = _editID;
                            _selectedLine.Name = _editName;
                            _selectedLine.Category = _editCategory;
                            _selectedLine.PurchasePrice = _editPurchasePrice;
                            _selectedLine.SalesPrice = _editSalesPrice;
                            _selectedLine.SalesExpense = _editSalesExpense;
                            _selectedLine.UnitName = _editUnitName;
                            _selectedLine.PiecesPerUnit = _editPiecesPerUnit;
                            _selectedLine.SelectedSupplier = Suppliers.FirstOrDefault();
                        }

                        EditWindowVisibility = Visibility.Collapsed;
                        IsEditWindowNotOpen = true;
                    }
                }));
            }
        }

        public ICommand CancelEditCommand
        {
            get
            {
                return _cancelEditCommand ?? (_cancelEditCommand = new RelayCommand(() =>
                {
                    EditWindowVisibility = Visibility.Collapsed;
                    IsEditWindowNotOpen = true;
                }));
            }
        }

        public Visibility EditWindowVisibility
        {
            get { return _editWindowVisibility; }
            set { SetProperty(ref _editWindowVisibility, value, "EditWindowVisibility"); }
        }

        public bool IsEditWindowNotOpen
        {
            get { return _isEditWindowNotOpen; }
            set { SetProperty(ref _isEditWindowNotOpen, value, "IsEditWindowNotOpen"); }
        }
        #endregion

        #region Helper Methods
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
                            .Where(e => e.ItemID.Equals(_selectedLine.ID))
                            .FirstOrDefault<Item>();

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
            _categories.Clear();
            _categoriesWithoutAll.Clear();

            // Load all categories for selection
            using (var uow = new UnitOfWork())
            {
                _categories.Add(new Category { Name = "All" });

                var categories = uow.CategoryRepository.GetAll().OrderBy(e => e.Name);
                foreach (var category in categories)
                {
                    if (!_categories.Contains(category))
                    {
                        _categories.Add(category);
                        _categoriesWithoutAll.Add(category);
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
                    .Include("AlternativeSalesPrices")
                    .Include("Category")
                    .Include("Suppliers")
                    .OrderBy(e => e.Category.Name)
                    .ThenBy(e => e.ItemID)
                    .ToList();

                foreach (var item in items)
                {
                    _items.Add(new ItemVM { Model = item, SelectedSupplier = item.Suppliers.FirstOrDefault() });
                }

                UpdateSuppliers();
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
                    if (supplier.Active == true && supplier.Name != "-")
                        _suppliers.Add(supplier);
                }
            }
        }

        private void UpdateEditAlternativeSalesPrices()
        {
            _editAlternativeSalesPrices.Clear();

            using (var context = new ERPContext())
            {
                var alternativeSalesPrices = context.AlternativeSalesPrices.Include("Item").Where(e => e.ItemID.Equals(_selectedLine.ID)).ToList();

                foreach (var alt in alternativeSalesPrices)
                    _editAlternativeSalesPrices.Add(alt);
            }
        }

        private void ResetEntryFields()
        {
            NewEntryID = null;
            NewEntryName = null;
            NewEntryCategory = null;
            NewEntrySupplier = null;
            NewEntryPiecesPerUnit = 1;
            NewEntryUnitName = null;
            NewEntrySalesPrice = null;
            NewEntryPurchasePrice = null;
        }
        #endregion
    }
}
