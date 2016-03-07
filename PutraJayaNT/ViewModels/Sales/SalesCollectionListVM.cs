using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PutraJayaNT.Utilities.Database.Salesman;
using PutraJayaNT.ViewModels.Salesman;

namespace PutraJayaNT.ViewModels.Sales
{
    using MVVMFramework;
    using Models;
    using Models.Sales;
    using PutraJayaNT.Reports.Windows;
    using Utilities;
    using Utilities.Database.Customer;
    using Customers;
    using Models.Salesman;

    internal class SalesCollectionListVM : ViewModelBase
    {
        #region Backing Fields
        private string _selectedCity;
        private SalesmanVM _selectedSalesman;
        private CustomerVM _selectedCustomer;
        private DateTime _toDate;
        private DateTime _fromDate;
        private DateTime _collectionDate;
        private decimal _total;
        private bool _isPaidChecked;
        private bool _allSelected;

        private ICommand _displayCommand;
        private ICommand _displayCollectionDateSelectionCommand;
        private ICommand _printPerCityCommand;
        private ICommand _printPerSalesmanCommand;
        #endregion

        public SalesCollectionListVM()
        {
            Cities = new ObservableCollection<string>();
            Salesmans = new ObservableCollection<SalesmanVM>();
            Customers = new ObservableCollection<CustomerVM>();
            DisplayedSalesTransactions = new ObservableCollection<SalesTransactionMultiPurposeVM>();

            var currentDate = UtilityMethods.GetCurrentDate().Date;
            _fromDate = currentDate.AddDays(-currentDate.Day + 1);
            _toDate = currentDate;
            _collectionDate = currentDate;

            UpdateCities();
            UpdateSalesmen();
            UpdateCustomers();
        }

        #region Collections
        public ObservableCollection<string> Cities { get; }

        public ObservableCollection<SalesmanVM> Salesmans { get; }

        public ObservableCollection<CustomerVM> Customers { get; }

        public ObservableCollection<SalesTransactionMultiPurposeVM> DisplayedSalesTransactions { get; }
        #endregion

        #region Properties
        public bool IsPaidChecked
        {
            get { return _isPaidChecked; }
            set { SetProperty(ref _isPaidChecked, value, "IsPaidChecked"); }
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
            }
        }

