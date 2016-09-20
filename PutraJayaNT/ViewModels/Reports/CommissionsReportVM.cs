namespace ECRP.ViewModels.Reports
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using ECRP.Reports.Windows;
    using Models.Sales;
    using Models.Salesman;
    using MVVMFramework;
    using Salesman;
    using Utilities;

    public class CommissionsReportVM : ViewModelBase
    {
        ObservableCollection<SalesmanVM> _salesmen;
        ObservableCollection<SalesCommissionVM> _lines;

        SalesmanVM _selectedSalesman;
        DateTime _fromDate;
        DateTime _toDate;
        decimal _total;

        ICommand _printCommand;

        public CommissionsReportVM()
        {
            _salesmen = new ObservableCollection<SalesmanVM>();
            _lines = new ObservableCollection<SalesCommissionVM>();

            _fromDate = UtilityMethods.GetCurrentDate().Date.AddDays(-UtilityMethods.GetCurrentDate().Day + 1);
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateSalesmen();
        }

        public ObservableCollection<SalesmanVM> Salesmen
        {
            get { return _salesmen; }
        }

        public ObservableCollection<SalesCommissionVM> Lines
        {
            get { return _lines; }
        }

        public SalesmanVM SelectedSalesman
        {
            get { return _selectedSalesman; }
            set
            {
                SetProperty(ref _selectedSalesman, value, "SelectedSalesman");

                if (_selectedSalesman == null) return;

                UpdateLines();
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

                if (_selectedSalesman == null) return;

                UpdateLines();
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

                if (_selectedSalesman == null) return;

                UpdateLines();
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

                    var commissionsReportWindow = new CommissionsReportWindow(this);
                    commissionsReportWindow.Owner = App.Current.MainWindow;
                    commissionsReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    commissionsReportWindow.Show();
                }));
            }
        }

        #region Helper Methods
        private void UpdateSalesmen()
        {
            _salesmen.Clear();
            _salesmen.Add(new SalesmanVM
            {
                Model = new Salesman { ID = -1, Name = "All" }
            });

            using (var context = UtilityMethods.createContext())
            {
                var salesmen = context.Salesmans.ToList().OrderBy(e => e.Name);

                foreach (var salesman in salesmen)
                    if (!salesman.Name.Equals(" ")) _salesmen.Add(new SalesmanVM { Model = salesman });
            }
        }

        private void UpdateLines()
        {
            _lines.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var categories = context.ItemCategories.OrderBy(e => e.Name).ToList();
                _total = 0;

                List<SalesTransactionLine> transactionLines;
                if (_selectedSalesman.Name.Equals("All"))
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("SalesTransaction")
                        .Include("Item")
                        .Include("Item.Category")
                        .Where(e => e.SalesTransaction.Date >= _fromDate &&
                        e.SalesTransaction.Date <= _toDate)
                        .ToList();
                }

                else
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("SalesTransaction")
                        .Include("Item")
                        .Include("Item.Category")
                        .Where(e => e.SalesTransaction.Date >= _fromDate &&
                        e.SalesTransaction.Date <= _toDate && e.Salesman.ID.Equals(_selectedSalesman.ID))
                        .ToList();
                }

                if (_selectedSalesman.Name.Equals("All"))
                {
                    foreach (var salesman in _salesmen)
                    {
                        foreach (var category in categories)
                        {
                            if (!salesman.Name.Equals("All"))
                            {
                                var commission = context.SalesCommissions
                                    .Include("Salesman")
                                    .Include("Category")
                                    .Where(e => e.Category_ID.Equals(category.ID) &&
                                    e.Salesman_ID.Equals(salesman.ID))
                                    .FirstOrDefault();

                                SalesCommissionVM categoryCommission;
                                if (commission != null)
                                    categoryCommission = new SalesCommissionVM { Model = commission };
                                else
                                {
                                    categoryCommission = new SalesCommissionVM
                                    {
                                        Model = new SalesCommission
                                        {
                                            Salesman = salesman.Model,
                                            Category = category,
                                            Percentage = 0
                                        }
                                    };
                                }

                                var categorySalesTransactionLines = transactionLines
                                    .Where(e => e.Item.Category.ID.Equals(category.ID)
                                    && e.Salesman_ID.Equals(salesman.ID));

                                foreach (var l in categorySalesTransactionLines)
                                    categoryCommission.Total += GetLineNetTotal(l);
                                categoryCommission.Commission = categoryCommission.Total * (categoryCommission.Percentage / 100);

                                _lines.Add(categoryCommission);
                                _total += categoryCommission.Commission;
                            }
                        }
                    }
                }

                else
                {
                    foreach (var category in categories)
                    {
                        var commission = context.SalesCommissions
                        .Include("Salesman")
                        .Include("Category")
                        .Where(e => e.Salesman_ID.Equals(_selectedSalesman.ID) &&
                        e.Category_ID.Equals(category.ID))
                        .FirstOrDefault();

                        SalesCommissionVM categoryCommission;
                        if (commission != null)
                            categoryCommission = new SalesCommissionVM { Model = commission };
                        else
                        {
                            categoryCommission = new SalesCommissionVM
                            {
                                Model = new SalesCommission
                                {
                                    Salesman = _selectedSalesman.Model,
                                    Category = category,
                                    Percentage = 0
                                }
                            };
                        }

                        var categorySalesTransactionLines = transactionLines.Where(e => e.Item.Category.ID.Equals(category.ID));
                        foreach (var l in categorySalesTransactionLines)
                            categoryCommission.Total += GetLineNetTotal(l);
                        categoryCommission.Commission = categoryCommission.Total * (categoryCommission.Percentage / 100);

                        _lines.Add(categoryCommission);
                        _total += categoryCommission.Commission;
                    }
                }
            }
            OnPropertyChanged("Total");
        }

        private decimal GetLineNetTotal(SalesTransactionLine line)
        {
            decimal netTotal = 0;

            using (var context = UtilityMethods.createContext())
            {
                var transaction = context.SalesTransactionLines
                    .Include("SalesTransaction")
                    .Where(e => e.SalesTransactionID.Equals(line.SalesTransaction.SalesTransactionID))
                    .FirstOrDefault()
                    .SalesTransaction;

                var lineDiscount = line.Discount / line.Item.PiecesPerUnit;
                var lineSalesPrice = line.SalesPrice / line.Item.PiecesPerUnit;
                var lineTotal = line.SalesPrice - line.Discount;
                if (lineTotal == 0) return 0;
                var fractionOfTransaction = (line.Quantity * (lineSalesPrice - lineDiscount)) / transaction.GrossTotal;
                var fractionOfTransactionDiscount = (fractionOfTransaction * transaction.Discount) / line.Quantity;
                var discount = lineDiscount + fractionOfTransactionDiscount;
                netTotal = line.Quantity * (line.SalesPrice - discount);
            }
            return netTotal;
        }
        #endregion
    }
}
