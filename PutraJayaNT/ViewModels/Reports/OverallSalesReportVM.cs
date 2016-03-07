using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Reports.Windows;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Reports
{
    class OverallSalesReportVM : ViewModelBase
    {
        ObservableCollection<SalesTransactionMultiPurposeVM> _lines;
        ObservableCollection<CustomerVM> _customers;

        CustomerVM _selectedCustomer;
        DateTime _fromDate;
        DateTime _toDate;
        decimal _total;
        decimal _remaining;
        ICommand _printCommand;

        public OverallSalesReportVM()
        {
            _customers = new ObservableCollection<CustomerVM>();
            _lines = new ObservableCollection<SalesTransactionMultiPurposeVM>();
            var currentDate = UtilityMethods.GetCurrentDate();
            _fromDate = currentDate.AddDays(-currentDate.Day + 1);
            _toDate = currentDate;
            UpdateCustomers();
        }

        public ObservableCollection<SalesTransactionMultiPurposeVM> Lines
        {
            get { return _lines; }
        }

        public ObservableCollection<CustomerVM> Customers
        {
            get { return _customers; }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                SetProperty(ref _selectedCustomer, value, "SelectedCustomer");

                if (_selectedCustomer == null) return;

                UpdateCustomers();
                RefreshDisplayLines();
            }
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
                if (_selectedCustomer != null) RefreshDisplayLines();
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
                if (_selectedCustomer != null) RefreshDisplayLines();
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
                    if (_lines.Count == 0) return;

                    var overallSalesReportWindow = new OverallSalesReportWindow(_lines);
                    overallSalesReportWindow.Owner = App.Current.MainWindow;
                    overallSalesReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    overallSalesReportWindow.Show();
                }));
            }
        }

        public decimal Remaining
        {
            get { return _remaining; }
            set { SetProperty(ref _remaining, value, "Remaining"); }
        }

        private void UpdateCustomers()
        {
            _customers.Clear();
            _customers.Add(new CustomerVM { Model = new Customer { ID = -1, Name = "All" } });
            using (var context = new ERPContext())
            {
                var customers = context.Customers.OrderBy(e => e.Name).ToList();

                foreach (var customer in customers)
                    _customers.Add(new CustomerVM { Model = customer }); 
            }
        }

        private void RefreshDisplayLines()
        {
            _lines.Clear();
            _total = 0;
            using (var context = new ERPContext())
            {
                List<SalesTransaction> salesTransactions;
                if (!_selectedCustomer.Name.Equals("All"))
                {
                    salesTransactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("User")
                        .Where(e => e.Customer.Name.Equals(_selectedCustomer.Name) && e.When >= _fromDate && e.When <= _toDate)
                        .OrderBy(e => e.When)
                        .ThenBy(e => e.SalesTransactionID)
                        .ToList();
                }

                else
                {
                    salesTransactions = context.SalesTransactions
                        .Include("Customer")
                        .Include("User")
                        .Where(e => e.When >= _fromDate && e.When <= _toDate)
                        .OrderBy(e => e.Customer.Name)
                        .ThenBy(e => e.When)
                        .ThenBy(e => e.SalesTransactionID)
                        .ToList();
                }

                foreach (var st in salesTransactions)
                {
                    _lines.Add(new SalesTransactionMultiPurposeVM { Model = st });
                    _total += st.Total;
                    _remaining += (st.Total - st.Paid);
                }

                OnPropertyChanged("Remaining");
                OnPropertyChanged("Total");
            }
        }
    }
}
