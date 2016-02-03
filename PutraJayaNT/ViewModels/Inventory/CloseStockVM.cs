using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System;
using System.Linq;

namespace PutraJayaNT.ViewModels.Inventory
{
    class CloseStockVM : ViewModelBase
    {
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

        public void Close()
        {
            _beginningDate = new DateTime(_periodYear, _period, 1, 0, 0, 0);
            _endingDate = _beginningDate.AddMonths(1).AddDays(-1);

            using (var context = new ERPContext())
            {
                var items = context.Inventory.ToList();

                foreach (var item in items)
                {
                    var stocks = context.Stocks.Include("Item").Include("Warehouse").Where(e => e.ItemID.Equals(item.ItemID)).ToList();

                    foreach (var stock in stocks)
                    {
                        var beginningBalance = GetBeginningBalance(context, stock.Warehouse, item);
                        var endingBalance = stock.Pieces;
                        SetEndingBalance(context, stock.Warehouse, item, endingBalance);
                    }
                }

                context.SaveChanges();
            }
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
