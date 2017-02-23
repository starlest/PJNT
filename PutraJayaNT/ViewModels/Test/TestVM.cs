namespace ECERP.ViewModels.Test
{
    using System;
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using System.Windows.Input;
    using Models.Accounting;
    using Models.Inventory;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;

    internal class TestVM : ViewModelBase
    {
        private ICommand _checkSalesTransactionsCommand;
        private ICommand _checkPurchaseTransactionsCommand;
        private ICommand _checkInventoryCommand;
        private ICommand _checkStockCommand;
        private ICommand _checkSoldOrReturnedCommand;
        private ICommand _checkLedgerTransactionsCommand;
        private ICommand _checkLedgerGeneralCommand;
        private ICommand _checkIssueFailedCommand;

        public ICommand CheckSalesTransactionsCommand
        {
            get
            {
                return _checkSalesTransactionsCommand ?? (_checkSalesTransactionsCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var transactions = context.SalesTransactions
                                   .Include("SalesTransactionLines")
                                   .ToList();

                               foreach (var t in transactions)
                               {
                                   var lines =
                                       context.SalesTransactionLines.Where(
                                           line => line.SalesTransaction.SalesTransactionID.Equals(t.SalesTransactionID));

                                   if (lines.Count() != t.SalesTransactionLines.Count)
                                   {
                                       MessageBox.Show(t.SalesTransactionID);
                                   }
                               }
                               foreach (var transaction in transactions)
                               {
                                   var grossAmount = 0M;
                                   foreach (var line in transaction.SalesTransactionLines)
                                   {
                                       var actualTotal = line.Quantity * (line.SalesPrice - line.Discount);
                                       if (line.Total != actualTotal)
                                       {
                                           //if (MessageBox.Show(string.Format("{0} has wrong Line Total Amount. \n {1}/{2} \n Fix?",
                                           //    transaction.SalesTransactionID, actualTotal, line.Total), "Error", MessageBoxButton.YesNo) ==
                                           //    MessageBoxResult.Yes)
                                           //{
                                           //    line.Total = actualTotal;
                                           //    context.SaveChanges();
                                           //}
                                       }
                                       grossAmount += line.Total;
                                   }

                                   if (grossAmount == transaction.GrossTotal) continue;

                                   //if (transaction.InvoiceIssued == null)
                                   //{
                                   //    transaction.GrossTotal = grossAmount;
                                   //    transaction.Total = grossAmount - transaction.Discount + transaction.SalesExpense;
                                   //    context.SaveChanges();
                                   //}

                                   if (MessageBox.Show(
                                           $"{transaction.SalesTransactionID} has wrong Gross Total Amount. \n {grossAmount}/{transaction.GrossTotal} \n Invoice Issued: {transaction.InvoiceIssued} \n Paid: {transaction.Paid} \n Fix?",
                                           "Error", MessageBoxButton.YesNo) != MessageBoxResult.Yes) continue;

                                   transaction.GrossTotal = grossAmount;
                                   transaction.NetTotal = grossAmount - transaction.Discount + transaction.SalesExpense;
                                   transaction.Paid = transaction.NetTotal;
                                   context.SaveChanges();
                               }

                               MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                           }
                       }));
            }
        }

        public ICommand CheckPurchaseTransactionsCommand
        {
            get
            {
                return _checkPurchaseTransactionsCommand ?? (_checkPurchaseTransactionsCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var transactions = context.PurchaseTransactions
                                   .Include("PurchaseTransactionLines")
                                   .ToList();

                               foreach (var transaction in transactions)
                               {
                                   decimal grossAmount = 0M;
                                   foreach (var line in transaction.PurchaseTransactionLines)
                                   {
                                       var actualTotal = line.Quantity * (line.PurchasePrice - line.Discount);
                                       if (line.Total != actualTotal)
                                       {
                                           //if (MessageBox.Show(string.Format("{0} has wrong Line Total Amount. \n {1}/{2} \n Fix?",
                                           //    transaction.PurchaseID, actualTotal, line.Total), "Error", MessageBoxButton.YesNo) ==
                                           //    MessageBoxResult.Yes)
                                           //{
                                           //    line.Total = actualTotal;
                                           //    context.SaveChanges();
                                           //}
                                       }
                                       grossAmount += line.Total;
                                   }

                                   if (grossAmount != transaction.GrossTotal)
                                   {
                                       //if (transaction.InvoiceIssued == null)
                                       //{
                                       //    transaction.GrossTotal = grossAmount;
                                       //    transaction.Total = grossAmount - transaction.Discount + transaction.SalesExpense;
                                       //    context.SaveChanges();
                                       //}

                                       if (MessageBox.Show(
                                               string.Format(
                                                   "{0} has wrong Gross Total Amount. \n {1}/{2}  \n Paid: {3} \n Fix?",
                                                   transaction.PurchaseID, grossAmount, transaction.GrossTotal,
                                                   transaction.Paid),
                                               "Error", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                       {
                                           transaction.GrossTotal = grossAmount;
                                           transaction.Total = grossAmount - transaction.Discount + transaction.Tax;
                                           transaction.Paid = transaction.Total;
                                           context.SaveChanges();
                                       }
                                   }
                               }
                           }

                           MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                       }));
            }
        }

        public ICommand CheckInventoryCommand
        {
            get
            {
                return _checkInventoryCommand ?? (_checkInventoryCommand = new RelayCommand(() =>
                       {
                           CheckInventoryValue();

                           MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                       }));
            }
        }

        private static void CheckInventoryValue()
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();

                var actualCOGS =
                    context.Ledger_Account_Balances.Single(e => e.LedgerAccount.Name.Equals("Inventory") && e.PeriodYear == 2017).Balance1 +
                    context.Ledger_General.Single(e => e.LedgerAccount.Name.Equals("Inventory")).Debit -
                    context.Ledger_General.Single(e => e.LedgerAccount.Name.Equals("Inventory")).Credit;
                // change beginningbalance

                decimal calculatedCOGS = 0;

                var purchaseTransactionLines = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Where(e => e.SoldOrReturned < e.Quantity).ToList();

                foreach (var purchase in purchaseTransactionLines)
                {
                    var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                    var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;
                    if (purchaseLineNetTotal == 0) continue;
                    var fractionOfTransactionDiscount = availableQuantity * purchaseLineNetTotal /
                                                        purchase.PurchaseTransaction.GrossTotal *
                                                        purchase.PurchaseTransaction.Discount;
                    var fractionOfTransactionTax = availableQuantity * purchaseLineNetTotal /
                                                   purchase.PurchaseTransaction.GrossTotal *
                                                   purchase.PurchaseTransaction.Tax;
                    calculatedCOGS += availableQuantity * purchaseLineNetTotal - fractionOfTransactionDiscount +
                                      fractionOfTransactionTax;
                }

                if (actualCOGS != calculatedCOGS)
                {
                    if (MessageBox.Show(
                            $"Actual Inventory: {actualCOGS} \n Calculated Inventory: {calculatedCOGS} \n Difference: {actualCOGS - calculatedCOGS} \n Fix?",
                            "Error", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        var newTransaction = new LedgerTransaction();
                        LedgerTransactionHelper.AddTransactionToDatabase(context, newTransaction,
                            UtilityMethods.GetCurrentDate().Date, "Inventory Adjustment", "Inventory Adjustment");
                        context.SaveChanges();

                        if (actualCOGS < calculatedCOGS)
                        {
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, newTransaction, "Inventory",
                                "Debit", calculatedCOGS - actualCOGS);
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, newTransaction,
                                "Cost of Goods Sold", "Credit", calculatedCOGS - actualCOGS);
                            MessageBox.Show($"Increased inventory by {calculatedCOGS - actualCOGS}", "Fixed",
                                MessageBoxButton.OK);
                        }

                        else
                        {
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, newTransaction,
                                "Cost of Goods Sold", "Debit", actualCOGS - calculatedCOGS);
                            LedgerTransactionHelper.AddTransactionLineToDatabase(context, newTransaction, "Inventory",
                                "Credit", actualCOGS - calculatedCOGS);
                            MessageBox.Show($"Decreased inventory by {actualCOGS - calculatedCOGS}", "Fixed",
                                MessageBoxButton.OK);
                        }
                        context.SaveChanges();
                    }
                }
                ts.Complete();
            }
        }

        public ICommand CheckStockCommand
        {
            get
            {
                return _checkStockCommand ?? (_checkStockCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var items = context.Inventory.ToList();
                               var warehouses = context.Warehouses.ToList();

                               foreach (var item in items)
                               {
                                   foreach (var warehouse in warehouses)
                                   {
                                       var stock = context.Stocks
                                           .Include("Item")
                                           .Include("Warehouse")
                                           .FirstOrDefault(
                                               e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID));

                                       var actualBalance = stock?.Pieces ?? 0;
                                       var calculatedBalance = GetBeginningBalance(item, warehouse,
                                           UtilityMethods.GetCurrentDate().Date);
                                       var difference = actualBalance - calculatedBalance;
                                       if (difference != 0)
                                       {
                                           if (MessageBox.Show(
                                                   $"{item.ItemID}-{warehouse.ID} {item.Name} \n Actual: {actualBalance} \n Calculated: {calculatedBalance}",
                                                   "Error", MessageBoxButton.YesNo)
                                               == MessageBoxResult.No) return;
                                       }
                                   }
                               }

                               MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                           }
                       }));
            }
        }

        public ICommand CheckSoldOrReturnedCommand
        {
            get
            {
                return _checkSoldOrReturnedCommand ?? (_checkSoldOrReturnedCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var items = context.Inventory.ToList();
                               foreach (var item in items)
                               {
                                   var purchaseLines =
                                       context.PurchaseTransactionLines.Where(e => e.ItemID.Equals(item.ItemID))
                                           .ToList();
                                   var soldOrReturned = 0;
                                   foreach (var line in purchaseLines)
                                   {
                                       soldOrReturned += line.SoldOrReturned;
                                   }

                                   var salesLines =
                                       context.SalesTransactionLines.Where(
                                               e => e.ItemID.Equals(item.ItemID) && e.SalesTransaction.InvoiceIssued != null)
                                           .ToList();
                                   var salesReturnLines =
                                       context.SalesReturnTransactionLines.Where(e => e.ItemID.Equals(item.ItemID))
                                           .ToList();
                                   var purchaseReturnLines =
                                       context.PurchaseReturnTransactionLines.Where(e => e.ItemID.Equals(item.ItemID))
                                           .ToList();
                                   var adjustmentLines =
                                       context.StockAdjustmentTransactionLines.Where(
                                           e =>
                                               e.StockAdjustmentTransactionID.Substring(0, 2).Equals("SA") &&
                                               e.ItemID.Equals(item.ItemID) && e.Quantity < 0).ToList();
                                   var soldOrReturnedQuantity = 0;

                                   foreach (var line in salesLines)
                                   {
                                       soldOrReturnedQuantity += line.Quantity;
                                   }

                                   foreach (var line in salesReturnLines)
                                   {
                                       soldOrReturnedQuantity -= line.Quantity;
                                   }

                                   foreach (var line in purchaseReturnLines)
                                   {
                                       soldOrReturnedQuantity += line.Quantity;
                                   }

                                   foreach (var line in adjustmentLines)
                                   {
                                       soldOrReturnedQuantity += (-line.Quantity);
                                   }

                                   if (soldOrReturned != soldOrReturnedQuantity)
                                   {
                                       MessageBox.Show(
                                           $"{item.ItemID} \n Actual: {soldOrReturned} \n Calculated: {soldOrReturnedQuantity}",
                                           "Error", MessageBoxButton.OK);
                                   }
                               }

                               MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                           }
                       }));
            }
        }

        public ICommand CheckLedgerTransactionsCommand
        {
            get
            {
                return _checkLedgerTransactionsCommand ?? (_checkLedgerTransactionsCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var transactions = context.Ledger_Transactions
                                   .Include("LedgerTransactionLines")
                                   .ToList();
                               var accounts = context.Ledger_Accounts
                                   .Include("LedgerTransactionLines")
                                   .Include("LedgerGeneral")
                                   .ToList();

                               decimal d = 0;
                               decimal c = 0;
                               var count = 0;
                               foreach (var transaction in transactions)
                               {
                                   decimal totalDebit = 0;
                                   decimal totalCredit = 0;
                                   foreach (var line in transaction.LedgerTransactionLines)
                                   {
                                       count++;
                                       if (line.Seq == "Debit")
                                       {
                                           d += line.Amount;
                                           totalDebit += line.Amount;
                                       }
                                       else if (line.Seq == "Credit")
                                       {
                                           c += line.Amount;
                                           totalCredit += line.Amount;
                                       }

                                       else
                                       {
                                           MessageBox.Show(string.Format("Check {0} - {1}", transaction.ID, line.Seq),
                                               "Error", MessageBoxButton.OK);
                                       }

                                       // Check if the transaction line's account exists
                                       var found = false;
                                       foreach (var a in accounts)
                                       {
                                           if (a.ID.Equals(line.LedgerAccountID))
                                           {
                                               found = true;
                                               break;
                                           }
                                       }

                                       if (!found)
                                       {
                                           MessageBox.Show(string.Format("Check {0} - {1}", transaction.ID, line.Seq),
                                               "Error", MessageBoxButton.OK);
                                       }
                                   }

                                   if ((totalDebit - totalCredit) != 0)
                                   {
                                       MessageBox.Show(
                                           string.Format("Trasaction {0}: \n Total Debit: {1} \n Total Credit: {2}",
                                               transaction.ID, totalDebit, totalCredit), "Error", MessageBoxButton.OK);
                                   }
                               }


                               if ((d - c) != 0)
                               {
                                   MessageBox.Show(string.Format("Total Debit: {0} \n Total Credit: {1}", d, c), "Error",
                                       MessageBoxButton.OK);
                               }

                               var transactionLines = context.Ledger_Transaction_Lines
                                   .Include("LedgerTransaction")
                                   .ToList();

                               if (transactionLines.Count != count)
                               {
                                   MessageBox.Show(string.Format("Count: {0}/{1}", transactionLines.Count, count),
                                       "Error", MessageBoxButton.OK);
                               }

                               foreach (var line in transactionLines)
                               {
                                   var found = false;
                                   foreach (var t in transactions)
                                   {
                                       if (line.LedgerTransactionID == t.ID)
                                       {
                                           found = true;
                                           break;
                                       }
                                   }

                                   if (line.LedgerTransaction == null)
                                   {
                                       MessageBox.Show(
                                           string.Format("Check {0} - {1}", line.LedgerTransactionID, line.Seq), "Error",
                                           MessageBoxButton.OK);
                                   }

                                   if (!found)
                                   {
                                       MessageBox.Show(
                                           string.Format("Check {0} - {1}", line.LedgerTransactionID, line.Seq), "Error",
                                           MessageBoxButton.OK);
                                   }
                               }

                               foreach (var a in accounts)
                               {
                                   foreach (var h in a.LedgerTransactionLines)
                                   {
                                       var found = false;
                                       foreach (var z in transactions)
                                       {
                                           if (h.LedgerTransactionID.Equals(z.ID))
                                           {
                                               found = true;
                                               break;
                                           }
                                       }

                                       if (!found)
                                       {
                                           MessageBox.Show(string.Format("Check {0}", h.LedgerTransactionID), "Error",
                                               MessageBoxButton.OK);
                                       }
                                   }
                               }

                               MessageBox.Show($"Check done. \n Total Lines: {transactionLines.Count}", "Successful",
                                   MessageBoxButton.OK);
                           }
                       }));
            }
        }

        public ICommand CheckLedgerGeneralCommand
        {
            get
            {
                return _checkLedgerGeneralCommand ?? (_checkLedgerGeneralCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var accounts = context.Ledger_Accounts
                                   .Include("LedgerTransactionLines")
                                   .Include("LedgerGeneral")
                                   .ToList();

                               var count = 0;
                               foreach (var a in accounts)
                               {
                                   decimal totalDebit = 0;
                                   decimal totalCredit = 0;
                                   foreach (
                                       var line in
                                       a.LedgerTransactionLines.Where(e => e.LedgerTransaction.Date.Year == 2017 && e.LedgerTransaction.Date.Month == 1))
                                   {
                                       count++;
                                       if (line.Amount < 0)
                                           MessageBox.Show($"Check {line.LedgerTransactionID} - {line.Seq}", "Error",
                                               MessageBoxButton.OK);

                                       switch (line.Seq)
                                       {
                                           case Constants.Accounting.DEBIT:
                                               totalDebit += line.Amount;
                                               break;
                                           case Constants.Accounting.CREDIT:
                                               totalCredit += line.Amount;
                                               break;
                                           default:
                                               MessageBox.Show($"Check {line.LedgerTransactionID} - {line.Seq}", "Error",
                                                   MessageBoxButton.OK);
                                               break;
                                       }
                                   }

                                   if (totalDebit - a.LedgerGeneral.Debit == 0 &&
                                       totalCredit - a.LedgerGeneral.Credit == 0)
                                       continue;
                                   if (MessageBox.Show(
                                           $"Account {a.Name}: \n Total Debit: {totalDebit}/{a.LedgerGeneral.Debit} \n Total Credit: {totalCredit}/{a.LedgerGeneral.Credit} \n Fix?",
                                           "Error", MessageBoxButton.YesNo) != MessageBoxResult.Yes) continue;
                                   a.LedgerGeneral.Debit = totalDebit;
                                   a.LedgerGeneral.Credit = totalCredit;
                                   context.SaveChanges();
                               }

                               var ledgerGenerals = context.Ledger_General.ToList();
                               decimal totalD = 0;
                               decimal totalC = 0;
                               foreach (var l in ledgerGenerals)
                               {
                                   totalD += l.Debit;
                                   totalC += l.Credit;
                               }

                               if (accounts.Count != ledgerGenerals.Count)
                               {
                                   MessageBox.Show($"Th {accounts.Count}/{ledgerGenerals.Count}", "Error",
                                       MessageBoxButton.OK);
                               }

                               if (totalD != totalC)
                               {
                                   MessageBox.Show($"Total Debit: {totalD} \n Total Credit: {totalC}", "Error",
                                       MessageBoxButton.OK);
                               }

                               MessageBox.Show($"Check done. \n Total Lines: {count}", "Successful", MessageBoxButton.OK);
                           }
                       }));
            }
        }

        public ICommand CheckIssueFailedCommand
        {
            get
            {
                return _checkIssueFailedCommand ?? (_checkIssueFailedCommand = new RelayCommand(() =>
                       {
                           using (var context = UtilityMethods.createContext())
                           {
                               var transactions =
                                   context.SalesTransactions.Where(e => e.InvoiceIssued == null && e.Paid > 0).ToList();
                               foreach (var t in transactions)
                                   MessageBox.Show($"{t.SalesTransactionID}", "Error", MessageBoxButton.OK);
                               MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                           }
                       }));
            }
        }

        private static int GetPeriodBeginningBalance(Item item, Warehouse warehouse, int year, int month)
        {
            var beginningBalance = 0;
            using (var context = UtilityMethods.createContext())
            {
                var stockBalance =
                    context.StockBalances.FirstOrDefault(
                        e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.Year == year);

                if (stockBalance == null)
                {
                    return beginningBalance;
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

            return beginningBalance;
        }

        private static int GetBeginningBalance(Item item, Warehouse warehouse, DateTime fromDate)
        {
            var monthDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var balance = GetPeriodBeginningBalance(item, warehouse, fromDate.Year, fromDate.Month);

            using (var context = UtilityMethods.createContext())
            {
                var purchaseLines = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Include("PurchaseTransaction.Supplier")
                    .Where(
                        e =>
                            e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.PurchaseTransaction.Date >= monthDate && e.PurchaseTransaction.Date <= fromDate &&
                            !e.PurchaseTransactionID.Substring(0, 2).Equals("SA") &&
                            !e.PurchaseTransaction.Supplier.Name.Equals("-"))
                    .ToList();

                var purchaseReturnLines = context.PurchaseReturnTransactionLines
                    .Include("PurchaseReturnTransaction")
                    .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                    .Where(
                        e =>
                            e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.PurchaseReturnTransaction.Date >= monthDate &&
                            e.PurchaseReturnTransaction.Date <= fromDate)
                    .ToList();

                var salesLines = context.SalesTransactionLines
                    .Include("SalesTransaction")
                    .Include("SalesTransaction.Customer")
                    .Where(
                        e =>
                            e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.SalesTransaction.Date >= monthDate && e.SalesTransaction.Date <= fromDate)
                    .ToList();

                var salesReturnLines = context.SalesReturnTransactionLines
                    .Include("SalesReturnTransaction")
                    .Include("SalesReturnTransaction.SalesTransaction.Customer")
                    .Where(
                        e =>
                            e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.SalesReturnTransaction.Date >= monthDate && e.SalesReturnTransaction.Date <= fromDate)
                    .ToList();

                var stockAdjustmentLines = context.StockAdjustmentTransactionLines
                    .Include("StockAdjustmentTransaction")
                    .Where(
                        e =>
                            e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) &&
                            e.StockAdjustmentTransaction.Date >= monthDate &&
                            e.StockAdjustmentTransaction.Date <= fromDate)
                    .ToList();

                var moveStockTransactions = context.StockMovementTransactions
                    .Include("FromWarehouse")
                    .Include("ToWarehouse")
                    .Include("StockMovementTransactionLines")
                    .Include("StockMovementTransactionLines.Item")
                    .Where(e => e.Date >= monthDate && e.Date <= fromDate
                                && (e.FromWarehouse.ID.Equals(warehouse.ID) || e.ToWarehouse.ID.Equals(warehouse.ID)))
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
            }

            return balance;
        }
    }
}