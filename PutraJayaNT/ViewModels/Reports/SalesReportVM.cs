﻿using MVVMFramework;
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

        /// <summary>
        /// Backing field for the <see cref="FromDate"/> property
        /// </summary>
        DateTime _fromDate;
        DateTime _toDate;
        decimal _total;

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

            _fromDate = DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1);
            _toDate = DateTime.Now.Date;

            UpdateCategories();
            RefreshCustomers();

            SelectedCategory = _categories.FirstOrDefault();
            SelectedItem = _categoryItems.FirstOrDefault();
            SelectedCustomer = _customers.FirstOrDefault();
        }

        #region Collection
        /// <summary>
        /// Gets the list of Categories loaded.
        /// </summary>
        public ObservableCollection<Category> Categories
        {
            get
            {
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
        /// Gets the list of Customers loaded.
        /// </summary>
        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
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
                _selectedItem = null;
                UpdateCategoryItems();
            }
        }

        /// <summary>
        /// The selected item from <see cref="CategoryItems"/>.
        /// </summary>
        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value, "SelectedItem"); }
        }

        /// <summary>
        /// The selected customer from <see cref="Customers"/>.
        /// </summary>
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
                RefreshTotal();
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

        #region Helper Methods
        private void UpdateCategories()
        {
            _categories.Clear();

            using (var context = new ERPContext())
            {
                _categories.Add(new Category { ID = -1, Name = "All" });
                var categories = context.Categories;
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
            }
        }

        private void RefreshTotal()
        {
            _total = 0;
            foreach (var line in _detailedDisplayLines)
                Total += line.NetTotal;
        }
        #endregion
    }

}
