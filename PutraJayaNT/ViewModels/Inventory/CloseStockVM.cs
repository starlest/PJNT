using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace PutraJayaNT.ViewModels.Inventory
{
    class CloseStockVM : ViewModelBase
    {
        List<Warehouse> _warehouses;
        int _periodYear;
        int _period;
        DateTime _beginningDate;
        DateTime _endingDate;

        public CloseStockVM()
        {
            using (var context = new ERPContext())
            {
                _periodYear = context.Ledger_General.FirstOrDefault().PeriodYear;
                _period = 1;

                _warehouses = context.Warehouses.ToList();
            }
        }

        public int PeriodYear
        {
            get { return _periodYear; }
            set { SetProperty(ref _periodYear, value, "PeriodYear"); }
        }

        public int Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value, "Period"); }
        }

        public void Close(BackgroundWorker worker)
        {
            _beginningDate = new DateTime(_periodYear, _period, 1, 0, 0, 0);
            _endingDate = _beginningDate.AddMonths(1).AddDays(-1);

            using (var context = new ERPContext())
            {
                var items = context.Inventory.ToList();

                var index = 1;
                foreach (var item in items)
                {
                    var stocks = context.Stocks.Include("Item").Include("Warehouse").Where(e => e.ItemID.Equals(item.ItemID)).ToList();

                    foreach (var warehouse in _warehouses)
                    {
                        var beginningBalance = GetBeginningBalance(context, warehouse, item);
                        var endingBalance = GetEndingBalance(context, warehouse, item, beginningBalance);
                        SetEndingBalance(context, warehouse, item, endingBalance);
                    }

                    var status = (index++ * (items.Count / 100)) - 1;
                    if (status < 0) status = 0;
                    worker.ReportProgress(status);
                }

                context.SaveChanges();
            }

            MessageBox.Show("Successfully closed stock!", "Succecss", MessageBoxButton.OK);
        }

        #region Helper Methods
        private int GetBeginningBalance(ERPContext context, Warehouse warehouse, Item item)
        {
            var periodYearBalances = context.StockBalances.Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.Year == _periodYear).FirstOrDefault();

            if (periodYearBalances == null) return 0;

            switch (_period)
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
                .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                e.PurchaseTransaction.Date.Month == _period && e.PurchaseTransaction.Date.Year == _periodYear &&
                !e.PurchaseTransaction.Supplier.Name.Equals("-") && !e.PurchaseTransactionID.Substring(0, 2).Equals("SA"))
                .ToList();

            var purchaseReturnLines = context.PurchaseReturnTransactionLines
                .Include("PurchaseReturnTransaction")
                .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                e.PurchaseReturnTransaction.Date.Month == _period && e.PurchaseReturnTransaction.Date.Year == _periodYear)
                .ToList();

            var salesLines = context.SalesTransactionLines
                .Include("SalesTransaction")
                .Include("SalesTransaction.Customer")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                e.SalesTransaction.When.Month == _period && e.SalesTransaction.When.Year == _periodYear)
                .ToList();

            var salesReturnLines = context.SalesReturnTransactionLines
                .Include("SalesReturnTransaction")
                .Include("SalesReturnTransaction.SalesTransaction.Customer")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                e.SalesReturnTransaction.Date.Month == _period && e.SalesReturnTransaction.Date.Year == _periodYear)
                .ToList();

            var stockAdjustmentLines = context.AdjustStockTransactionLines
                .Include("AdjustStockTransaction")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                e.AdjustStockTransaction.Date.Month == _period && e.AdjustStockTransaction.Date.Year == _periodYear)
                .ToList();

            var stockMovementLines = context.MoveStockTransactionLines
                .Include("MoveStockTransaction")
                .Include("MoveStockTransaction.FromWarehouse")
                .Include("MoveStockTransaction.ToWarehouse")
                .Where(e => e.ItemID.Equals(item.ItemID) &&
                (e.MoveStockTransaction.FromWarehouse.ID.Equals(warehouse.ID) || e.MoveStockTransaction.ToWarehouse.ID.Equals(warehouse.ID))
                && e.MoveStockTransaction.Date.Month == _period && e.MoveStockTransaction.Date.Year == _periodYear)
                .ToList();

            var moveStockTransactions = context.MoveStockTransactions
                .Include("FromWarehouse")
                .Include("ToWarehouse")
                .Include("MoveStockTransactionLines")
                .Include("MoveStockTransactionLines.Item")
                .Where(e => e.Date.Month == _period && e.Date.Year == _periodYear
                && (e.FromWarehouse.ID.Equals(warehouse.ID) || e.ToWarehouse.ID.Equals(warehouse.ID)))
                .ToList();

            if (item.Name=="Tep Ketan Rose")
            {

            }

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

            //foreach (var line in stockMovementLines)
            //{
            //    if (line.MoveStockTransaction.FromWarehouse.Equals(warehouse.ID))
            //        balance -= line.Quantity;
            //    else
            //        balance += line.Quantity;
            //}

            foreach (var transaction in moveStockTransactions)
            {
                foreach (var line in transaction.MoveStockTransactionLines)
                {
                    if (line.ItemID.Equals(item.ItemID))
                    {
                        if (transaction.FromWarehouse.ID.Equals(warehouse.ID))
                        {
                            balance -= line.Quantity;
                        }

                        else if (transaction.ToWarehouse.ID.Equals(warehouse.ID))
                        {
                            balance += line.Quantity;
                        }
                    }
                }
            }
            return balance;
        }

        private void SetEndingBalance(ERPContext context, Warehouse warehouse, Item item, int endingBalance)
        {

            var stockBalance = context.StockBalances.Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.Year == _periodYear).FirstOrDefault();

            var flag = true;

            if (stockBalance == null)
            {
                flag = false;
                stockBalance = new StockBalance
                {
                    Item = context.Inventory.Where(e => e.ItemID.Equals(item.ItemID)).FirstOrDefault(),
                    Warehouse = context.Warehouses.Where(e => e.ID.Equals(warehouse.ID)).FirstOrDefault(),
                    Year = _periodYear
                };
            }

            switch (_period)
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
