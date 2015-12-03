using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    class SalesReportVM : ViewModelBase
    {
        ObservableCollection<Category> _categories;
        ObservableCollection<Item> _categoryItems;
        ObservableCollection<SalesTransactionLine> _detailedDisplayLines;
        ObservableCollection<SalesTransactionLine> _globalDisplayLines;

        ObservableCollection<string> _modes;
        string _selectedMode;
        Visibility _detailedVisibility;
        Visibility _globalVisibility;

        DateTime _fromDate;
        DateTime _toDate;

        Category _selectedCategory;
        Item _selectedItem;

        public SalesReportVM()
        {
            _categories = new ObservableCollection<Category>();
            _categoryItems = new ObservableCollection<Item>();
            _detailedDisplayLines = new ObservableCollection<SalesTransactionLine>();
            _globalDisplayLines = new ObservableCollection<SalesTransactionLine>();

            _modes = new ObservableCollection<string>();
            _modes.Add("Global");
            _modes.Add("Detailed");
            SelectedMode = Modes.First();

            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date.AddDays(1);
        }

        public ObservableCollection<Category> Categories
        {
            get
            {
                RefreshCategories();
                return _categories;
            }
        }

        public ObservableCollection<Item> CategoryItems
        {
            get { return _categoryItems; }
        }

        public ObservableCollection<SalesTransactionLine> DetailedDisplayLines
        {
            get { return _detailedDisplayLines; }
        }

        public ObservableCollection<SalesTransactionLine> GlobalDisplayLines
        {
            get { return _globalDisplayLines; }
        }

        public ObservableCollection<string> Modes
        {
            get { return _modes; }
        }

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
                        var items = context.Inventory;
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
                            .Where(e => e.Category.Name == _selectedCategory.Name);
                        foreach (var item in items)
                            _categoryItems.Add(item);
                    }
                }
            }
        }

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, "SelectedItem");
                RefreshDisplayLines();
            }
        }

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
                var categories = context.Category.Include("Items");
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
                        _detailedDisplayLines.Add(line);

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.ItemID == line.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLine(line));
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
                        _detailedDisplayLines.Add(line);

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.ItemID == line.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLine(line));
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
                        _detailedDisplayLines.Add(line);

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.ItemID == line.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLine(line));
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
                        _detailedDisplayLines.Add(line);

                        var contains = false;
                        foreach (var l in _globalDisplayLines)
                        {
                            if (l.ItemID == line.ItemID)
                            {
                                l.Quantity += line.Quantity;
                                contains = true;
                                break;
                            }
                        }

                        if (!contains) _globalDisplayLines.Add(new SalesTransactionLine(line));
                    }
                }
            }
        }
    }
}
