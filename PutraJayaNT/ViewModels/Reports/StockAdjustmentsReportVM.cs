namespace ECERP.ViewModels.Reports
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Inventory;
    using MVVMFramework;
    using Utilities;

    class StockAdjustmentsReportVM : ViewModelBase
    {
        private DateTime _fromDate;
        private DateTime _toDate;
        private ICommand _displayCommand;

        public StockAdjustmentsReportVM()
        {
            var currentDate = UtilityMethods.GetCurrentDate();
            _fromDate = currentDate.AddDays(-currentDate.Day + 1);
            _toDate = currentDate;
            DisplayedStockAdjustments = new ObservableCollection<StockAdjustmentTransactionLineVM>();
        }

        public ObservableCollection<StockAdjustmentTransactionLineVM> DisplayedStockAdjustments { get; }

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

                SetProperty(ref _fromDate, value, () => FromDate);
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

                SetProperty(ref _toDate, value, () => ToDate);
            }
        }

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() => UpdateDisplayedStockAdjustments()));
            }
        }

        private void UpdateDisplayedStockAdjustments()
        {
            DisplayedStockAdjustments.Clear();

            using (var context = UtilityMethods.createContext())
            {
                var stockAdjustmentsFromDB = context.StockAdjustmentTransactionLines
                    .Include("StockAdjustmentTransaction")
                    .Include("StockAdjustmentTransaction.User")
                    .Include("Item")
                    .Include("Warehouse")
                    .Where(stockAdjustmentLine => stockAdjustmentLine.StockAdjustmentTransaction.Date >= _fromDate &&
                                                  stockAdjustmentLine.StockAdjustmentTransaction.Date <= _toDate)
                    .OrderBy(stockAdjustmentLine => stockAdjustmentLine.StockAdjustmentTransaction.Date)
                    .ThenBy(
                        stockAdjustmentLine =>
                                stockAdjustmentLine.StockAdjustmentTransaction.StockAdjustmentTransactionID);

                foreach (var stockAdjustment in stockAdjustmentsFromDB)
                {
                    DisplayedStockAdjustments.Add(new StockAdjustmentTransactionLineVM { Model = stockAdjustment });
                }
            }
        }
    }
}