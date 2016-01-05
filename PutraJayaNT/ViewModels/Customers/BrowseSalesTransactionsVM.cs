using MVVMFramework;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels.Customers
{
    class BrowseSalesTransactionsVM : ViewModelBase
    {
        ObservableCollection<SalesTransaction> _salesTransactions;

        DateTime _fromDate;
        DateTime _toDate;

        public BrowseSalesTransactionsVM()
        {
            _salesTransactions = new ObservableCollection<SalesTransaction>();
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;

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

        #region Helper methods
        private void UpdateSalesTransactions()
        {
            using (var context = new ERPContext())
            {
                var salesTransactions = context.SalesTransactions
                    .Include("User")
                    .Include("Customer")
                    .Include("Salesman")
                    .Where(e => e.When >= _fromDate && e.When <= _toDate)
                    .ToList();

                foreach (var t in salesTransactions)
                    _salesTransactions.Add(t);
            }
        }
        #endregion
    }
}
