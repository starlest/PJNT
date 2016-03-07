﻿namespace PutraJayaNT.ViewModels.Customers
{
    using Models;
    using MVVMFramework;
    using Models.Sales;
    using Models.Salesman;
    using PutraJayaNT.Reports.Windows;
    using Utilities;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    class SalesCollectionListVM : ViewModelBase
    {
        ObservableCollection<string> _cities;
        ObservableCollection<Salesman> _salesmen;
        ObservableCollection<Customer> _customers;
        ObservableCollection<SalesTransactionMultiPurposeVM> _salesTransactions;

        string _selectedCity;
        Salesman _selectedSalesman;
        Supplier _selectedCustomer;
        DateTime _toDate;
        DateTime _fromDate;
        DateTime _collectionDate;
        decimal _total;
        bool _isPaidChecked;
        bool _allSelected;

        ICommand _refreshCollectionDateSelectionCommand;
        ICommand _printPerCityCommand;
        ICommand _printPerSalesmanCommand;

        public SalesCollectionListVM()
        {
            _cities = new ObservableCollection<string>();
            _salesmen = new ObservableCollection<Salesman>();
            _customers = new ObservableCollection<Customer>();
            _salesTransactions = new ObservableCollection<SalesTransactionMultiPurposeVM>();
            var currentDate = UtilityMethods.GetCurrentDate().Date;
            _fromDate = currentDate.AddDays(-currentDate.Day + 1);
            _toDate = currentDate;
            _collectionDate = currentDate;
            UpdateCities();
            UpdateSalesmen();
            UpdateCustomers();
        }

        public ObservableCollection<string> Cities
        {
            get { return _cities; }
        }

        public ObservableCollection<Salesman> Salesmen
        {
            get { return _salesmen; }
        }

        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
        }

        public ObservableCollection<SalesTransactionMultiPurposeVM> SalesTransactions
        {
            get { return _salesTransactions; }
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

                if (_toDate != null && _selectedCustomer != null && _isPaidChecked) UpdateCustomerSalesTransactions();
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

                if (_selectedSalesman != null && _selectedCity != null) UpdateSalesTransactions();
                if (_fromDate != null && _selectedCustomer != null) UpdateCustomerSalesTransactions();
            }
        }

        public DateTime CollectionDate
        {
            get { return _collectionDate; }
            set { SetProperty(ref _collectionDate, value, "CollectionDate"); }
        }

        public string SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                SetProperty(ref _selectedCity, value, "SelectedCity");

                if (_selectedCity == null) return;

                SelectedCustomer = null;
                if (_selectedSalesman != null && _selectedCity != null) UpdateSalesTransactions();
            }
        }

        public Salesman SelectedSalesman
        {
            get { return _selectedSalesman; }
            set
            {
                SetProperty(ref _selectedSalesman, value, "SelectedSalesman");

                if (_selectedSalesman == null) return;

                SelectedCustomer = null;
                if (_selectedSalesman != null && _selectedCity != null) UpdateSalesTransactions();
            }
        }

        public Supplier SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                if (_fromDate == null || _toDate == null)
                {
                    MessageBox.Show("Please select a date range.", "Invalid Command", MessageBoxButton.OK);
                    return;
                }

                SelectedCity = null;
                SelectedSalesman = null;

                UpdateCustomerSalesTransactions();
            }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, "Total"); }
        }

        public bool IsPaidChecked
        {
            get { return _isPaidChecked; }
            set
            {
                SetProperty(ref _isPaidChecked, value, "IsPaidChecked");

                if (_selectedCity != null && _selectedSalesman != null) UpdateSalesTransactions();
                if (_fromDate != null && _toDate != null && _selectedCustomer != null) UpdateCustomerSalesTransactions();
            }
        }

        public bool AllSelected
        {
            get { return _allSelected; }
            set
            {
                SetProperty(ref _allSelected, value, "AllSelected");

                if (_allSelected)
                {
                    foreach (var line in _salesTransactions)
                        line.IsSelected = true;
                }
                
                else
                {
                    foreach (var line in _salesTransactions)
                        line.IsSelected = false;
                }
            }
        }

        public ICommand RefreshCollectionDateSelectionCommand
        {
            get
            {
                return _refreshCollectionDateSelectionCommand ?? (_refreshCollectionDateSelectionCommand = new RelayCommand(() =>
                {
                    SelectedCustomer = null;
                    SelectedCity = null;
                    SelectedSalesman = null;
                    UpdateCollectionDateSalesTransactions();
                }));
            }
        }

        public ICommand PrintPerCityCommand
        {
            get
            {
                return _printPerCityCommand ?? (_printPerCityCommand = new RelayCommand(() =>
                {
                    if (_salesTransactions.Count == 0) return;

                    var collectionReportWindow = new CollectionReportPerCityWindow(_salesTransactions, _toDate);
                    collectionReportWindow.Owner = App.Current.MainWindow;
                    collectionReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    collectionReportWindow.Show();
                }));
            }
        }

        public ICommand PrintPerSalesmanCommand
        {
            get
            {
                return _printPerSalesmanCommand ?? (_printPerSalesmanCommand = new RelayCommand(() =>
                {
                    if (_salesTransactions.Count == 0) return;

                    var collectionReportWindow = new CollectionReportPerSalesmanWindow(_salesTransactions, _collectionDate);
                    collectionReportWindow.Owner = App.Current.MainWindow;
                    collectionReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    collectionReportWindow.Show();
                }));
            }
        }

        #region Helper Methods 
        private void UpdateCities()
        {
            _cities.Clear();
            _cities.Add("All");
            using (var context = new ERPContext())
            {
                var customers = context.Customers.ToList();

                foreach (var customer in customers)
                {
                    if (!_cities.Contains(customer.City))
                        _cities.Add(customer.City);  
                }
            }
        }

        private void UpdateSalesmen()
        {
            _salesmen.Clear();
            using (var context = new ERPContext())
            {
                var salesmen = context.Salesmans.ToList();

                _salesmen.Add(new Salesman { ID = -1, Name = "All" });
                foreach (var salesman in salesmen)
                    _salesmen.Add(salesman);
            }
        }

        private void UpdateCustomers()
        {
            _customers.Clear();

            using (var context = new ERPContext())
            {
                var customers = context.Customers.OrderBy(e => e.Name).ToList();
                _customers.Add(new Customer { ID = -1, Name = "All" });
                foreach (var customer in customers)
                    _customers.Add(customer);
            }
        }

        private void UpdateSalesTransactions()
        {
            _salesTransactions.Clear();
            _total = 0;
            List<SalesTransaction> salesTransactions;

            using (var context = new ERPContext())
            {
                if (!_isPaidChecked)
                {
                    if (_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.Paid < e.Total && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ThenBy(e => e.Customer.Name)
                            .ToList();
                    }

                    else if (_selectedCity.Equals("All") && !_selectedSalesman.Name.Equals("All"))
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.CollectionSalesman.ID.Equals(_selectedSalesman.ID)
                            && e.Paid < e.Total && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ThenBy(e => e.Customer.Name)
                            .ToList();
                    }

                    else if (!_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.Customer.City.Equals(_selectedCity) && e.Paid < e.Total && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ThenBy(e => e.Customer.Name)
                            .ToList();
                    }

                    else
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.Customer.City.Equals(_selectedCity) && e.CollectionSalesman.ID.Equals(_selectedSalesman.ID)
                            && e.Paid < e.Total && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ToList();
                    }
                }

                else
                {
                    if (_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.Paid >= e.Total && e.DueDate >= _fromDate && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ThenBy(e => e.Customer.Name)
                            .ToList();
                    }

                    else if (_selectedCity.Equals("All") && !_selectedSalesman.Name.Equals("All"))
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.CollectionSalesman.ID.Equals(_selectedSalesman.ID) && e.Paid >= e.Total && e.DueDate >= _fromDate && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ThenBy(e => e.Customer.Name)
                            .ToList();
                    }

                    else if (!_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.Customer.City.Equals(_selectedCity) && e.Paid >= e.Total && e.DueDate >= _fromDate && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ThenBy(e => e.Customer.Name)
                            .ToList();
                    }

                    else
                    {
                        salesTransactions = context.SalesTransactions
                            .Include("Customer")
                            .Include("Customer.Group")
                            .Include("CollectionSalesman")
                            .Where(e => e.Customer.City.Equals(_selectedCity) && e.CollectionSalesman.ID.Equals(_selectedSalesman.ID) && e.Paid >= e.Total && e.DueDate >= _fromDate && e.DueDate <= _toDate)
                            .OrderBy(e => e.CollectionSalesman.Name)
                            .ThenBy(e => e.DueDate)
                            .ToList();
                    }
                }

                foreach (var t in salesTransactions)
                {
                    var vm = new SalesTransactionMultiPurposeVM { Model = t };
                    _salesTransactions.Add(vm);
                    _total += vm.Remaining;
                }

                OnPropertyChanged("Total");
            }
        }

        private void UpdateCustomerSalesTransactions()
        {
            _salesTransactions.Clear();
            _total = 0;
            using (var context = new ERPContext())
            {
                List<SalesTransaction> transactions;

                if (_selectedCustomer.Name.Equals("All") && !_isPaidChecked)
                {
                    transactions = context.SalesTransactions                
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Paid < e.Total && e.DueDate <= _toDate)
                        .OrderBy(e => e.Customer.Name)
                        .ThenBy(e => e.DueDate)
                        .ToList();
                }

                else if (_selectedCustomer.Name.Equals("All") && _isPaidChecked)
                {
                    transactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Paid >= e.Total && e.DueDate <= _toDate && e.DueDate >= _fromDate)
                        .OrderBy(e => e.Customer.Name)
                        .ThenBy(e => e.DueDate)
                        .ToList();
                }

                else if (!_selectedCustomer.Name.Equals("All") && !_isPaidChecked)
                {
                    transactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Customer.Name.Equals(_selectedCustomer.Name) &&
                        e.Paid < e.Total && e.DueDate <= _toDate)
                        .OrderBy(e => e.Customer.Name)
                        .ThenBy(e => e.DueDate)
                        .ToList();
                }

                else
                {
                    transactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Customer.Name.Equals(_selectedCustomer.Name) &&
                        e.Paid >= e.Total && (e.DueDate >= _fromDate && e.DueDate >= _fromDate && e.DueDate <= _toDate))
                        .OrderBy(e => e.Customer.Name)
                        .ThenBy(e => e.DueDate)
                        .ToList();
                }

                foreach (var transaction in transactions)
                {
                    _salesTransactions.Add(new SalesTransactionMultiPurposeVM { Model = transaction });
                    _total += transaction.Total - transaction.Paid;
                }

                OnPropertyChanged("Total");
            }
        }

        private void UpdateCollectionDateSalesTransactions()
        {
            _salesTransactions.Clear();
            using (var context = new ERPContext())
            {
                var ledgerTransactions = context.Ledger_Transactions.Where(e => e.Description.Equals("Sales Transaction Receipt") && e.Date.Equals(_collectionDate)).ToList();
                var temp = new List<SalesTransactionMultiPurposeVM>();
                var emptyCollectionSalesman = context.Salesmans.Where(e => e.Name.Equals(" ")).FirstOrDefault();
                foreach (var lt in ledgerTransactions)
                {
                    var transaction = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.SalesTransactionID.Equals(lt.Documentation)).FirstOrDefault();

                    var vm = new SalesTransactionMultiPurposeVM { Model = transaction };

                    if (!temp.Contains(vm))
                    {
                        if (vm.CollectionSalesman == null) vm.CollectionSalesman = emptyCollectionSalesman;
                        temp.Add(vm);
                        _total += vm.Remaining;
                    }
                }


                var sort = temp.OrderBy(e => e.CollectionSalesman.Name).ToList();

                foreach (var line in sort)
                {
                    _salesTransactions.Add(line);
                }
            }
        }
        #endregion
    }
}
