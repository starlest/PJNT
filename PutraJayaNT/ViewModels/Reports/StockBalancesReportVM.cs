using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels.Reports
{
    class StockBalancesReportVM : ViewModelBase
    {
        ObservableCollection<ItemVM> _products;
        ObservableCollection<StockBalanceLineVM> _lines;

        ItemVM _selectedProduct;
        DateTime _fromDate;
        DateTime _toDate;
        string _beginningBalanceString;
        string _productUnitName;
        int? _productPiecesPerUnit;

        int _beginningBalance = 0;

        public StockBalancesReportVM()
        {
            _products = new ObservableCollection<ItemVM>();
            _lines = new ObservableCollection<StockBalanceLineVM>();
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;
            UpdateProducts();
        }

        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
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

                ProductUnitName = _selectedProduct.UnitName;
                ProductPiecesPerUnit = _selectedProduct.PiecesPerUnit;
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

        public string ProductUnitName
        {
            get { return _productUnitName; }
            set { SetProperty(ref _productUnitName, value, "ProductUnitName"); }
        }

        public int? ProductPiecesPerUnit
        {
            get { return _productPiecesPerUnit; }
            set { SetProperty(ref _productPiecesPerUnit, value, "ProductPiecesPerUnit"); }
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

        private void UpdateLines()
        {
            _lines.Clear();
            var balance = _beginningBalance;
            using (var context = new ERPContext())
            {
                var purchaseLines = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Include("PurchaseTransaction.Supplier")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.PurchaseTransaction.Date >= _fromDate && e.PurchaseTransaction.Date <= _toDate)
                    .ToList();

                var purchaseReturnLines = context.PurchaseReturnTransactionLines
                    .Include("PurchaseReturnTransaction")
                    .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.PurchaseReturnTransaction.Date >= _fromDate && e.PurchaseReturnTransaction.Date <= _toDate)
                    .ToList();

                var salesLines = context.SalesTransactionLines
                    .Include("SalesTransaction")
                    .Include("SalesTransaction.Customer")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.SalesTransaction.When >= _fromDate && e.SalesTransaction.When <= _toDate)
                    .ToList();

                var salesReturnLines = context.SalesReturnTransactionLines
                    .Include("SalesReturnTransaction")
                    .Include("SalesReturnTransaction.SalesTransaction.Customer")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.SalesReturnTransaction.Date >= _fromDate && e.SalesReturnTransaction.Date <= _toDate)
                    .ToList();

                foreach (var line in purchaseLines)
                {
                    balance += line.Quantity;
                    var vm = new StockBalanceLineVM
                    {
                        Item = line.Item,
                        Date = line.PurchaseTransaction.Date,
                        Documentation = line.PurchaseTransaction.PurchaseID,
                        Description = "Purchase",
                        CustomerSupplier = line.PurchaseTransaction.Supplier.Name,
                        Amount = line.Quantity,
                        Balance = balance
                    };
                    _lines.Add(vm);
                }

                foreach (var line in purchaseReturnLines)
                {
                    balance -= line.Quantity;
                    var vm = new StockBalanceLineVM
                    {
                        Item = line.Item,
                        Date = line.PurchaseReturnTransaction.Date,
                        Documentation = line.PurchaseReturnTransaction.PurchaseReturnTransactionID,
                        Description = "Purchase Return",
                        CustomerSupplier = line.PurchaseReturnTransaction.PurchaseTransaction.Supplier.Name,
                        Amount = -line.Quantity,
                        Balance = balance
                    };
                    _lines.Add(vm);
                }

                foreach (var line in salesLines)
                {
                    balance -= line.Quantity;
                    var vm = new StockBalanceLineVM
                    {
                        Item = line.Item,
                        Date = line.SalesTransaction.When,
                        Documentation = line.SalesTransaction.SalesTransactionID,
                        Description = "Sales",
                        CustomerSupplier = line.SalesTransaction.Customer.Name,
                        Amount = -line.Quantity,
                        Balance = balance
                    };
                    _lines.Add(vm);
                }

                foreach (var line in salesReturnLines)
                {
                    balance += line.Quantity;
                    var vm = new StockBalanceLineVM
                    {
                        Item = line.Item,
                        Date = line.SalesReturnTransaction.Date,
                        Documentation = line.SalesReturnTransaction.SalesReturnTransactionID,
                        Description = "Sales Return",
                        CustomerSupplier = line.SalesReturnTransaction.SalesTransaction.Customer.Name,
                        Amount = +line.Quantity,
                        Balance = balance
                    };
                    _lines.Add(vm);
                }
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
                var stockBalance = context.StockBalances.Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.Year == year).FirstOrDefault();

                if (stockBalance == null)
                {
                    MessageBox.Show("Invalid Date Selected", "Invalid", MessageBoxButton.OK);
                    _lines.Clear();
                    return beginningBalance;
                }

                switch (month)
                {
                    case 1:
                        beginningBalance = stockBalance.BeginningBalance;
                        break;
                    case 2:
                        beginningBalance = stockBalance.Balance1;
                        break;
                    case 3:
                        beginningBalance = stockBalance.Balance2;
                        break;
                    case 4:
                        beginningBalance = stockBalance.Balance3;
                        break;
                    case 5:
                        beginningBalance = stockBalance.Balance4;
                        break;
                    case 6:
                        beginningBalance = stockBalance.Balance5;
                        break;
                    case 7:
                        beginningBalance = stockBalance.Balance6;
                        break;
                    case 8:
                        beginningBalance = stockBalance.Balance7;
                        break;
                    case 9:
                        beginningBalance = stockBalance.Balance8;
                        break;
                    case 10:
                        beginningBalance = stockBalance.Balance9;
                        break;
                    case 11:
                        beginningBalance = stockBalance.Balance10;
                        break;
                    case 12:
                        beginningBalance = stockBalance.Balance11;
                        break;
                    default:
                        break;
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
                var purchaseLines = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Include("PurchaseTransaction.Supplier")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.PurchaseTransaction.Date >= monthDate && e.PurchaseTransaction.Date < fromDate)
                    .ToList();

                var purchaseReturnLines = context.PurchaseReturnTransactionLines
                    .Include("PurchaseReturnTransaction")
                    .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.PurchaseReturnTransaction.Date >= monthDate && e.PurchaseReturnTransaction.Date < fromDate)
                    .ToList();

                var salesLines = context.SalesTransactionLines
                    .Include("SalesTransaction")
                    .Include("SalesTransaction.Customer")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.SalesTransaction.When >= monthDate && e.SalesTransaction.When < fromDate)
                    .ToList();

                var salesReturnLines = context.SalesReturnTransactionLines
                    .Include("SalesReturnTransaction")
                    .Include("SalesReturnTransaction.SalesTransaction.Customer")
                    .Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.SalesReturnTransaction.Date >= monthDate && e.SalesReturnTransaction.Date < fromDate)
                    .ToList();

                foreach (var line in purchaseLines)
                    balance += line.Quantity;

                foreach (var line in purchaseReturnLines)
                    balance -= line.Quantity;

                foreach (var line in salesLines)
                    balance -= line.Quantity;

                foreach (var line in salesReturnLines)
                    balance += line.Quantity;
            }

            return balance;
        }
        #endregion
    }
}
