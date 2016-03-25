﻿namespace PutraJayaNT.ViewModels.Reports
{
    using MVVMFramework;
    using Models.Inventory;
    using Models.Sales;
    using Utilities;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Customer;
    using Customer;
    using Sales;

    internal class SalesReportVM : ViewModelBase
    {
        private string _selectedMode;
        private Visibility _detailedVisibility;
        private Visibility _globalVisibility;

        private DateTime _fromDate;
        private DateTime _toDate;
        private decimal _total;
        private string _quantitySold;

        private Category _selectedCategory;
        private Item _selectedItem;
        private CustomerVM _selectedCustomer;

        private ICommand _displayCommand;

        public SalesReportVM()
        {
            Categories = new ObservableCollection<Category>();
            CategoryItems = new ObservableCollection<Item>();
            Customers = new ObservableCollection<CustomerVM>();
            DetailedDisplayLines = new ObservableCollection<SalesTransactionLineVM>();
            GlobalDisplayLines = new ObservableCollection<SalesTransactionLineVM>();

            Modes = new ObservableCollection<string> {"Global", "Detailed"};
            SelectedMode = Modes.First();

            _fromDate = UtilityMethods.GetCurrentDate().Date.AddDays(-UtilityMethods.GetCurrentDate().Day + 1);
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateCategories();
            RefreshCustomers();

            SelectedCategory = Categories.FirstOrDefault();
            SelectedItem = CategoryItems.FirstOrDefault();
            SelectedCustomer = Customers.FirstOrDefault();
        }

        #region Collection
        public ObservableCollection<Category> Categories { get; }

        public ObservableCollection<Item> CategoryItems { get; }

        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<SalesTransactionLineVM> DetailedDisplayLines { get; }

        public ObservableCollection<SalesTransactionLineVM> GlobalDisplayLines { get; }

        public ObservableCollection<string> Modes { get; }
        #endregion

        #region Properties
        public Visibility DetailedVisibility
        {
            get { return _detailedVisibility; }
            set { SetProperty(ref _detailedVisibility, value, () => DetailedVisibility); }
        }

        public Visibility GlobalVisibility
        {
            get { return _globalVisibility; }
            set { SetProperty(ref _globalVisibility, value, () => GlobalVisibility); }
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
                SetProperty(ref _fromDate, value, () => FromDate);
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
                SetProperty(ref _toDate, value, () => ToDate);
            }
        }

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                SetProperty(ref _selectedCategory, value, () => SelectedCategory);
                if (_selectedCategory == null) return;
                UpdateCategoryItems();
            }
        }

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value, () => SelectedItem); }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value, () => SelectedCustomer); }
        }

        public string SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                SetProperty(ref _selectedMode, value, () => SelectedMode);
                SetDisplayMode();
                
            }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, () => Total); }
        }

        public string QuantitySold
        {
            get { return _quantitySold; }
            set { SetProperty(ref _quantitySold, value, () => QuantitySold); }
        }
        #endregion

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (_selectedItem != null && _selectedCustomer!= null) RefreshDisplayLines();
                    UpdateCategories();
                    RefreshCustomers();
                }));
            }
        }

        #region Helper Methods
        private void UpdateCategories()
        {
            var oldSelectedCategory = _selectedCategory;

            Categories.Clear();
            using (var context = new ERPContext())
            {
                Categories.Add(new Category { ID = -1, Name = "All" });
                var categories = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categories)
                    Categories.Add(category);
            }

            UpdateSelectedCategory(oldSelectedCategory);
        }

        private void UpdateSelectedCategory(Category oldSelectedCategory)
        {
            if (oldSelectedCategory == null) return;
            SelectedCategory = Categories.SingleOrDefault(category => category.ID.Equals(oldSelectedCategory.ID));
        }

        private void UpdateCategoryItems()
        {
            var oldSelectedItem = _selectedItem;

            CategoryItems.Clear();
            CategoryItems.Add(new Item { ItemID = "-1", Name = "All" });
            if (_selectedCategory.Name == "All")
            {
                using (var context = new ERPContext())
                {
                    var items = context.Inventory.OrderBy(item => item.Name);
                    foreach (var item in items)
                        CategoryItems.Add(item);
                }
            }

            else
            {
                using (var context = new ERPContext())
                {
                    var items = context.Inventory
                        .Where(e => e.Category.Name == _selectedCategory.Name)
                        .OrderBy(item => item.Name);
                    foreach (var item in items)
                        CategoryItems.Add(item);
                }
            }

            UpdateSelectedItem(oldSelectedItem);
        }

        private void UpdateSelectedItem(Item oldSelectedItem)
        {
            if (oldSelectedItem == null) return;
            SelectedItem = CategoryItems.SingleOrDefault(item => item.ItemID.Equals(oldSelectedItem.ItemID));
        }

        private void RefreshCustomers()
        {
            var oldSelectedCustomer = _selectedCustomer;

            Customers.Clear();
            Customers.Add(new CustomerVM {  Model = new Customer { ID = -1, Name = "All" } });
            using (var context = new ERPContext())
            {
                var customers = context.Customers.Include("Group").OrderBy(customer => customer.Name);
                foreach (var customer in customers)
                    Customers.Add(new CustomerVM { Model = customer });
            }

            UpdateSelectedCustomer(oldSelectedCustomer);
        }

        private void UpdateSelectedCustomer(CustomerVM oldSelectedCustomer)
        {
            if (oldSelectedCustomer == null) return;
            SelectedCustomer = Customers.SingleOrDefault(customer => customer.ID.Equals(oldSelectedCustomer.ID));
        }

        private void RefreshDisplayLines()
        {
            DetailedDisplayLines.Clear();
            GlobalDisplayLines.Clear();
            if (_selectedItem == null) return;
            using (var context = new ERPContext())
            {
                List<SalesTransactionLine> transactionLines;
                if (_selectedCategory.Name == "All" && _selectedItem.Name == "All")
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("Item")
                        .Include("Warehouse")
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .Where(e => e.SalesTransaction.Date >= _fromDate && e.SalesTransaction.Date <= _toDate)
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
                        .Where(e => e.Item.Name == _selectedItem.Name && e.SalesTransaction.Date >= _fromDate && e.SalesTransaction.Date <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .ToList();
                }

                else if (_selectedCategory.Name != "All" && _selectedItem.Name == "All")
                {

                    transactionLines = context.SalesTransactionLines
                        .Where(e => e.Item.Category.Name == _selectedCategory.Name
                        && e.SalesTransaction.Date >= _fromDate && e.SalesTransaction.Date <= _toDate)
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
                        && e.SalesTransaction.Date >= _fromDate
                        && e.SalesTransaction.Date <= _toDate)
                        .OrderBy(e => e.Item.Name)
                        .Include("Item")
                        .Include("Warehouse")
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .ToList();
                }


                foreach (var line in transactionLines.Where(line => _selectedCustomer.ID == -1 || _selectedCustomer.ID == line.SalesTransaction.Customer.ID))
                {
                    DetailedDisplayLines.Add(new SalesTransactionLineVM { Model = line });
                    //if (_selectedMode != "Global") continue;
                    var contains = false;
                    foreach (var l in GlobalDisplayLines.Where(l => l.Item.ItemID.Equals(line.ItemID)))
                    {
                        l.Quantity += line.Quantity;
                        contains = true;
                        break;
                    }
                    if (!contains) GlobalDisplayLines.Add(new SalesTransactionLineVM { Model = line });
                }
                RefreshTotal();
            }
        }

        private void RefreshTotal()
        {
            _total = 0;
            var quantitySold = 0;
            foreach (var line in DetailedDisplayLines)
            {
                quantitySold += line.Quantity;
                _total += line.NetTotal;
            }

            if (_selectedItem.Name != "All")
                QuantitySold = quantitySold / _selectedItem.PiecesPerUnit + "/" + quantitySold % _selectedItem.PiecesPerUnit;
            else
                QuantitySold = "";
            OnPropertyChanged("Total");
        }

        private void SetDisplayMode()
        {
            if (_selectedMode.Equals("Global"))
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
        #endregion
    }

}
