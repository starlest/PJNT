using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels.Reports
{
    class SalesReportVM : ViewModelBase
    {
        ObservableCollection<Category> _categories;
        ObservableCollection<Item> _categoryItems;
        ObservableCollection<SalesTransactionLineVM> _detailedDisplayLines;
        ObservableCollection<SalesTransactionLineVM> _globalDisplayLines;

        ObservableCollection<string> _modes;
        string _selectedMode;
        Visibility _detailedVisibility;
        Visibility _globalVisibility;

        /// <summary>
        /// Backing field for the <see cref="FromDate"/> property
        /// </summary>
        DateTime _fromDate;
        DateTime _toDate;

        Category _selectedCategory;
        Item _selectedItem;

        public SalesReportVM()
        {
            _categories = new ObservableCollection<Category>();
            _categoryItems = new ObservableCollection<Item>();
            _detailedDisplayLines = new ObservableCollection<SalesTransactionLineVM>();
            _globalDisplayLines = new ObservableCollection<SalesTransactionLineVM>();

            _modes = new ObservableCollection<string>();
            _modes.Add("Global");
            _modes.Add("Detailed");
            SelectedMode = Modes.First();

            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;
        }

        /// <summary>
        /// Gets the list of Categories loaded.
        /// </summary>
        public ObservableCollection<Category> Categories
        {
            get
            {
                RefreshCategories();
                return _categories;
            }
        }

        /// <summary>
        /// Gets the list of selected Category Items loaded.
        /// </summary>
        public ObservableCollection<Item> CategoryItems
        {
            get { return _categoryItems; }
        }

        /// <summary>
        /// Gets the list of detailed lines loaded.
        /// </summary>
        public ObservableCollection<SalesTransactionLineVM> DetailedDisplayLines
        {
            get { return _detailedDisplayLines; }
        }

        /// <summary>
        /// Gets the list of lines loaded.
        /// </summary>
        public ObservableCollection<SalesTransactionLineVM> GlobalDisplayLines
        {
            get { return _globalDisplayLines; }
        }

        /// <summary>
        /// Gets the list of view modes.
        /// </summary>
        /// <remark>
        /// Available modes are Global and Detailed.
        /// </remark>
        public ObservableCollection<string> Modes
        {
            get { return _modes; }
        }

        /// <summary>
        /// Gets or sets the starting date of the transactions to be loaded.
        /// </summary>
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_toDate < value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _fromDate, value, "FromDate");
                RefreshDisplayLines();
            }
        }

        /// <summary>
        /// Gets or sets the ending date of the transactions to be loaded.
        /// </summary>
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (_fromDate > value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _toDate, value, "ToDate");
                RefreshDisplayLines();
            }
        }

        /// <summary>
        /// The selected category from <see cref="Categories"/>.
        /// </summary>
        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _detailedDisplayLines.Clear();
                SetProperty(ref _selectedCategory, value, "SelectedCategory");

                if (_selectedCategory == null) return;

                _categoryItems.Clear();
                _selectedItem = null;
                RefreshCategories();

                if (_selectedCategory.Name == "All")
                {
                    _categoryItems.Add(new Item { Name = "All" });

                    using (var context = new ERPContext())
                    {
                        var items = context.Inventory.ToList();
                        foreach (var item in items)
                            _categoryItems.Add(item);
                    }
                }

                else
                {
                    _categoryItems.Add(new Item { Name = "All" });

                    using (var context = new ERPContext())
                    {
                        var items = context.Inventory
                            .Where(e => e.Category.Name == _selectedCategory.Name)
                            .ToList();
                        foreach (var item in items)
                            _categoryItems.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// The selected item from <see cref="CategoryItems"/>.
        /// </summary>
        public Item SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, "SelectedItem");
                RefreshDisplayLines();
            }
        }

        /// <summary>
        /// The selected mode from <see cref="Modes"/>.
        /// </summary>
        public string SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                SetProperty(ref _selectedMode, value, "SelectedMode");

                if (_selectedMode == "Global")
                {
                    GlobalVisibility = Visibility.Visible;
                    DetailedVisibility = Visibility.Collapsed;
                }
                else
                {
                    GlobalVisibility = Visibility.Collapsed;
                    DetailedVisibility = Visibility.Visible;
                }
            }
        }

        public Visibility DetailedVisibility
        {
            get { return _detailedVisibility; }
            set { SetProperty(ref _detailedVisibility, value, "DetailedVisibility"); }
        }

        public Visibility GlobalVisibility
        {
            get { return _globalVisibility; }
            set { SetProperty(ref _globalVisibility, value, "GlobalVisibility"); }
        }

        private void RefreshCategories()
        {
            _categories.Clear();

            using (var context = new ERPContext())
            {
                _categories.Add(new Category { Name = "All" });
                var categories = context.Categories.Include("Items");
                foreach (var category in categories)
                    _categories.Add(category);
            }
        }

        private void RefreshDisplayLines()
        {
            _detailedDisplayLines.Clear();
            _globalDisplayLines.Clear();

            if (_selectedItem == null) return;

            if (_selectedCategory.Name == "All" && _selectedItem.Name == "All")
            {
                using (var context = new ERPContext())
                {
                    var transactionLines = context.SalesTransactionLines
                        .Include("Item")
                        .Include("SalesTransaction")
                        .Where(e => e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name);


                    foreach (var line in transactionLines)
                    {
                        _detailedDisplayLines.Add(new SalesTransactionLineVM { Model = line });

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.Item.ItemID == line.Item.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLineVM { Model = line });
                    }  
                }
            }

            else if (_selectedCategory.Name == "All" && _selectedItem.Name != "All")
            {
                using (var context = new ERPContext())
                {
                    var transactionLines = context.SalesTransactionLines
                        .Include("Item")
                        .Include("SalesTransaction")
                        .Where(e => e.Item.Name == _selectedItem.Name && e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name);

                    foreach (var line in transactionLines)
                    {
                        _detailedDisplayLines.Add(new SalesTransactionLineVM { Model = line });

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.Item.ItemID == line.Item.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLineVM { Model = line });
                    }
                }
            }

            else if (_selectedCategory.Name != "All" && _selectedItem.Name == "All")
            {
                using (var context = new ERPContext())
                {
                    var transactionLines = context.SalesTransactionLines
                        .Where(e => e.Item.Category.Name == _selectedCategory.Name
                        && e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .Include("Item")
                        .Include("SalesTransaction");

                    foreach (var line in transactionLines)
                    {
                        _detailedDisplayLines.Add(new SalesTransactionLineVM { Model = line });

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.Item.ItemID == line.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLineVM { Model = line });
                    }
                }
            }

            else
            {
                using (var context = new ERPContext())
                {
                    var transactionLines = context.SalesTransactionLines
                        .Where(e => e.Item.Category.Name == _selectedCategory.Name
                        && e.Item.Name == _selectedItem.Name
                        && e.SalesTransaction.When >= _fromDate
                        && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .Include("Item")
                        .Include("SalesTransaction");

                    foreach (var line in transactionLines)
                    {
                        _detailedDisplayLines.Add(new SalesTransactionLineVM { Model = line });

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.Item.ItemID == line.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLineVM { Model = line });
                    }
                }
            }
        }
    }
}
