namespace PutraJayaNT.ViewModels.Customers.Sales
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Models.Sales;
    using MVVMFramework;
    using Utilities;

    internal class PrintListVM : ViewModelBase
    {
        private string _selectedMode;
        private DateTime _fromDate;
        private DateTime _toDate;

        public PrintListVM()
        {
            SalesTransactions = new ObservableCollection<SalesTransaction>();
            Modes = new ObservableCollection<string> { "Printed", "Not Printed" };
            _fromDate = UtilityMethods.GetCurrentDate().Date;
            _toDate = UtilityMethods.GetCurrentDate().Date;

            SelectedMode = Modes.FirstOrDefault();
        }

        public ObservableCollection<SalesTransaction> SalesTransactions { get; }

        public ObservableCollection<string> Modes { get; }

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
            SalesTransactions.Clear();
            using (var context = UtilityMethods.createContext())
            {
                IEnumerable<SalesTransaction> salesTransactions;

                if (_selectedMode.Equals("Printed"))
                {
                    salesTransactions = context.SalesTransactions
                        .Include("User")
                        .Include("Customer")
                        .Where(e => e.InvoicePrinted && e.Date >= _fromDate && e.Date <= _toDate)
                        .OrderBy(e => e.Date)
                        .ThenBy(e => e.SalesTransactionID);
                }

                else
                {
                    salesTransactions = context.SalesTransactions
                        .Include("User")
                        .Include("Customer")
                        .Where(e => !e.InvoicePrinted && e.Date >= _fromDate && e.Date <= _toDate)
                        .OrderBy(e => e.Date)
                        .ThenBy(e => e.SalesTransactionID);
                }

                foreach (var salesTransaction in salesTransactions)
                    SalesTransactions.Add(salesTransaction);
            }
        }
        #endregion
    }
}