        public SalesmanVM SelectedSalesman
        {
            get { return _selectedSalesman; }
            set
            {
                SetProperty(ref _selectedSalesman, value, "SelectedSalesman");

                if (_selectedSalesman == null) return;

                SelectedCustomer = null;
            }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                SelectedCity = null;
                SelectedSalesman = null;
            }
        }

        public bool AllSelected
        {
            get { return _allSelected; }
            set
            {
                SetProperty(ref _allSelected, value, "AllSelected");
                ChangeAllSelections();
            }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, "Total"); }
        }
        #endregion

        #region Commands

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    UpdateDisplayedSalesTransactions();
                    UpdateCities();
                    UpdateSalesmen();
                    UpdateCustomers();
                }));
            }
        }

        public ICommand DisplayCollectionDateSelectionCommand
        {
            get
            {
                return _displayCollectionDateSelectionCommand ?? (_displayCollectionDateSelectionCommand = new RelayCommand(() =>
                {
                    SelectedCustomer = null;
                    SelectedCity = null;
                    SelectedSalesman = null;
                    UpdateDisplayedSalesTransactions();
                    UpdateCities();
                    UpdateSalesmen();
                    UpdateCustomers();
                }));
            }
        }

        public ICommand PrintPerCityCommand
        {
            get
            {
                return _printPerCityCommand ?? (_printPerCityCommand = new RelayCommand(() =>
                {
                    if (DisplayedSalesTransactions.Count == 0) return;

                    var collectionReportWindow = new CollectionReportPerCityWindow(DisplayedSalesTransactions, _toDate)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
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
                    if (DisplayedSalesTransactions.Count == 0) return;

                    var collectionReportWindow = new CollectionReportPerSalesmanWindow(DisplayedSalesTransactions,
                        _collectionDate)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    collectionReportWindow.Show();
                }));
            }
        }
        #endregion 

        #region Helper Methods 
        private void UpdateCities()
        {
            var oldSelectedCity = _selectedCity;

            Cities.Clear();
            var customersReturnedFromDatabase = DatabaseCustomerHelper.GetAll();
            foreach (var customer in customersReturnedFromDatabase.Where(customer => !Cities.Contains(customer.City)))
                Cities.Add(customer.City);
            ArrangeCitiesAlphabetically();

            UpdateSelectedCity(oldSelectedCity);
        }

        private void ArrangeCitiesAlphabetically()
        {
            var arragedCities = Cities.OrderBy(city => city).ToList();
            Cities.Clear();
            Cities.Add("All");
            foreach (var city in arragedCities)
                Cities.Add(city);
        }

        private void UpdateSelectedCity(string oldSelectedCity)
        {
            if (oldSelectedCity == null) return;
            SelectedCity = Cities.FirstOrDefault(city => city.Equals(oldSelectedCity));
        }

        private void UpdateSalesmen()
        {
            var oldSelectedSalesman = _selectedSalesman;
            Salesmans.Clear();

            var salesmansFromDatabase = DatabaseSalesmanHelper.GetAll();
            var allSalesman = new Salesman { ID = -1, Name = "All" };
            Salesmans.Add(new SalesmanVM { Model = allSalesman });
            foreach (var salesman in salesmansFromDatabase)
                Salesmans.Add(new SalesmanVM { Model = salesman });

            UpdateSelectedSalesman(oldSelectedSalesman);
        }

        private void UpdateSelectedSalesman(SalesmanVM oldSelectedSalesman)
        {
            if (oldSelectedSalesman == null) return;
            SelectedSalesman = Salesmans.FirstOrDefault(salesman => salesman.ID.Equals(oldSelectedSalesman.ID));
        }

        private void UpdateCustomers()
        {
            var oldSelectedCustomer = _selectedCustomer;
            Customers.Clear();

            var customers = DatabaseCustomerHelper.GetAll();
            var allCustomer = new Customer { ID = -1, Name = "All" };
            Customers.Add(new CustomerVM { Model = allCustomer });
            foreach (var customer in customers)
                Customers.Add(new CustomerVM { Model = customer });

            UpdateSelectedCustomer(oldSelectedCustomer);
        }

        private void UpdateSelectedCustomer(CustomerVM oldSelectedCustomer)
        {
            if (oldSelectedCustomer == null) return;
            SelectedCustomer = Customers.FirstOrDefault(customer => customer.ID.Equals(oldSelectedCustomer.ID));
        }

        private void ChangeAllSelections()
        {
            if (_allSelected)
            {
                foreach (var line in DisplayedSalesTransactions)
                    line.IsSelected = true;
            }

            else
            {
                foreach (var line in DisplayedSalesTransactions)
                    line.IsSelected = false;
            }
        }

        private void UpdateAccordingToCitySalesmanSalesTransactions()
        {
            DisplayedSalesTransactions.Clear();
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
                    DisplayedSalesTransactions.Add(vm);
                    _total += vm.Remaining;
                }

                OnPropertyChanged("Total");
            }
        }

        private void UpdateAccordingToCustomerSalesTransactions()
        {
            DisplayedSalesTransactions.Clear();
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
                    DisplayedSalesTransactions.Add(new SalesTransactionMultiPurposeVM { Model = transaction });
                    _total += transaction.Total - transaction.Paid;
                }

                OnPropertyChanged("Total");
            }
        }

        private void UpdateAccordingToCollectionDateSalesTransactions()
        {
            DisplayedSalesTransactions.Clear();
            using (var context = new ERPContext())
            {
                var ledgerTransactions = context.Ledger_Transactions.Where(e => e.Description.Equals("Sales Transaction Receipt") && e.Date.Equals(_collectionDate)).ToList();
                var temp = new List<SalesTransactionMultiPurposeVM>();
                var emptyCollectionSalesman = context.Salesmans.FirstOrDefault(e => e.Name.Equals(" "));
                foreach (var lt in ledgerTransactions)
                {
                    var transaction = context.SalesTransactions
                        .Include("Customer")
                        .Include("Customer.Group")
                        .Include("CollectionSalesman").FirstOrDefault(e => e.SalesTransactionID.Equals(lt.Documentation));

                    var vm = new SalesTransactionMultiPurposeVM { Model = transaction };

                    if (temp.Contains(vm)) continue;
                    if (vm.CollectionSalesman == null) vm.CollectionSalesman = emptyCollectionSalesman;
                    temp.Add(vm);
                    _total += vm.Remaining;
                }


                var sort = temp.OrderBy(e => e.CollectionSalesman.Name).ToList();

                foreach (var line in sort)
                {
                    DisplayedSalesTransactions.Add(line);
                }
            }
        }

        private void UpdateDisplayedSalesTransactions()
        {
            if (_selectedCity != null && _selectedSalesman != null)
                UpdateAccordingToCitySalesmanSalesTransactions();
            if ((_selectedCity != null && _selectedSalesman == null) || (_selectedCity == null && _selectedSalesman != null))
                DisplayedSalesTransactions.Clear();
            if (_selectedCustomer != null)
                UpdateAccordingToCustomerSalesTransactions();
            if (_selectedCustomer == null && _selectedCity == null && _selectedSalesman == null)
                UpdateAccordingToCollectionDateSalesTransactions();
        }
        #endregion
    }
}
