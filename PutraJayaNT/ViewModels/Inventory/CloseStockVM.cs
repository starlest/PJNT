namespace ECRP.ViewModels.Inventory
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using Models.Inventory;
    using MVVMFramework;
    using Utilities;

    internal class CloseStockVM : ViewModelBase
    {
        private readonly List<Warehouse> _warehouses;
        private int _periodYear;
        private int _periodMonth;
        private readonly DateTime currentPeriod;

        public CloseStockVM()
        {
            using (var context = UtilityMethods.createContext())
            {
                PeriodYears = new ObservableCollection<int> { DateTime.Now.Year - 1, DateTime.Now.Year, DateTime.Now.Year + 1 };

                var firstOrDefault = context.Ledger_General.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    _periodYear = firstOrDefault.PeriodYear;
                    _periodMonth = firstOrDefault.Period;
                    currentPeriod = new DateTime(_periodYear, _periodMonth, 1);
                }

                Periods = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                _periodMonth = UtilityMethods.GetCurrentDate().Month;
                _warehouses = context.Warehouses.ToList();
            }
        }

        public ObservableCollection<int> PeriodYears { get; }

        public ObservableCollection<int> Periods { get; }

        public int PeriodYear
        {
            get { return _periodYear; }
            set { SetProperty(ref _periodYear, value, () => PeriodYear); }
        }

        public int PeriodMonth
        {
            get { return _periodMonth; }
            set { SetProperty(ref _periodMonth, value, () => PeriodMonth); }
        }

        public void Close(BackgroundWorker worker)
        {
            var selectedPeriod = new DateTime(_periodYear, _periodMonth, 1);
            if (selectedPeriod.AddMonths(-1) > currentPeriod)
            {
                MessageBox.Show("This period cannot be closed at the moment.", "Invalid Command", MessageBoxButton.OK);
                return;
            }

            using (var context = UtilityMethods.createContext())
            {
                var items = context.Inventory.ToList();

                var index = 1;
                foreach (var item in items)
                {
                    foreach (var warehouse in _warehouses)
                    {
                        var beginningBalance = GetBeginningBalance(context, warehouse, item);
                        var endingBalance = GetEndingBalance(context, warehouse, item, beginningBalance);
                        SetEndingBalance(context, warehouse, item, endingBalance);
                    }

                    var status = index++ * (items.Count / 100) - 1;
                    if (status < 0) status = 0;
                    worker.ReportProgress(status);
                }

                context.SaveChanges();
            }

            MessageBox.Show("Successfully closed stock!", "Success", MessageBoxButton.OK);
        }

        #region Helper Methods

        private int GetBeginningBalance(ERPContext context, Warehouse warehouse, Item item)
        {
            var periodYearBalances =
                context.StockBalances.FirstOrDefault(
                    e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.Year == _periodYear);

            if (periodYearBalances == null) return 0;

            switch (_periodMonth)
            {
                case 1:
                    return periodYearBalances.BeginningBalance;
                case 2:
                    return periodYearBalances.Balance1;
                case 3:
                    return periodYearBalances.Balance2;
                case 4:
                    return periodYearBalances.Balance3;
                case 5:
                    return periodYearBalances.Balance4;
                case 6:
                    return periodYearBalances.Balance5;
                case 7:
                    return periodYearBalances.Balance6;
                case 8:
                    return periodYearBalances.Balance7;
                case 9:
                    return periodYearBalances.Balance8;
                case 10:
                    return periodYearBalances.Balance9;
                case 11:
                    return periodYearBalances.Balance10;
                default:
                    return periodYearBalances.Balance11;
            }
        }

        private int GetEndingBalance(ERPContext context, Warehouse warehouse, Item item, int beginningBalance)
        {
            var balance = beginningBalance;

            var purchaseLines = context.PurchaseTransactionLines
                .Include("PurchaseTransaction")
                .Include("PurchaseTransaction.Supplier")
                .Where(line =>
                    line.PurchaseTransaction.Date.Month == _periodMonth &&
                    line.PurchaseTransaction.Date.Year == _periodYear &&
                    line.ItemID.Equals(item.ItemID) && line.WarehouseID.Equals(warehouse.ID) &&
                    !line.PurchaseTransaction.Supplier.Name.Equals("-") &&
                    !line.PurchaseTransactionID.Substring(0, 2).Equals("SA"))
                .ToList();

            var purchaseReturnLines = context.PurchaseReturnTransactionLines
                .Include("PurchaseReturnTransaction")
                .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                .Where(line =>
                    line.PurchaseReturnTransaction.Date.Month == _periodMonth &&
                    line.PurchaseReturnTransaction.Date.Year == _periodYear &&
                    line.ItemID.Equals(item.ItemID) && line.WarehouseID.Equals(warehouse.ID))
                .ToList();

            var salesLines = context.SalesTransactionLines
                .Include("SalesTransaction")
                .Include("SalesTransaction.Customer")
                .Where(line =>
                    line.SalesTransaction.Date.Month == _periodMonth && line.SalesTransaction.Date.Year == _periodYear &&
                    line.ItemID.Equals(item.ItemID) && line.WarehouseID.Equals(warehouse.ID))
                .ToList();

            var salesReturnLines = context.SalesReturnTransactionLines
                .Include("SalesReturnTransaction")
                .Include("SalesReturnTransaction.SalesTransaction.Customer")
                .Where(line =>
                    line.SalesReturnTransaction.Date.Month == _periodMonth &&
                    line.SalesReturnTransaction.Date.Year == _periodYear && line.ItemID.Equals(item.ItemID) &&
                    line.WarehouseID.Equals(warehouse.ID))
                .ToList();

            var stockAdjustmentLines = context.StockAdjustmentTransactionLines
                .Include("StockAdjustmentTransaction")
                .Where(line =>
                    line.StockAdjustmentTransaction.Date.Month == _periodMonth &&
                    line.StockAdjustmentTransaction.Date.Year == _periodYear &&
                    line.ItemID.Equals(item.ItemID) && line.WarehouseID.Equals(warehouse.ID))
                .ToList();

            var moveStockTransactions = context.StockMovementTransactions
                .Include("FromWarehouse")
                .Include("ToWarehouse")
                .Include("StockMovementTransactionLines")
                .Include("StockMovementTransactionLines.Item")
                .Where(line =>
                    line.Date.Month == _periodMonth && line.Date.Year == _periodYear &&
                    (line.FromWarehouse.ID.Equals(warehouse.ID) || line.ToWarehouse.ID.Equals(warehouse.ID)))
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

            foreach (var transaction in moveStockTransactions)
            {
                foreach (var line in transaction.StockMovementTransactionLines)
                {
                    if (!line.ItemID.Equals(item.ItemID)) continue;

                    if (transaction.FromWarehouse.ID.Equals(warehouse.ID))
                        balance -= line.Quantity;
                    
                    else if (transaction.ToWarehouse.ID.Equals(warehouse.ID))
                        balance += line.Quantity;  
                }
            }
            return balance;
        }

        private void SetEndingBalance(ERPContext context, Warehouse warehouse, Item item, int endingBalance)
        {
            var stockBalance =
                context.StockBalances
                    .SingleOrDefault(
                        balance =>
                            balance.Year == _periodYear && balance.ItemID.Equals(item.ItemID) &&
                            balance.WarehouseID.Equals(warehouse.ID));

            var flag = true;

            if (stockBalance == null)
            {
                flag = false;
                stockBalance = new StockBalance
                {
                    Item = context.Inventory.SingleOrDefault(e => e.ItemID.Equals(item.ItemID)),
                    Warehouse = context.Warehouses.SingleOrDefault(e => e.ID.Equals(warehouse.ID)),
                    Year = _periodYear
                };
            }

            switch (_periodMonth)
            {
                case 1:
                    stockBalance.Balance1 = endingBalance;
                    break;
                case 2:
                    stockBalance.Balance2 = endingBalance;
                    break;
                case 3:
                    stockBalance.Balance3 = endingBalance;
                    break;
                case 4:
                    stockBalance.Balance4 = endingBalance;
                    break;
                case 5:
                    stockBalance.Balance5 = endingBalance;
                    break;
                case 6:
                    stockBalance.Balance6 = endingBalance;
                    break;
                case 7:
                    stockBalance.Balance7 = endingBalance;
                    break;
                case 8:
                    stockBalance.Balance8 = endingBalance;
                    break;
                case 9:
                    stockBalance.Balance9 = endingBalance;
                    break;
                case 10:
                    stockBalance.Balance10 = endingBalance;
                    break;
                case 11:
                    stockBalance.Balance11 = endingBalance;
                    break;
                default:
                    stockBalance.Balance12 = endingBalance;
                    break;
            }

            if (!flag)
            {
                context.StockBalances.Add(stockBalance);
            }
        }

        #endregion
    }
}