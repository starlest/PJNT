namespace PutraJayaNT.ViewModels.Reports
{
    using MVVMFramework;
    using Models.Sales;
    using Models.Salesman;
    using Utilities;
    using Salesman;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;


    class CommissionsReportVM : ViewModelBase
    {
        ObservableCollection<SalesmanVM> _salesmen;
        ObservableCollection<SalesCommissionVM> _lines;

        SalesmanVM _selectedSalesman;
        DateTime _fromDate;
        DateTime _toDate;
        decimal _total;

        public CommissionsReportVM()
        {
            _salesmen = new ObservableCollection<SalesmanVM>();
            _lines = new ObservableCollection<SalesCommissionVM>();

            _fromDate = DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1);
            _toDate = DateTime.Now.Date;

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

        #region Helper Methods
        private void UpdateSalesmen()
        {
            _salesmen.Clear();
            _salesmen.Add(new SalesmanVM
            {
                Model = new Salesman { ID = -1, Name = "All" }
            });

            using (var context = new ERPContext())
            {
                var salesmen = context.Salesmans.ToList().OrderBy(e => e.Name);

                foreach (var salesman in salesmen)
                    _salesmen.Add(new SalesmanVM { Model = salesman });
            }
        }

        private void UpdateLines()
        {
            _lines.Clear();
            using (var context = new ERPContext())
            {
                var categories = context.Categories.OrderBy(e => e.Name).ToList();
                _total = 0;

                List<SalesTransactionLine> transactionLines;
                if (_selectedSalesman.Name.Equals("All"))
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("SalesTransaction")
                        .Include("Item")
                        .Include("Item.Category")
                        .Where(e => e.SalesTransaction.When >= _fromDate &&
                        e.SalesTransaction.When <= _toDate)
                        .ToList();
                }

                else
                {
                    transactionLines = context.SalesTransactionLines
                        .Include("SalesTransaction")
                        .Include("Item")
                        .Include("Item.Category")
                        .Where(e => e.SalesTransaction.When >= _fromDate &&
                        e.SalesTransaction.When <= _toDate && e.Salesman.ID.Equals(_selectedSalesman.ID))
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
                                categoryCommission.Commission = categoryCommission.Total * categoryCommission.Percentage;

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
                        categoryCommission.Commission = categoryCommission.Total * categoryCommission.Percentage;

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

            using (var context = new ERPContext())
            {
                var transaction = context.SalesTransactionLines
                    .Include("SalesTransaction")
                    .Where(e => e.SalesTransactionID.Equals(line.SalesTransaction.SalesTransactionID))
                    .FirstOrDefault()
                    .SalesTransaction;

                var lineDiscount = line.Discount / line.Item.PiecesPerUnit;
                var lineSalesPrice = line.SalesPrice / line.Item.PiecesPerUnit;
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
