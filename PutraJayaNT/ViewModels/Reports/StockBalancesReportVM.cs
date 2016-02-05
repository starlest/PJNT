using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Reports.Windows;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Reports
{
    public class StockBalancesReportVM : ViewModelBase
    {
        ObservableCollection<ItemVM> _products;
        ObservableCollection<WarehouseVM> _warehouses;
        ObservableCollection<StockBalanceLineVM> _lines;

        ItemVM _selectedProduct;
        DateTime _fromDate;
        DateTime _toDate;
        string _productUnit;
        string _beginningBalanceString;
        string _endingBalanceString;
        string _totalInString;
        string _totalOutString;

        ICommand _printCommand;

        int _beginningBalance = 0;

        public StockBalancesReportVM()
        {
            _products = new ObservableCollection<ItemVM>();
            _warehouses = new ObservableCollection<WarehouseVM>();
            _lines = new ObservableCollection<StockBalanceLineVM>();
            _fromDate = DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1);
            _toDate = DateTime.Now.Date;
            UpdateProducts();
            UpdateWarehouses();
        }

        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
        }

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }

        public ObservableCollection<StockBalanceLineVM> Lines
        {
            get { return _lines; }
        }

        public ItemVM SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                SetProperty(ref _selectedProduct, value, "SelectedProduct");

                if (_selectedProduct == null) return;

                ProductUnit = _selectedProduct.UnitName + "/" + _selectedProduct.PiecesPerUnit;
                SetBeginningBalance();
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

                if (_selectedProduct == null) return;

                SetBeginningBalance();
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

                if (_selectedProduct == null) return;

                SetBeginningBalance();
                UpdateLines();
            }
        }

        public string BeginningBalanceString
        {
            get { return _beginningBalanceString; }
            set { SetProperty(ref _beginningBalanceString, value, "BeginningBalanceString"); }
        }

        public string EndingBalanceString
        {
            get { return _endingBalanceString; }
            set { SetProperty(ref _endingBalanceString, value, "EndingBalanceString"); }
        }

        public string TotalInString
        {
            get { return _totalInString; }
            set { SetProperty(ref _totalInString, value, "TotalInString"); }
        }

        public string TotalOutString
        {
            get { return _totalOutString; }
            set { SetProperty(ref _totalOutString, value, "TotalOutString"); }
        }

        public string ProductUnit
        {
            get { return _productUnit; }
            set { SetProperty(ref _productUnit, value, "ProductUnit"); }
        }

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (_lines.Count == 0) return;

                    var stockCardReportWindow = new StockCardReportWindow(this);
                    stockCardReportWindow.Owner = App.Current.MainWindow;
                    stockCardReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    stockCardReportWindow.Show();
                }));
            }
        }

        #region Helper Methods
        private void UpdateProducts()
        {
            _products.Clear();
            using (var context = new ERPContext())
            {
                var products = context.Inventory.ToList();

                foreach (var product in products)
                    _products.Add(new ItemVM { Model = product });
            }
        }

        private void UpdateWarehouses()
        {
            using (var context = new ERPContext())
            {
                _warehouses.Clear();

                var warehouses = context.Warehouses.ToList();

                foreach (var warehouse in warehouses)
                    _warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void UpdateLines()
        {
            _lines.Clear();
            var lines = new List<StockBalanceLineVM>();
            using (var context = new ERPContext())
            {
                foreach (var warehouse in _warehouses)
                {
                    if (!warehouse.IsSelected) continue;

                    var purchaseLines = context.PurchaseTransactionLines
                        .Include("PurchaseTransaction")
                        .Include("PurchaseTransaction.Supplier")
                        .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) &&
                        e.PurchaseTransaction.Date >= _fromDate && e.PurchaseTransaction.Date <= _toDate &&
                         !e.PurchaseTransactionID.Substring(0, 2).Equals("SA") && !e.PurchaseTransaction.Supplier.Name.Equals("-"))
                        .ToList();

                    var purchaseReturnLines = context.PurchaseReturnTransactionLines
                        .Include("PurchaseReturnTransaction")
                        .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                        .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID)
                        && e.PurchaseReturnTransaction.Date >= _fromDate && e.PurchaseReturnTransaction.Date <= _toDate)
                        .ToList();

                    var salesLines = context.SalesTransactionLines
                        .Include("SalesTransaction")
                        .Include("SalesTransaction.Customer")
                        .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) 
                        && e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                        .ToList();

                    var salesReturnLines = context.SalesReturnTransactionLines
                        .Include("SalesReturnTransaction")
                        .Include("SalesReturnTransaction.SalesTransaction.Customer")
                        .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) 
                        && e.SalesReturnTransaction.Date >= _fromDate && e.SalesReturnTransaction.Date <= _toDate)
                        .ToList();

                    var stockAdjustmentLines = context.AdjustStockTransactionLines
                        .Include("AdjustStockTransaction")
                        .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) 
                        && e.AdjustStockTransaction.Date >= _fromDate && e.AdjustStockTransaction.Date <= _toDate)
                        .ToList();

                    var moveStockTransactions = context.MoveStockTransactions
                        .Include("FromWarehouse")
                        .Include("ToWarehouse")
                        .Include("MoveStockTransactionLines")
                        .Include("MoveStockTransactionLines.Item")
                        .Where(e => e.Date >= _fromDate && e.Date <= _toDate 
                        && (e.FromWarehouse.ID.Equals(warehouse.ID) || e.ToWarehouse.ID.Equals(warehouse.ID)))
                        .ToList();

                    foreach (var line in purchaseLines)
                    {
                        var vm = new StockBalanceLineVM
                        {
                            Item = line.Item,
                            Date = line.PurchaseTransaction.Date,
                            Documentation = line.PurchaseTransaction.PurchaseID,
                            Description = "Purchase",
                            CustomerSupplier = line.PurchaseTransaction.Supplier.Name,
                            Amount = line.Quantity,
                        };
                        lines.Add(vm);
                    }

                    foreach (var line in purchaseReturnLines)
                    {
                        var vm = new StockBalanceLineVM
                        {
                            Item = line.Item,
                            Date = line.PurchaseReturnTransaction.Date,
                            Documentation = line.PurchaseReturnTransaction.PurchaseReturnTransactionID,
                            Description = "Purchase Return",
                            CustomerSupplier = line.PurchaseReturnTransaction.PurchaseTransaction.Supplier.Name,
                            Amount = -line.Quantity,
                        };
                        lines.Add(vm);
                    }

                    foreach (var line in salesLines)
                    {
                        var vm = new StockBalanceLineVM
                        {
                            Item = line.Item,
                            Date = line.SalesTransaction.When,
                            Documentation = line.SalesTransaction.SalesTransactionID,
                            Description = "Sales",
                            CustomerSupplier = line.SalesTransaction.Customer.Name,
                            Amount = -line.Quantity,
                        };
                        lines.Add(vm);
                    }

                    foreach (var line in salesReturnLines)
                    {
                        var vm = new StockBalanceLineVM
                        {
                            Item = line.Item,
                            Date = line.SalesReturnTransaction.Date,
                            Documentation = line.SalesReturnTransaction.SalesReturnTransactionID,
                            Description = "Sales Return",
                            CustomerSupplier = line.SalesReturnTransaction.SalesTransaction.Customer.Name,
                            Amount = +line.Quantity,
                        };
                        lines.Add(vm);
                    }

                    foreach (var line in stockAdjustmentLines)
                    {
                        var vm = new StockBalanceLineVM
                        {
                            Item = line.Item,
                            Date = line.AdjustStockTransaction.Date,
                            Documentation = line.AdjustStockTransaction.AdjustStrockTransactionID,
                            Description = "Stock Adjustment",
                            CustomerSupplier = "",
                            Amount = line.Quantity,
                        };

                        lines.Add(vm);
                    }

                    foreach (var transaction in moveStockTransactions)
                    {
                        foreach (var line in transaction.MoveStockTransactionLines)
                        {
                            if (line.ItemID.Equals(_selectedProduct.ID))
                            {
                                if (transaction.FromWarehouse.ID.Equals(warehouse.ID))
                                {
                                    var vm = new StockBalanceLineVM
                                    {
                                        Item = line.Item,
                                        Date = line.MoveStockTransaction.Date,
                                        Documentation = transaction.MoveStrockTransactionID,
                                        Description = transaction.FromWarehouse.Name + " => " + transaction.ToWarehouse.Name,
                                        CustomerSupplier = "",
                                        Amount = -line.Quantity,
                                    };

                                    lines.Add(vm);
                                }

                                else if (transaction.ToWarehouse.ID.Equals(warehouse.ID))
                                {
                                    var vm = new StockBalanceLineVM
                                    {
                                        Item = line.Item,
                                        Date = line.MoveStockTransaction.Date,
                                        Documentation = transaction.MoveStrockTransactionID,
                                        Description = transaction.FromWarehouse.Name + " => " + transaction.ToWarehouse.Name,
                                        CustomerSupplier = "",
                                        Amount = line.Quantity,
                                    };

                                    lines.Add(vm);
                                }
                            }
                        }
                    }
                }

                var balance = _beginningBalance;
                var totalIn = 0;
                var totalOut = 0;
                foreach (var l in lines.OrderBy(e => e.Date))
                {
                    balance += l.Amount;
                    l.Balance = balance;
                    _lines.Add(l);

                    if (l.Amount < 0) totalOut += -l.Amount;
                    else totalIn += l.Amount;
                }

                EndingBalanceString = (balance / _selectedProduct.PiecesPerUnit) + "/" + (balance % _selectedProduct.PiecesPerUnit);
                TotalInString = (totalIn / _selectedProduct.PiecesPerUnit) + "/" + (totalIn % _selectedProduct.PiecesPerUnit);
                TotalOutString = (totalOut / _selectedProduct.PiecesPerUnit) + "/" + (totalOut % _selectedProduct.PiecesPerUnit);
            }
        }

        private void SetBeginningBalance()
        {
            var year = _fromDate.Year;
            var month = _fromDate.Month;

            _beginningBalance = GetBeginningBalance(_fromDate);
            BeginningBalanceString = (_beginningBalance / _selectedProduct.PiecesPerUnit) + "/" + (_beginningBalance % _selectedProduct.PiecesPerUnit);
        }

        private int GetPeriodBeginningBalance(int year, int month)
        {
            var beginningBalance = 0;

            using (var context = new ERPContext())
            {
                foreach (var warehouse in _warehouses)
                {
                    if (warehouse.IsSelected)
                    {

                        var stockBalance = context.StockBalances.Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) && e.Year == year).FirstOrDefault();

                        if (stockBalance == null)
                        {
                            continue;
                        }

                        switch (month)
                        {
                            case 1:
                                beginningBalance += stockBalance.BeginningBalance;
                                break;
                            case 2:
                                beginningBalance += stockBalance.Balance1;
                                break;
                            case 3:
                                beginningBalance += stockBalance.Balance2;
                                break;
                            case 4:
                                beginningBalance += stockBalance.Balance3;
                                break;
                            case 5:
                                beginningBalance += stockBalance.Balance4;
                                break;
                            case 6:
                                beginningBalance += stockBalance.Balance5;
                                break;
                            case 7:
                                beginningBalance += stockBalance.Balance6;
                                break;
                            case 8:
                                beginningBalance += stockBalance.Balance7;
                                break;
                            case 9:
                                beginningBalance += stockBalance.Balance8;
                                break;
                            case 10:
                                beginningBalance += stockBalance.Balance9;
                                break;
                            case 11:
                                beginningBalance += stockBalance.Balance10;
                                break;
                            case 12:
                                beginningBalance += stockBalance.Balance11;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return beginningBalance;
        }

        private int GetBeginningBalance(DateTime fromDate)
        {
            var monthDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var balance = GetPeriodBeginningBalance(fromDate.Year, fromDate.Month);

            using (var context = new ERPContext())
            {
                foreach (var warehouse in _warehouses)
                {
                    if (warehouse.IsSelected)
                    {
                        var purchaseLines = context.PurchaseTransactionLines
                            .Include("PurchaseTransaction")
                            .Include("PurchaseTransaction.Supplier")
                            .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.PurchaseTransaction.Date >= monthDate && e.PurchaseTransaction.Date < fromDate &&
                            !e.PurchaseTransaction.Supplier.Name.Equals("-") && !e.PurchaseTransactionID.Substring(0, 2).Equals("SA"))
                            .ToList();

                        var purchaseReturnLines = context.PurchaseReturnTransactionLines
                            .Include("PurchaseReturnTransaction")
                            .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                            .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.PurchaseReturnTransaction.Date >= monthDate && e.PurchaseReturnTransaction.Date < fromDate)
                            .ToList();

                        var salesLines = context.SalesTransactionLines
                            .Include("SalesTransaction")
                            .Include("SalesTransaction.Customer")
                            .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.SalesTransaction.When >= monthDate && e.SalesTransaction.When < fromDate)
                            .ToList();

                        var salesReturnLines = context.SalesReturnTransactionLines
                            .Include("SalesReturnTransaction")
                            .Include("SalesReturnTransaction.SalesTransaction.Customer")
                            .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.SalesReturnTransaction.Date >= monthDate && e.SalesReturnTransaction.Date < fromDate)
                            .ToList();

                        var stockAdjustmentLines = context.AdjustStockTransactionLines
                            .Include("AdjustStockTransaction")
                            .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.AdjustStockTransaction.Date >= monthDate && e.AdjustStockTransaction.Date < fromDate)
                            .ToList();

                        foreach (var line in purchaseLines)
                            balance += line.Quantity;

                        foreach (var line in purchaseReturnLines)
                            balance -= line.Quantity;

                        foreach (var line in salesLines)
                            balance -= line.Quantity;

                        foreach (var line in salesReturnLines)
                            balance += line.Quantity;

                        foreach (var line in stockAdjustmentLines)
                            balance += line.Quantity;
                    }
                }
            }

            return balance;
        }
        #endregion
    }
}
