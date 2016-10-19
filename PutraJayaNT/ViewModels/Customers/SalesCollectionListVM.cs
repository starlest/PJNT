namespace ECRP.ViewModels.Customers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using ECRP.Reports.Windows;
    using Models.Accounting;
    using Models.Customer;
    using Models.Sales;
    using Models.Salesman;
    using MVVMFramework;
    using Salesman;
    using Utilities;
    using ViewModels.Sales;

    public class SalesCollectionListVM : ViewModelBase
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

        private ICommand _searchCommand;
        private ICommand _searchCollectionDateSelectionCommand;
        private ICommand _printPerCityCommand;
        private ICommand _printPerSalesmanCommand;

        #endregion

        public SalesCollectionListVM()
        {
            Cities = new ObservableCollection<string>();
            Salesmans = new ObservableCollection<SalesmanVM>();
            Customers = new ObservableCollection<CustomerVM>();
            DisplayedSalesTransactions = new ObservableCollection<SalesTransactionVM>();

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

        public ObservableCollection<SalesTransactionVM> DisplayedSalesTransactions { get; }

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
                SetProperty(ref _allSelected, value, () => AllSelected);
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

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(() =>
                       {
                           UpdateDisplayedSalesTransactions();
                           UpdateCities();
                           UpdateSalesmen();
                           UpdateCustomers();
                       }));
            }
        }

        public ICommand SearchCollectionDateSelectionCommand
        {
            get
            {
                return _searchCollectionDateSelectionCommand ??
                       (_searchCollectionDateSelectionCommand = new RelayCommand(() =>
                       {
                           SelectedCustomer = null;
                           SelectedCity = null;
                           SelectedSalesman = null;
                           UpdateDisplayedSalesTransactionsAccordingToCollectionDateSalesTransactions();
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

                           var collectionReportWindow = new CollectionReportPerCityWindow(DisplayedSalesTransactions)
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

                           var collectionReportWindow = new CollectionReportPerSalesmanWindow(
                               DisplayedSalesTransactions,
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
            using (var context = UtilityMethods.createContext())
            {
                var customersReturnedFromDatabase = context.Customers.ToList();
                foreach (
                    var customer in customersReturnedFromDatabase.Where(customer => !Cities.Contains(customer.City)))
                    Cities.Add(customer.City);
            }

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

            using (var context = UtilityMethods.createContext())
            {
                var salesmansFromDatabase =
                    context.Salesmans.Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name);
                var allSalesman = new Salesman { ID = -1, Name = "All" };
                Salesmans.Add(new SalesmanVM { Model = allSalesman });
                foreach (var salesman in salesmansFromDatabase)
                    Salesmans.Add(new SalesmanVM { Model = salesman });
            }

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

            using (var context = UtilityMethods.createContext())
            {
                var allCustomer = new Customer { ID = -1, Name = "All" };
                Customers.Add(new CustomerVM { Model = allCustomer });
                var customersFromDatabase = context.Customers.OrderBy(customer => customer.Name);
                foreach (var customer in customersFromDatabase)
                    Customers.Add(new CustomerVM { Model = customer });
            }

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

        private void UpdateDisplayedSalesTransactions()
        {
            if (_selectedCity != null && _selectedSalesman != null)
                UpdateDisplayedSalesTransactionsAccordingToCitySalesmanSalesTransactions();
            if ((_selectedCity != null && _selectedSalesman == null) ||
                (_selectedCity == null && _selectedSalesman != null))
                DisplayedSalesTransactions.Clear();
            if (_selectedCustomer != null)
                UpdateDisplayedSalesTransactionsAccordingToCustomerSalesTransactions();
            UpdateTotalToUI();
        }

        private void UpdateDisplayedSalesTransactionsAccordingToCitySalesmanSalesTransactions()
        {
            _total = 0;
            var searchCondition = GetCitySalesmanSelectionSearchCondition();
            using (var context = UtilityMethods.createContext())
            {
                var salesTransactionsFromDatabase =
                    context.SalesTransactions
                        .Include("Customer")
                        .Include("CollectionSalesman")
                        .Where(searchCondition)
                        .OrderBy(salesTransaction => salesTransaction.DueDate)
                        .ThenBy(salesTransaction => salesTransaction.Date)
                        .ThenBy(salesTransaction => salesTransaction.Customer.Name);

                DisplayedSalesTransactions.Clear();
                foreach (
                    var salesTransactionVM in
                    salesTransactionsFromDatabase.Select(
                        salesTransaction => new SalesTransactionVM { Model = salesTransaction }))
                {
                    DisplayedSalesTransactions.Add(salesTransactionVM);
                    _total += salesTransactionVM.Remaining;
                }
            }
        }

        private Func<SalesTransaction, bool> GetCitySalesmanSelectionSearchCondition()
        {
            Func<SalesTransaction, bool> searchCondition;

            if (!_isPaidChecked)
            {
                if (_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    searchCondition =
                        salesTransaction =>
                                salesTransaction.Paid < salesTransaction.NetTotal && salesTransaction.DueDate <= _toDate;

                else if (_selectedCity.Equals("All") && !_selectedSalesman.Name.Equals("All"))
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.CollectionSalesman.ID.Equals(_selectedSalesman.ID)
                            && salesTransaction.Paid < salesTransaction.NetTotal &&
                            salesTransaction.DueDate <= _toDate;

                else if (!_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.Customer.City.Equals(_selectedCity) &&
                            salesTransaction.Paid < salesTransaction.NetTotal && salesTransaction.DueDate <= _toDate;

                else
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.Customer.City.Equals(_selectedCity) &&
                            salesTransaction.CollectionSalesman.ID.Equals(_selectedSalesman.ID)
                            && salesTransaction.Paid < salesTransaction.NetTotal && salesTransaction.DueDate <= _toDate;
            }

            else
            {
                if (_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.Paid >= salesTransaction.NetTotal &&
                            salesTransaction.DueDate >= _fromDate && salesTransaction.DueDate <= _toDate;

                else if (_selectedCity.Equals("All") && !_selectedSalesman.Name.Equals("All"))
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.CollectionSalesman.ID.Equals(_selectedSalesman.ID) &&
                            salesTransaction.Paid >= salesTransaction.NetTotal &&
                            salesTransaction.DueDate >= _fromDate && salesTransaction.DueDate <= _toDate;

                else if (!_selectedCity.Equals("All") && _selectedSalesman.Name.Equals("All"))
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.Customer.City.Equals(_selectedCity) &&
                            salesTransaction.Paid >= salesTransaction.NetTotal && salesTransaction.DueDate >= _fromDate &&
                            salesTransaction.DueDate <= _toDate;

                else
                    searchCondition =
                        salesTransaction =>
                            salesTransaction.Customer.City.Equals(_selectedCity) &&
                            salesTransaction.CollectionSalesman.ID.Equals(_selectedSalesman.ID) &&
                            salesTransaction.Paid >= salesTransaction.NetTotal && salesTransaction.DueDate >= _fromDate &&
                            salesTransaction.DueDate <= _toDate;
            }

            return searchCondition;
        }

        private void UpdateDisplayedSalesTransactionsAccordingToCustomerSalesTransactions()
        {
            _total = 0;
            var searchCondition = GetCustomerSelectionSearchCondition();
            using (var context = UtilityMethods.createContext())
            {
                var salesTransactionsFromDatabase =
                    context.SalesTransactions
                        .Include("Customer")
                        .Include("CollectionSalesman")
                        .Where(searchCondition)
                        .OrderBy(salesTransaction => salesTransaction.DueDate)
                        .ThenBy(salesTransaction => salesTransaction.Date)
                        .ThenBy(salesTransaction => salesTransaction.Customer.Name)
                        .ToList();

                DisplayedSalesTransactions.Clear();
                foreach (
                    var salesTransactionVM in
                    salesTransactionsFromDatabase.Select(
                        salesTransaction => new SalesTransactionVM { Model = salesTransaction }))
                {
                    DisplayedSalesTransactions.Add(salesTransactionVM);
                    _total += salesTransactionVM.Remaining;
                }
            }
        }

        private Func<SalesTransaction, bool> GetCustomerSelectionSearchCondition()
        {
            if (_selectedCustomer.Name.Equals("All") && !_isPaidChecked)
                return
                    salesTransaction =>
                            salesTransaction.Paid < salesTransaction.NetTotal && salesTransaction.DueDate <= _toDate;

            if (_selectedCustomer.Name.Equals("All") && _isPaidChecked)
                return salesTransaction => salesTransaction.Paid >= salesTransaction.NetTotal &&
                                           salesTransaction.DueDate <= _toDate && salesTransaction.DueDate >= _fromDate;

            if (!_selectedCustomer.Name.Equals("All") && !_isPaidChecked)
                return
                    salesTransaction =>
                        salesTransaction.Paid < salesTransaction.NetTotal &&
                        salesTransaction.Customer.ID.Equals(_selectedCustomer.ID);

            return salesTransaction => salesTransaction.Customer.ID.Equals(_selectedCustomer.ID) &&
                                       salesTransaction.Paid >= salesTransaction.NetTotal &&
                                       salesTransaction.DueDate >= _fromDate && salesTransaction.DueDate >= _fromDate &&
                                       salesTransaction.DueDate <= _toDate;
        }

        private void UpdateDisplayedSalesTransactionsAccordingToCollectionDateSalesTransactions()
        {
            _total = 0;
            using (var context = UtilityMethods.createContext())
            {
                var emptyCollectionSalesman = context.Salesmans.SingleOrDefault(e => e.Name.Equals(" "));
                var ledgerTransactions = context.Ledger_Transactions
                    .Where(
                        transaction =>
                            transaction.Description.Equals("Sales Transaction Receipt") &&
                            transaction.Date.Equals(_collectionDate));

                DisplayedSalesTransactions.Clear();
                foreach (var ledgerTransaction in ledgerTransactions)
                {
                    var vm = GetCorrespondingSalesTransactionVM(ledgerTransaction);
                    if (DisplayedSalesTransactions.Contains(vm)) continue;
                    if (vm.CollectionSalesman == null) vm.CollectionSalesman = emptyCollectionSalesman;
                    DisplayedSalesTransactions.Add(vm);
                    _total += vm.Remaining;
                }
            }
            SortDisplayedTransactionsAccordingToCollectionSalesman();
            UpdateTotalToUI();
        }

        private static SalesTransactionVM GetCorrespondingSalesTransactionVM(LedgerTransaction ledgerTransaction)
        {
            using (var context = UtilityMethods.createContext())
            {
                var salesTransactionFromDatabase = context.SalesTransactions
                    .Include("CollectionSalesman")
                    .Include("Customer")
                    .SingleOrDefault(
                        salesTransaction => salesTransaction.SalesTransactionID.Equals(ledgerTransaction.Documentation));
                return new SalesTransactionVM { Model = salesTransactionFromDatabase };
            }
        }

        private void SortDisplayedTransactionsAccordingToCollectionSalesman()
        {
            var sortedList = DisplayedSalesTransactions.OrderBy(e => e.CollectionSalesman.Name).ToList();
            DisplayedSalesTransactions.Clear();
            foreach (var line in sortedList)
                DisplayedSalesTransactions.Add(line);
        }

        private void UpdateTotalToUI()
        {
            OnPropertyChanged("Total");
        }

        #endregion
    }
}