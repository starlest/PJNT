using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels
{
    class SalesReturnsReportVM : ViewModelBase
    {
        DateTime _fromDate;
        DateTime _toDate;

        ObservableCollection<SalesReturnTransaction> _salesReturns;
        ObservableCollection<SalesReturnTransactionLine> _displayLines;

        SalesReturnTransaction _selectedSalesReturn;

        public SalesReturnsReportVM()
        {
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;

            _salesReturns = new ObservableCollection<SalesReturnTransaction>();
            _displayLines = new ObservableCollection<SalesReturnTransactionLine>();

            RefreshSalesReturns();
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
                RefreshSalesReturns();
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
                RefreshSalesReturns();
            }
        }

        public ObservableCollection<SalesReturnTransaction> SalesReturns
        {
            get { return _salesReturns; }
        }

        public ObservableCollection<SalesReturnTransactionLine> DisplayLines
        {
            get { return _displayLines; }
        }

        public SalesReturnTransaction SelectedSalesReturn
        {
            get { return _selectedSalesReturn; }
            set
            {
                SetProperty(ref _selectedSalesReturn, value, "SelectedSalesReturn");

                if (_selectedSalesReturn == null) return;

                UpdateDisplayLines();
            }
        }

        private void RefreshSalesReturns()
        {
            _salesReturns.Clear();
            _displayLines.Clear();

            using (var context = new ERPContext())
            {
                var salesReturns = context.SalesReturnTransactions
                    .Include("TransactionLines")
                    .Where(e => e.Date >= _fromDate && e.Date <= _toDate)
                    .ToList();

                foreach (var salesReturn in salesReturns)
                    _salesReturns.Add(salesReturn);
            }
        }

        private void UpdateDisplayLines()
        {
            _displayLines.Clear();

            using (var context = new ERPContext())
            {
                var salesReturnLines = context.SalesReturnTransactionLines
                    .Include("Item")
                    .Where(e => e.SalesReturnTransactionID == _selectedSalesReturn.SalesReturnTransactionID)
                    .ToList();

                foreach (var line in salesReturnLines)
                    _displayLines.Add(line);
            }
        }
    }
}
