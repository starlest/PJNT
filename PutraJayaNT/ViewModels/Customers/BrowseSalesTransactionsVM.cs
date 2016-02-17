using MVVMFramework;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Reports.Windows;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Customers
{
    class BrowseSalesTransactionsVM : ViewModelBase
    {
        ObservableCollection<SalesTransaction> _salesTransactions;

        DateTime _fromDate;
        DateTime _toDate;
        decimal _total;

        ICommand _printCommand;

        public BrowseSalesTransactionsVM()
        {
            _salesTransactions = new ObservableCollection<SalesTransaction>();
            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateSalesTransactions();
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

                    var salesTransactionsReportWindow = new SalesTransactionsReportWindow(_salesTransactions);
                    salesTransactionsReportWindow.Owner = App.Current.MainWindow;
                    salesTransactionsReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    salesTransactionsReportWindow.Show();
                }));
            }
        }

        #region Helper methods
        private void UpdateSalesTransactions()
        {
            _total = 0;
            _salesTransactions.Clear();
            using (var context = new ERPContext())
            {
                var salesTransactions = context.SalesTransactions
                    .Include("User")
                    .Include("Customer")
                    .Where(e => e.When >= _fromDate && e.When <= _toDate)
                    .OrderBy(e => e.When)
                    .ThenBy(e => e.SalesTransactionID)
                    .ToList();

                foreach (var t in salesTransactions)
                {
                    _salesTransactions.Add(t);
                    _total += t.Total;
                }
            }

            OnPropertyChanged("Total");
        }
        #endregion
    }
}
