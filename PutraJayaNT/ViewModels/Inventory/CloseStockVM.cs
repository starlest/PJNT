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
                _period = context.Ledger_General.FirstOrDefault().Period;
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
                    var beginningBalance = GetBeginningBalance(context, item);
                    var endingBalance = GetEndingBalance(context, item, beginningBalance);
                    SetEndingBalance(context, item, endingBalance);
                }

                context.SaveChanges();
            }
        }

        #region Helper Methods
        private int GetBeginningBalance(ERPContext context, Item item)
        {
            var periodYearBalances = context.StockBalances.Where(e => e.ItemID.Equals(item.ItemID) && e.Year == _periodYear).FirstOrDefault();

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

        private int GetEndingBalance(ERPContext context, Item item, int beginningBalance)
        {
            var balance = beginningBalance;

            var purchaseLines = context.PurchaseTransactionLines
                .Include("PurchaseTransaction")
                .Include("PurchaseTransaction.Supplier")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.PurchaseTransaction.Date >= _beginningDate && e.PurchaseTransaction.Date <= _endingDate && !e.PurchaseTransaction.Supplier.Name.Equals("-"))
                .ToList();

            var purchaseReturnLines = context.PurchaseReturnTransactionLines
                .Include("PurchaseReturnTransaction")
                .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.PurchaseReturnTransaction.Date >= _beginningDate && e.PurchaseReturnTransaction.Date <= _endingDate)
                .ToList();

            var salesLines = context.SalesTransactionLines
                .Include("SalesTransaction")
                .Include("SalesTransaction.Customer")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.SalesTransaction.When >= _beginningDate && e.SalesTransaction.When <= _endingDate)
                .ToList();

            var salesReturnLines = context.SalesReturnTransactionLines
                .Include("SalesReturnTransaction")
                .Include("SalesReturnTransaction.SalesTransaction.Customer")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.SalesReturnTransaction.Date >= _beginningDate && e.SalesReturnTransaction.Date <= _endingDate)
                .ToList();

            var stockAdjustmentLines = context.AdjustStockTransactionLines
                .Include("AdjustStockTransaction")
                .Where(e => e.ItemID.Equals(item.ItemID) && e.AdjustStockTransaction.Date >= _beginningDate && e.AdjustStockTransaction.Date <= _endingDate)
                .ToList();

            foreach (var line in purchaseLines)
                balance += line.Quantity;

            foreach (var line in purchaseReturnLines)
                balance -= line.Quantity;

            foreach (var line in salesLines)
                balance -= line.Quantity;

            foreach (var line in salesReturnLines)
                balance -= line.Quantity;

            foreach (var line in stockAdjustmentLines)
                balance += line.Quantity;

            return balance;
        }

        private void SetEndingBalance(ERPContext context, Item item, int endingBalance)
        {

            var periodYearBalances = context.StockBalances.Where(e => e.ItemID.Equals(item.ItemID) && e.Year == _periodYear).FirstOrDefault();

            switch (_period)
            {
                case 1:
                    periodYearBalances.Balance1 = endingBalance;
                    break;
                case 2:
                    periodYearBalances.Balance2 = endingBalance;
                    break;
                case 3:
                    periodYearBalances.Balance3 = endingBalance;
                    break;
                case 4:
                    periodYearBalances.Balance4 = endingBalance;
                    break;
                case 5:
                    periodYearBalances.Balance5 = endingBalance;
                    break;
                case 6:
                    periodYearBalances.Balance6 = endingBalance;
                    break;
                case 7:
                    periodYearBalances.Balance7 = endingBalance;
                    break;
                case 8:
                    periodYearBalances.Balance8 = endingBalance;
                    break;
                case 9:
                    periodYearBalances.Balance9 = endingBalance;
                    break;
                case 10:
                    periodYearBalances.Balance10 = endingBalance;
                    break;
                case 11:
                    periodYearBalances.Balance11 = endingBalance;
                    break;
                default:
                    periodYearBalances.Balance12 = endingBalance;
                    break;
            }
        }
        #endregion
    }
}
