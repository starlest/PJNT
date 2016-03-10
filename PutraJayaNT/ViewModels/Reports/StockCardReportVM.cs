namespace PutraJayaNT.ViewModels.Reports
{
    using MVVMFramework;
    using PutraJayaNT.Reports.Windows;
    using Utilities;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Inventory;
    using Models.Inventory;
    using Utilities.Database.Item;
    using Utilities.Database.Purchase;
    using Utilities.Database.Sales;
    using Utilities.Database.Stock;
    using Utilities.Database.Warehouse;

    public class StockCardReportVM : ViewModelBase
    {
        private ItemVM _selectedProduct;
        private DateTime _fromDate;
        private DateTime _toDate;
        private string _productUnit;
        private string _beginningBalanceString;
        private string _endingBalanceString;
        private string _totalInString;
        private string _totalOutString;

        private ICommand _displayCommand;
        private ICommand _printCommand;

        private int _beginningBalance;

        public StockCardReportVM()
        {
            Products = new ObservableCollection<ItemVM>();
            Warehouses = new ObservableCollection<WarehouseVM>();
            DisplayedLines = new ObservableCollection<StockCardLineVM>();
            _fromDate = UtilityMethods.GetCurrentDate().Date.AddDays(-UtilityMethods.GetCurrentDate().Day + 1);
            _toDate = UtilityMethods.GetCurrentDate().Date;
            UpdateProducts();
            UpdateWarehouses();
        }

        #region Collections
        public ObservableCollection<ItemVM> Products { get; }

        public ObservableCollection<WarehouseVM> Warehouses { get; }

        public ObservableCollection<StockCardLineVM> DisplayedLines { get; }
        #endregion

        #region Properties
        public ItemVM SelectedProduct
        {
            get { return _selectedProduct; }
            set { SetProperty(ref _selectedProduct, value, "SelectedProduct"); }
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
        #endregion

        #region Commands
        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (DisplayedLines.Count == 0) return;
                    var stockCardReportWindow = new StockCardReportWindow(this)
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    stockCardReportWindow.Show();
                }));
            }
        }

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (_selectedProduct == null) return;
                    ProductUnit = _selectedProduct.UnitName + "/" + _selectedProduct.PiecesPerUnit;
                    SetBeginningBalance();
                    UpdateDisplayedLines();
                    UpdateProducts();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateProducts()
        {
            var oldSelectedProduct = _selectedProduct;

            Products.Clear();
            var productsFromDatabase = DatabaseItemHelper.GetAll();
            foreach (var product in productsFromDatabase)
                Products.Add(new ItemVM { Model = product });

            UpdateSelectedProduct(oldSelectedProduct);
        }

        private void UpdateSelectedProduct(ItemVM oldSelectedProduct)
        {
            if (oldSelectedProduct == null) return;
            SelectedProduct = Products.FirstOrDefault(product => product.ID.Equals(oldSelectedProduct.ID));
        }

        private void UpdateWarehouses()
        {
            Warehouses.Clear();
            var warehousesFromDatabase = DatabaseWarehouseHelper.GetAll();
            foreach (var warehouse in warehousesFromDatabase)
                Warehouses.Add(new WarehouseVM { Model = warehouse });
        }

        private void SetBeginningBalance()
        {
            _beginningBalance = GetBeginningBalance(_fromDate);
            BeginningBalanceString = _beginningBalance / _selectedProduct.PiecesPerUnit + "/" + _beginningBalance % _selectedProduct.PiecesPerUnit;
        }

        private int GetBeginningBalance(DateTime fromDate)
        {
            var balance = GetPeriodBeginningBalance(fromDate.Year, fromDate.Month);

            using (var context = new ERPContext())
            {
                foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
                {

                    balance += SumOfPurchaseTransactionLinesValueFromBeginningPeriodToSelectedDate(context, warehouse.Model, fromDate);
                    balance -= SumOfPurchaseReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(context, warehouse.Model, fromDate);
                    balance -= SumOfSalesTransactionLinesValueFromBeginningPeriodToSelectedDate(context, warehouse.Model, fromDate);
                    balance += SumOfSalesReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(context, warehouse.Model,
                        fromDate);
                    balance += SumOfStockAdjustmentTransactionLinesValueFromBeginningPeriodToSelectedDate(context, warehouse.Model,
                        fromDate);
                }
            }

            return balance;
        }

        private int SumOfPurchaseTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context, Warehouse warehouse, DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var purchaseTransactionLinesFromDatabase = context.PurchaseTransactionLines.Where(
                line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                        && line.PurchaseTransaction.Date >= beginningPeriodDate &&
                        line.PurchaseTransaction.Date < fromDate
                        && !line.PurchaseTransactionID.Substring(0, 2).Equals("SA"))
                        .ToList();
            return purchaseTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfPurchaseReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context, Warehouse warehouse, DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var purchaseReturnTransactionLinesFromDatabase = context.PurchaseReturnTransactionLines.Where(
                line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID) 
                && line.PurchaseReturnTransaction.Date >= beginningPeriodDate 
                && line.PurchaseReturnTransaction.Date < fromDate)
                .ToList();
            return purchaseReturnTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfSalesTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context, Warehouse warehouse, DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var salesTransactionLinesFromDatabase = context.SalesTransactionLines.Where(
                line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                        && line.SalesTransaction.Date >= beginningPeriodDate && line.SalesTransaction.Date < fromDate)
                        .ToList();
            return salesTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfSalesReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context, Warehouse warehouse, DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var salesReturnTransactionLineFromDatabase = context.SalesReturnTransactionLines.Where(
                line => 
                line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID) 
                && line.SalesReturnTransaction.Date >= beginningPeriodDate 
                && line.SalesReturnTransaction.Date < fromDate)
                .ToList();
            return salesReturnTransactionLineFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfStockAdjustmentTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context, Warehouse warehouse,
            DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var stockAdjustmentTransactionLinesFromDatabase = context.AdjustStockTransactionLines.Where(
                line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID) 
                && line.AdjustStockTransaction.Date >= beginningPeriodDate 
                && line.AdjustStockTransaction.Date < fromDate)
                .ToList();
            return stockAdjustmentTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int GetPeriodBeginningBalance(int year, int month)
        {
            var beginningBalance = 0;
            using (var context = new ERPContext())
            {
                foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
                {
                    var stockBalanceFromDatabase = context.StockBalances.FirstOrDefault(
                        stockBalance => 
                        stockBalance.ItemID.Equals(_selectedProduct.ID) && stockBalance.WarehouseID.Equals(warehouse.ID) 
                        && stockBalance.Year == year);
                    if (stockBalanceFromDatabase == null) continue;
                    beginningBalance += GetStockBalancePeriodBeginningBalance(stockBalanceFromDatabase, month);
                }
            }
            return beginningBalance;
        }

        private static int GetStockBalancePeriodBeginningBalance(StockBalance stockBalanceFromDatabase, int month)
        {
            switch (month)
            {
                case 1:
                    return stockBalanceFromDatabase.BeginningBalance;
                case 2:
                    return stockBalanceFromDatabase.Balance1;
                case 3:
                    return stockBalanceFromDatabase.Balance2;
                case 4:
                    return stockBalanceFromDatabase.Balance3;
                case 5:
                    return stockBalanceFromDatabase.Balance4;
                case 6:
                    return stockBalanceFromDatabase.Balance5;
                case 7:
                    return stockBalanceFromDatabase.Balance6;
                case 8:
                    return stockBalanceFromDatabase.Balance7;
                case 9:
                    return stockBalanceFromDatabase.Balance8;
                case 10:
                    return stockBalanceFromDatabase.Balance9;
                case 11:
                    return stockBalanceFromDatabase.Balance10;
                case 12:
                    return stockBalanceFromDatabase.Balance11;
                default:
                    return 0;
            }
        }

        private void UpdateDisplayedLines()
        {
            DisplayedLines.Clear();
            LoadPeriodPurchaseTransactionLinesIntoDisplayedLines();
            LoadPeriodPurchaseReturnTransactionLinesIntoDisplayedLines();
            LoadPeriodSalesTransactionLinesIntoDisplayedLines();
            LoadPeriodSalesReturnTransactionLinesIntoDisplayedLines();
            LoadPeriodStockAdjustmentTransactionLinesIntoDisplayedLines();
            LoadPeriodStockMovementTransactionLinesIntoDisplayedLines();
            SortDisplayedLinesAccordingToDate();            
        }

        private void LoadPeriodPurchaseTransactionLinesIntoDisplayedLines()
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var purchaseLinesFromDatabase =
                    DatabasePurchaseTransactionLineHelper.Get(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID) &&
                        line.PurchaseTransaction.Date >= _fromDate && line.PurchaseTransaction.Date <= _toDate &&
                        !line.PurchaseTransactionID.Substring(0, 2).Equals("SA"));

                foreach (var purchaseLineVM in purchaseLinesFromDatabase.Select(purchaseLine => new StockCardLineVM
                {
                    Item = purchaseLine.Item,
                    Date = purchaseLine.PurchaseTransaction.Date,
                    Documentation = purchaseLine.PurchaseTransaction.PurchaseID,
                    Description = "Purchase",
                    CustomerSupplier = purchaseLine.PurchaseTransaction.Supplier.Name,
                    Amount = purchaseLine.Quantity
                }))
                {
                    DisplayedLines.Add(purchaseLineVM);
                }
            }
        }

        private void LoadPeriodPurchaseReturnTransactionLinesIntoDisplayedLines()
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var purchaseReturnLinesFromDatabase =
                    DatabasePurchaseReturnTransactionLineHelper.Get(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                        && line.PurchaseReturnTransaction.Date >= _fromDate && line.PurchaseReturnTransaction.Date <= _toDate);

                foreach (var vm in purchaseReturnLinesFromDatabase.Select(purchaseReturnLine => new StockCardLineVM
                {
                    Item = purchaseReturnLine.Item,
                    Date = purchaseReturnLine.PurchaseReturnTransaction.Date,
                    Documentation = purchaseReturnLine.PurchaseReturnTransaction.PurchaseReturnTransactionID,
                    Description = "Purchase Return",
                    CustomerSupplier = purchaseReturnLine.PurchaseReturnTransaction.PurchaseTransaction.Supplier.Name,
                    Amount = -purchaseReturnLine.Quantity,
                }))
                {
                    DisplayedLines.Add(vm);
                }
            }
        }

        private void LoadPeriodSalesTransactionLinesIntoDisplayedLines()
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var salesLinesFromDatabase = DatabaseSalesTransactionLineHelper.Get(
                    line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                    && line.SalesTransaction.Date >= _fromDate && line.SalesTransaction.Date <= _toDate);

                foreach (var vm in salesLinesFromDatabase.Select(salesLine => new StockCardLineVM
                {
                    Item = salesLine.Item,
                    Date = salesLine.SalesTransaction.Date,
                    Documentation = salesLine.SalesTransaction.SalesTransactionID,
                    Description = "Sales",
                    CustomerSupplier = salesLine.SalesTransaction.Customer.Name,
                    Amount = -salesLine.Quantity,
                }))
                {
                    DisplayedLines.Add(vm);
                }
            }
        }

        private void LoadPeriodSalesReturnTransactionLinesIntoDisplayedLines()
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var salesReturnLinesFromDatabase =
                    DatabaseSalesReturnTransactionLineHelper.Get(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                        && line.SalesReturnTransaction.Date >= _fromDate &&
                        line.SalesReturnTransaction.Date <= _toDate);

                foreach (var salesReturnline in salesReturnLinesFromDatabase)
                {
                    var vm = new StockCardLineVM
                    {
                        Item = salesReturnline.Item,
                        Date = salesReturnline.SalesReturnTransaction.Date,
                        Documentation = salesReturnline.SalesReturnTransaction.SalesReturnTransactionID,
                        Description = "Sales Return",
                        CustomerSupplier = salesReturnline.SalesReturnTransaction.SalesTransaction.Customer.Name,
                        Amount = +salesReturnline.Quantity,
                    };
                    DisplayedLines.Add(vm);
                }
            }
        }

        private void LoadPeriodStockAdjustmentTransactionLinesIntoDisplayedLines()
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var stockAdjustmentLinesFromDatabase =
                    DatabaseStockAdjustmentTransactionLineHelper.Get(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                                && line.AdjustStockTransaction.Date >= _fromDate &&
                                line.AdjustStockTransaction.Date <= _toDate);

                foreach (var vm in stockAdjustmentLinesFromDatabase.Select(line => new StockCardLineVM
                {
                    Item = line.Item,
                    Date = line.AdjustStockTransaction.Date,
                    Documentation = line.AdjustStockTransaction.AdjustStrockTransactionID,
                    Description = "Stock Adjustment",
                    CustomerSupplier = line.AdjustStockTransaction.User.Username,
                    Amount = line.Quantity,
                }))
                {
                    DisplayedLines.Add(vm);
                }
            }
        }

        private void LoadPeriodStockMovementTransactionLinesIntoDisplayedLines()
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var moveStockTransactionsFromDatabase =
                    DatabaseStockMovementTransactionHelper.Get(
                        e => e.Date >= _fromDate && e.Date <= _toDate
                        &&  (e.FromWarehouse.ID.Equals(warehouse.ID) 
                        || e.ToWarehouse.ID.Equals(warehouse.ID)));

                foreach (var transaction in moveStockTransactionsFromDatabase)
                {
                    foreach (var line in transaction.MoveStockTransactionLines)
                    {
                        if (!line.ItemID.Equals(_selectedProduct.ID)) continue;

                        if (transaction.FromWarehouse.ID.Equals(warehouse.ID))
                        {
                            var vm = new StockCardLineVM
                            {
                                Item = line.Item,
                                Date = line.MoveStockTransaction.Date,
                                Documentation = transaction.MoveStrockTransactionID,
                                Description = transaction.FromWarehouse.Name + " => " + transaction.ToWarehouse.Name,
                                CustomerSupplier = transaction.User.Username,
                                Amount = -line.Quantity,
                            };

                            DisplayedLines.Add(vm);
                        }

                        else if (transaction.ToWarehouse.ID.Equals(warehouse.ID))
                        {
                            var vm = new StockCardLineVM
                            {
                                Item = line.Item,
                                Date = line.MoveStockTransaction.Date,
                                Documentation = transaction.MoveStrockTransactionID,
                                Description = transaction.FromWarehouse.Name + " => " + transaction.ToWarehouse.Name,
                                CustomerSupplier = transaction.User.Username,
                                Amount = line.Quantity,
                            };

                            DisplayedLines.Add(vm);
                        }
                    }
                }
            }
        }

        private void SortDisplayedLinesAccordingToDate()
        {
            var sortedlines = DisplayedLines.OrderBy(line => line.Date).ToList();
            var balance = _beginningBalance;
            var totalIn = 0;
            var totalOut = 0;
            foreach (var l in sortedlines)
            {
                balance += l.Amount;
                l.Balance = balance;
                DisplayedLines.Add(l);

                if (l.Amount < 0) totalOut += -l.Amount;
                else totalIn += l.Amount;
            }

            EndingBalanceString = balance / _selectedProduct.PiecesPerUnit + "/" + balance % _selectedProduct.PiecesPerUnit;
            TotalInString = totalIn / _selectedProduct.PiecesPerUnit + "/" + totalIn % _selectedProduct.PiecesPerUnit;
            TotalOutString = totalOut / _selectedProduct.PiecesPerUnit + "/" + totalOut % _selectedProduct.PiecesPerUnit;
        }
        #endregion
    }
}
