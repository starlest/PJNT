namespace PutraJayaNT.ViewModels.Customers.Sales
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Sales;
    using MVVMFramework;
    using PutraJayaNT.Reports.Windows;
    using Utilities;

    internal class BrowseSalesTransactionsVM : ViewModelBase
    {
        private DateTime _fromDate;
        private DateTime _toDate;
        private decimal _total;

        ICommand _printCommand;

        public BrowseSalesTransactionsVM()
        {
            SalesTransactions = new ObservableCollection<SalesTransaction>();
            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateSalesTransactions();
        }

        public ObservableCollection<SalesTransaction> SalesTransactions { get; }

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
                    if (SalesTransactions.Count == 0) return;

                    var salesTransactionsReportWindow = new SalesTransactionsReportWindow(SalesTransactions)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    salesTransactionsReportWindow.Show();
                }));
            }
        }

        #region Helper methods
        private void UpdateSalesTransactions()
        {
            _total = 0;
            SalesTransactions.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var salesTransactions = context.SalesTransactions
                    .Include("User")
                    .Include("Customer")
                    .Where(e => e.Date >= _fromDate && e.Date <= _toDate)
                    .OrderBy(e => e.Date)
                    .ThenBy(e => e.SalesTransactionID);

                foreach (var t in salesTransactions)
                {
                    SalesTransactions.Add(t);
                    _total += t.NetTotal;
                }
            }

            OnPropertyChanged("Total");
        }
        #endregion
    }
}
