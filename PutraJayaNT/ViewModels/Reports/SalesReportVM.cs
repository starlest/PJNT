using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Collections.Generic;
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
        ObservableCollection<CustomerVM> _customers;
        ObservableCollection<SalesTransactionLineVM> _detailedDisplayLines;
        ObservableCollection<SalesTransactionLineVM> _globalDisplayLines;

        ObservableCollection<string> _modes;
        string _selectedMode;
        Visibility _detailedVisibility;
        Visibility _globalVisibility;

        DateTime _fromDate;
        DateTime _toDate;
        decimal _total;
        string _quantitySold;

        Category _selectedCategory;
        Item _selectedItem;
        CustomerVM _selectedCustomer;

        public SalesReportVM()
        {
            _categories = new ObservableCollection<Category>();
            _categoryItems = new ObservableCollection<Item>();
            _customers = new ObservableCollection<CustomerVM>();
            _detailedDisplayLines = new ObservableCollection<SalesTransactionLineVM>();
            _globalDisplayLines = new ObservableCollection<SalesTransactionLineVM>();

            _modes = new ObservableCollection<string>();
            _modes.Add("Global");
            _modes.Add("Detailed");
            SelectedMode = Modes.First();

            _fromDate = UtilityMethods.GetCurrentDate().Date.AddDays(-UtilityMethods.GetCurrentDate().Day + 1);
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateCategories();
            RefreshCustomers();

            SelectedCategory = _categories.FirstOrDefault();
            SelectedItem = _categoryItems.FirstOrDefault();
            SelectedCustomer = _customers.FirstOrDefault();
        }

        #region Collection
        public ObservableCollection<Category> Categories
        {
            get
            {
                return _categories;
            }
        }

        public ObservableCollection<Item> CategoryItems
        {
            get { return _categoryItems; }
        }

        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
        }

        public ObservableCollection<SalesTransactionLineVM> DetailedDisplayLines
        {
            get { return _detailedDisplayLines; }
        }

        public ObservableCollection<SalesTransactionLineVM> GlobalDisplayLines
        {
            get { return _globalDisplayLines; }
        }

        public ObservableCollection<string> Modes
        {
            get { return _modes; }
        }
        #endregion

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
                _selectedItem = null;
                UpdateCategoryItems();
            }
        }

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value, "SelectedItem"); }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                if (_selectedItem == null || _selectedCategory == null)
                {
                    MessageBox.Show("Please select an Item first.", "Invalid Action", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");
                RefreshDisplayLines();
            }
        }

        public string SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                SetProperty(ref _selectedMode, value, "SelectedMode");

                RefreshDisplayLines();

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

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, "Total"); }
        }

        public string QuantitySold
        {
            get { return _quantitySold; }
            set { SetProperty(ref _quantitySold, value, "QuantitySold"); }
        }

        #region Helper Methods
        private void UpdateCategories()
        {
            _categories.Clear();

            using (var context = new ERPContext())
            {
                _categories.Add(new Category { ID = -1, Name = "All" });
                var categories = context.ItemCategories;
                foreach (var category in categories)
                    _categories.Add(category);
            }
        }

        private void UpdateCategoryItems()
        {
            _categoryItems.Clear();
            _categoryItems.Add(new Item { ItemID = "-1", Name = "All" });

            if (_selectedCategory.Name == "All")
            {
                using (var context = new ERPContext())
                {
                    var items = context.Inventory.ToList();
                    foreach (var item in items)
                        _categoryItems.Add(item);
                }
            }

            else
            {
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

        private void RefreshCustomers()
        {
            _customers.Clear();

            _customers.Add(new CustomerVM {  Model = new Customer { ID = -1, Name = "All" } });
            using (var context = new ERPContext())
            {
                var customers = context.Customers.Include("Group").ToList();

                foreach (var customer in customers)
                    _customers.Add(new CustomerVM { Model = customer });
            }
        }

        private void RefreshDisplayLines()
        {
            _detailedDisplayLines.Clear();
            _globalDisplayLines.Clear();

            if (_selectedItem == null) return;

            var transactionLines = new List<SalesTransactionLine>();

            using (var context = new ERPContext())
            {
                if (_selectedCategory.Name == "All" && _selectedItem.Name == "All")
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("Item")
                        .Include("Warehouse")
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .Where(e => e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .ToList();
                }

                else if (_selectedCategory.Name == "All" && _selectedItem.Name != "All")
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("Item")
                        .Include("Warehouse")
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .Where(e => e.Item.Name == _selectedItem.Name && e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .ToList();
                }

                else if (_selectedCategory.Name != "All" && _selectedItem.Name == "All")
                {

                    transactionLines = context.SalesTransactionLines
                        .Where(e => e.Item.Category.Name == _selectedCategory.Name
                        && e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .Include("Item")
                        .Include("Warehouse")
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .ToList();
                }

                else
                {
                    transactionLines = context.SalesTransactionLines
                        .Where(e => e.Item.Category.Name == _selectedCategory.Name
                        && e.Item.Name == _selectedItem.Name
                        && e.SalesTransaction.When >= _fromDate
                        && e.SalesTransaction.When <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .Include("Item")
                        .Include("Warehouse")
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .ToList();
                }


                foreach (var line in transactionLines)
                {
                    if (_selectedCustomer.ID == -1 || _selectedCustomer.ID == line.SalesTransaction.Customer.ID)
                    {
                        _detailedDisplayLines.Add(new SalesTransactionLineVM { Model = line });

                        if (_selectedMode == "Global")
                        {
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

                RefreshTotal();
            }
        }

        private void RefreshTotal()
        {
            _total = 0;
            var quantitySold = 0;
            foreach (var line in _detailedDisplayLines)
            {
                quantitySold += line.Quantity;
                _total += line.NetTotal;
            }

            if (_selectedItem.Name != "All")
                QuantitySold = (quantitySold / _selectedItem.PiecesPerUnit) + "/" + (quantitySold % _selectedItem.PiecesPerUnit);

            else
                QuantitySold = "";
            OnPropertyChanged("Total");
        }
        #endregion
    }

}
