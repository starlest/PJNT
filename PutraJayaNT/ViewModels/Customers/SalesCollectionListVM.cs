namespace PutraJayaNT.ViewModels.Customers
{
    using Models;
    using MVVMFramework;
    using PutraJayaNT.Models.Sales;
    using PutraJayaNT.Models.Salesman;
    using PutraJayaNT.Reports.Windows;
    using PutraJayaNT.Utilities;
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
        ObservableCollection<SalesCollectionListLineVM> _salesTransactions;

        string _selectedCity;
        Salesman _selectedSalesman;
        Customer _selectedCustomer;
        DateTime _date;
        decimal _total;
        ICommand _printCommand;

        public SalesCollectionListVM()
        {
            _cities = new ObservableCollection<string>();
            _salesmen = new ObservableCollection<Salesman>();
            _customers = new ObservableCollection<Customer>();
            _salesTransactions = new ObservableCollection<SalesCollectionListLineVM>();
            _date = DateTime.Now.Date;
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

        public ObservableCollection<SalesCollectionListLineVM> SalesTransactions
        {
            get { return _salesTransactions; }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {

                SetProperty(ref _date, value, "Date");
                if (_selectedSalesman != null && _selectedCity != null) UpdateSalesTransactions();
            }
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

        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                SelectedCity = null;
                SelectedSalesman = null;

                UpdateCustomerSalesTransactionLines();
            }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, "Total"); }
        }

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (_salesTransactions.Count == 0) return;

                    var collectionReportWindow = new CollectionReportWindow(_salesTransactions);
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
                var customers = context.Customers.ToList();
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
                if (_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                {
                    salesTransactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Paid < e.Total && e.DueDate <= _date)
                        .OrderBy(e => e.DueDate)
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
                        && e.Paid < e.Total && e.DueDate <= _date)
                        .OrderBy(e => e.DueDate)
                        .ThenBy(e => e.Customer.Name)
                        .ToList();
                }

                else if (!_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                {
                    salesTransactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Customer.City.Equals(_selectedCity) && e.Paid < e.Total && e.DueDate <= _date)
                        .OrderBy(e => e.DueDate)
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
                        && e.Paid < e.Total && e.DueDate <= _date)
                        .OrderBy(e => e.DueDate)
                        .ThenBy(e => e.Customer.Name)
                        .ToList();
                }

                foreach (var t in salesTransactions)
                {
                    var vm = new SalesCollectionListLineVM { Model = t };
                    _salesTransactions.Add(vm);
                    _total += vm.Remaining;
                }

                OnPropertyChanged("Total");
            }
        }

        private void UpdateCustomerSalesTransactionLines()
        {
            _salesTransactions.Clear();
            _total = 0;
            using (var context = new ERPContext())
            {
                List<SalesTransaction> transactions;

                if (_selectedCustomer.Name.Equals("All"))
                {
                    transactions = context.SalesTransactions                
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman")
                        .Where(e => e.Paid < e.Total)
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
                        e.Paid < e.Total)
                        .OrderBy(e => e.Customer.Name)
                        .ThenBy(e => e.DueDate)
                        .ToList();
                }

                foreach (var transaction in transactions)
                {
                    _salesTransactions.Add(new SalesCollectionListLineVM { Model = transaction });
                    _total += transaction.Total - transaction.Paid;
                }

                OnPropertyChanged("Total");
            }
        }
        #endregion
    }
}
