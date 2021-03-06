﻿namespace ECERP.ViewModels.Reports
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using ECERP.Reports.Windows;
    using Item;
    using Models.Inventory;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;

    public class StockCardReportVM : ViewModelBase
    {
        private ItemVM _selectedProduct;
        private DateTime _fromDate;
        private DateTime _toDate;
        private string _productUnitName;
        private string _productQuantityPerUnit;
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
            set { SetProperty(ref _selectedProduct, value, () => SelectedProduct); }
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

        public string BeginningBalanceString
        {
            get { return _beginningBalanceString; }
            set { SetProperty(ref _beginningBalanceString, value, () => BeginningBalanceString); }
        }

        public string EndingBalanceString
        {
            get { return _endingBalanceString; }
            set { SetProperty(ref _endingBalanceString, value, () => EndingBalanceString); }
        }

        public string TotalInString
        {
            get { return _totalInString; }
            set { SetProperty(ref _totalInString, value, () => TotalInString); }
        }

        public string TotalOutString
        {
            get { return _totalOutString; }
            set { SetProperty(ref _totalOutString, value, () => TotalOutString); }
        }

        public string ProductUnitName
        {
            get { return _productUnitName; }
            set { SetProperty(ref _productUnitName, value, () => ProductUnitName); }
        }

        public string ProductQuantityPerUnit
        {
            get { return _productQuantityPerUnit; }
            set { SetProperty(ref _productQuantityPerUnit, value, () => ProductQuantityPerUnit); }
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
                           ProductUnitName = InventoryHelper.GetItemUnitName(_selectedProduct.Model);
                           ProductQuantityPerUnit = InventoryHelper.GetItemQuantityPerUnit(_selectedProduct.Model);
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
            using (var context = UtilityMethods.createContext())
            {
                var productsFromDatabase = context.Inventory.OrderBy(item => item.Name);
                foreach (var product in productsFromDatabase)
                    Products.Add(new ItemVM { Model = product });
            }

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
            using (var context = UtilityMethods.createContext())
            {
                var warehousesFromDatabase = context.Warehouses.OrderBy(warehouse => warehouse.Name);
                foreach (var warehouse in warehousesFromDatabase)
                    Warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void SetBeginningBalance()
        {
            _beginningBalance = GetBeginningBalance(_fromDate);
            BeginningBalanceString = InventoryHelper.ConvertItemQuantityTostring(_selectedProduct.Model,
                _beginningBalance);
        }

        private int GetBeginningBalance(DateTime fromDate)
        {
            var balance = GetPeriodBeginningBalance(fromDate.Year, fromDate.Month);

            using (var context = UtilityMethods.createContext())
            {
                foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
                {
                    balance += SumOfPurchaseTransactionLinesValueFromBeginningPeriodToSelectedDate(context,
                        warehouse.Model, fromDate);
                    balance -= SumOfPurchaseReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(context,
                        warehouse.Model, fromDate);
                    balance -= SumOfSalesTransactionLinesValueFromBeginningPeriodToSelectedDate(context, warehouse.Model,
                        fromDate);
                    balance += SumOfSalesReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(context,
                        warehouse.Model,
                        fromDate);
                    balance += SumOfStockAdjustmentTransactionLinesValueFromBeginningPeriodToSelectedDate(context,
                        warehouse.Model,
                        fromDate);
                    balance += SumOfStockMovementTransactionLinesValueFromBeginningPeriodToSelectedDate(context,
                        warehouse.Model,
                        fromDate);
                }
            }

            return balance;
        }

        private int SumOfPurchaseTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context,
            Warehouse warehouse, DateTime fromDate)
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

        private int SumOfPurchaseReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context,
            Warehouse warehouse, DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var purchaseReturnTransactionLinesFromDatabase = context.PurchaseReturnTransactionLines.Where(
                    line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                            && line.PurchaseReturnTransaction.Date >= beginningPeriodDate
                            && line.PurchaseReturnTransaction.Date < fromDate)
                .ToList();
            return purchaseReturnTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfSalesTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context,
            Warehouse warehouse, DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var salesTransactionLinesFromDatabase = context.SalesTransactionLines.Where(
                    line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                            && line.SalesTransaction.Date >= beginningPeriodDate &&
                            line.SalesTransaction.Date < fromDate)
                .ToList();
            return salesTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfSalesReturnTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context,
            Warehouse warehouse, DateTime fromDate)
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

        private int SumOfStockAdjustmentTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context,
            Warehouse warehouse,
            DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var stockAdjustmentTransactionLinesFromDatabase = context.StockAdjustmentTransactionLines.Where(
                    line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                            && line.StockAdjustmentTransaction.Date >= beginningPeriodDate
                            && line.StockAdjustmentTransaction.Date < fromDate)
                .ToList();
            return stockAdjustmentTransactionLinesFromDatabase.Sum(line => line.Quantity);
        }

        private int SumOfStockMovementTransactionLinesValueFromBeginningPeriodToSelectedDate(ERPContext context,
            Warehouse warehouse,
            DateTime fromDate)
        {
            var beginningPeriodDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var stockMovementFromTransactionLinesFromDatabase = context.StockMovementTransactionLines.Where(
                    line =>
                        line.ItemID.Equals(_selectedProduct.ID) &&
                        line.StockMovementTransaction.FromWarehouse.ID.Equals(warehouse.ID)
                        && line.StockMovementTransaction.Date >= beginningPeriodDate
                        && line.StockMovementTransaction.Date < fromDate)
                .ToList();
            var stockMovementToTransactionLinesFromDatabase = context.StockMovementTransactionLines.Where(
                    line =>
                        line.ItemID.Equals(_selectedProduct.ID) &&
                        line.StockMovementTransaction.ToWarehouse.ID.Equals(warehouse.ID)
                        && line.StockMovementTransaction.Date >= beginningPeriodDate
                        && line.StockMovementTransaction.Date < fromDate)
                .ToList();
            var quantity = stockMovementToTransactionLinesFromDatabase.Sum(line => line.Quantity);
            return stockMovementFromTransactionLinesFromDatabase.Aggregate(quantity,
                (current, line) => current - line.Quantity);
        }

        private int GetPeriodBeginningBalance(int year, int month)
        {
            var beginningBalance = 0;
            using (var context = UtilityMethods.createContext())
            {
                foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
                {
                    var stockBalanceFromDatabase = context.StockBalances.FirstOrDefault(
                        stockBalance =>
                            stockBalance.ItemID.Equals(_selectedProduct.ID) &&
                            stockBalance.WarehouseID.Equals(warehouse.ID)
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
            using (var context = UtilityMethods.createContext())
            {
                LoadPeriodPurchaseTransactionLinesIntoDisplayedLines(context);
                LoadPeriodPurchaseReturnTransactionLinesIntoDisplayedLines(context);
                LoadPeriodSalesTransactionLinesIntoDisplayedLines(context);
                LoadPeriodSalesReturnTransactionLinesIntoDisplayedLines(context);
                LoadPeriodStockAdjustmentTransactionLinesIntoDisplayedLines(context);
                LoadPeriodStockMovementTransactionLinesIntoDisplayedLines(context);
            }
            SortDisplayedLinesAccordingToDate();
        }

        private void LoadPeriodPurchaseTransactionLinesIntoDisplayedLines(ERPContext context)
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var purchaseLinesFromDatabase =
                    context.PurchaseTransactionLines.Where(
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

        private void LoadPeriodPurchaseReturnTransactionLinesIntoDisplayedLines(ERPContext context)
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var purchaseReturnLinesFromDatabase =
                    context.PurchaseReturnTransactionLines.Where(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                                && line.PurchaseReturnTransaction.Date >= _fromDate &&
                                line.PurchaseReturnTransaction.Date <= _toDate);

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

        private void LoadPeriodSalesTransactionLinesIntoDisplayedLines(ERPContext context)
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var salesLinesFromDatabase =
                    context.SalesTransactionLines.Where(
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

        private void LoadPeriodSalesReturnTransactionLinesIntoDisplayedLines(ERPContext context)
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var salesReturnLinesFromDatabase =
                    context.SalesReturnTransactionLines.Where(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                                && line.SalesReturnTransaction.Date >= _fromDate &&
                                line.SalesReturnTransaction.Date <= _toDate);

                foreach (var vm in salesReturnLinesFromDatabase.Select(salesReturnLine => new StockCardLineVM
                {
                    Item = salesReturnLine.Item,
                    Date = salesReturnLine.SalesReturnTransaction.Date,
                    Documentation = salesReturnLine.SalesReturnTransaction.SalesReturnTransactionID,
                    Description = "Sales Return",
                    CustomerSupplier = salesReturnLine.SalesReturnTransaction.SalesTransaction.Customer.Name,
                    Amount = salesReturnLine.Quantity,
                }))
                {
                    DisplayedLines.Add(vm);
                }
            }
        }

        private void LoadPeriodStockAdjustmentTransactionLinesIntoDisplayedLines(ERPContext context)
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var stockAdjustmentLinesFromDatabase =
                    context.StockAdjustmentTransactionLines.Where(
                        line => line.ItemID.Equals(_selectedProduct.ID) && line.WarehouseID.Equals(warehouse.ID)
                                && line.StockAdjustmentTransaction.Date >= _fromDate &&
                                line.StockAdjustmentTransaction.Date <= _toDate);

                foreach (var vm in stockAdjustmentLinesFromDatabase.Select(line => new StockCardLineVM
                {
                    Item = line.Item,
                    Date = line.StockAdjustmentTransaction.Date,
                    Documentation = line.StockAdjustmentTransaction.StockAdjustmentTransactionID,
                    Description = "Stock Adjustment",
                    CustomerSupplier = line.StockAdjustmentTransaction.User.Username,
                    Amount = line.Quantity,
                }))
                {
                    DisplayedLines.Add(vm);
                }
            }
        }

        private void LoadPeriodStockMovementTransactionLinesIntoDisplayedLines(ERPContext context)
        {
            foreach (var warehouse in Warehouses.Where(warehouse => warehouse.IsSelected))
            {
                var moveStockTransactionsFromDatabase =
                    context.StockMovementTransactions
                        .Where(
                            transaction =>
                                transaction.Date >= _fromDate && transaction.Date <= _toDate
                                && (transaction.FromWarehouse.ID.Equals(warehouse.ID)
                                    || transaction.ToWarehouse.ID.Equals(warehouse.ID)))
                        .ToList();

                foreach (var transaction in moveStockTransactionsFromDatabase)
                {
                    foreach (var line in transaction.StockMovementTransactionLines)
                    {
                        if (!line.ItemID.Equals(_selectedProduct.ID)) continue;

                        if (transaction.FromWarehouse.ID.Equals(warehouse.ID))
                        {
                            var vm = new StockCardLineVM
                            {
                                Item = line.Item,
                                Date = line.StockMovementTransaction.Date,
                                Documentation = transaction.StockMovementTransactionID,
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
                                Date = line.StockMovementTransaction.Date,
                                Documentation = transaction.StockMovementTransactionID,
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
            DisplayedLines.Clear();
            foreach (var l in sortedlines)
            {
                balance += l.Amount;
                l.Balance = balance;
                DisplayedLines.Add(l);

                if (l.Amount < 0) totalOut += -l.Amount;
                else totalIn += l.Amount;
            }

            EndingBalanceString = InventoryHelper.ConvertItemQuantityTostring(_selectedProduct.Model, balance);
            TotalInString = InventoryHelper.ConvertItemQuantityTostring(_selectedProduct.Model, totalIn);
            TotalOutString = InventoryHelper.ConvertItemQuantityTostring(_selectedProduct.Model, totalOut);
        }

        #endregion
    }
}