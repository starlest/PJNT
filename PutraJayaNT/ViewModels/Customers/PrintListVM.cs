using MVVMFramework;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels.Customers
{
    class PrintListVM : ViewModelBase
    {
        ObservableCollection<SalesTransaction> _salesTransactions;
        ObservableCollection<string> _modes;

        string _selectedMode;
        DateTime _fromDate;
        DateTime _toDate;

        public PrintListVM()
        {
            _salesTransactions = new ObservableCollection<SalesTransaction>();
            _modes = new ObservableCollection<string> { "Printed", "Not Printed" };
            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;

            SelectedMode = _modes.FirstOrDefault();
        }

        public ObservableCollection<SalesTransaction> SalesTransactions
        {
            get { return _salesTransactions; }
        }

        public ObservableCollection<string> Modes
        {
            get { return _modes; }
        }

        public string SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                SetProperty(ref _selectedMode, value, "SelectedMode");

                if (_selectedMode == null) return;

                UpdateSalesTransactions();
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
            _salesTransactions.Clear();
            using (var context = new ERPContext())
            {
                List<SalesTransaction> salesTransactions;

                if (_selectedMode.Equals("Printed"))
                {
                    salesTransactions = context.SalesTransactions
                        .Include("User")
                        .Include("Customer")
                        .Where(e => e.InvoicePrinted && e.Date >= _fromDate && e.Date <= _toDate)
                        .OrderBy(e => e.Date)
                        .ThenBy(e => e.SalesTransactionID)
                        .ToList();
                }

                else
                {
                    salesTransactions = context.SalesTransactions
                        .Include("User")
                        .Include("Customer")
                        .Where(e => !e.InvoicePrinted && e.Date >= _fromDate && e.Date <= _toDate)
                        .OrderBy(e => e.Date)
                        .ThenBy(e => e.SalesTransactionID)
                        .ToList();
                }

                foreach (var t in salesTransactions)
                    _salesTransactions.Add(t);
            }
        }
        #endregion
    }
}
