using MVVMFramework;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Reports;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Customers
{
    class PaymentListVM : ViewModelBase
    {
        ObservableCollection<string> _cities;
        ObservableCollection<SalesTransaction> _salesTransactions;

        string _selectedCity;
        DateTime _fromDate;
        DateTime _toDate;
        ICommand _printCommand;

        public PaymentListVM()
        {
            _cities = new ObservableCollection<string>();
            _salesTransactions = new ObservableCollection<SalesTransaction>();
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;
            UpdateCities();
        }

        public ObservableCollection<string> Cities
        {
            get { return _cities; }
        }

        public ObservableCollection<SalesTransaction> SalesTransactions
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
                UpdateSalesTransactions();
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
                UpdateSalesTransactions();
            }
        }

        public string SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                SetProperty(ref _selectedCity, value, "SelectedCity");
                UpdateSalesTransactions();
            }
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

        private void UpdateSalesTransactions()
        {

            _salesTransactions.Clear();

            using (var context = new ERPContext())
            {
                var salesTransactions = context.SalesTransactions
                    .Include("Customer")
                    .Include("Customer.Group")
                    .Where(e => e.Customer.City.Equals(_selectedCity) && e.Paid < e.Total && (e.DueDate >= _fromDate && e.DueDate <= _toDate))
                    .OrderBy(e => e.DueDate)
                    .ThenBy(e => e.Customer.Name)
                    .ToList();

                foreach (var t in salesTransactions)
                {
                    t.Remaining = t.Total - t.Paid;
                    _salesTransactions.Add(t);
                }
            }
        }
        #endregion
    }
}
