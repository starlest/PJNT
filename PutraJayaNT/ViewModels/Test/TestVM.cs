namespace PutraJayaNT.ViewModels.Test
{
    using System.Windows.Input;
    using MVVMFramework;
    using Utilities;
    using System.Linq;
    using System.Windows;
    using Models.Accounting;
    using System;
    using System.Transactions;
    using Models.Inventory;

    class TestVM : ViewModelBase
    {
        ICommand _checkSalesTransactionsCommand;
        ICommand _checkPurchaseTransactionsCommand;
        ICommand _checkInventoryCommand;
        ICommand _checkStockCommand;
        ICommand _checkSoldOrReturnedCommand;
        ICommand _checkLedgerTransactionsCommand;
        ICommand _checkLedgerGeneralCommand;
        ICommand _checkPastCommand;

        public ICommand CheckSalesTransactionsCommand
        {
            get
            {
                return _checkSalesTransactionsCommand ?? (_checkSalesTransactionsCommand = new RelayCommand(() =>
                {
                    using (var context = new ERPContext())
                    {
                        var transactions = context.SalesTransactions
                        .Include("SalesTransactionLines")
                        .ToList();

                        foreach (var transaction in transactions)
                        {
                            decimal grossAmount = 0M;
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

                            if (grossAmount != transaction.GrossTotal)
                            {
                                //if (transaction.InvoiceIssued == null)
                                //{
                                //    transaction.GrossTotal = grossAmount;
                                //    transaction.Total = grossAmount - transaction.Discount + transaction.SalesExpense;
                                //    context.SaveChanges();
                                //}

                                if (MessageBox.Show(string.Format("{0} has wrong Gross Total Amount. \n {1}/{2} \n Invoice Issued: {3} \n Paid: {4} \n Fix?",
                                    transaction.SalesTransactionID, grossAmount, transaction.GrossTotal, transaction.InvoiceIssued, transaction.Paid),
                                    "Error", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    transaction.GrossTotal = grossAmount;
                                    transaction.NetTotal = grossAmount - transaction.Discount + transaction.SalesExpense;
                                    transaction.Paid = transaction.NetTotal;
                                    context.SaveChanges();
                                }
                            }
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
                    using (var context = new ERPContext())
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

                                if (MessageBox.Show(string.Format("{0} has wrong Gross Total Amount. \n {1}/{2}  \n Paid: {3} \n Fix?",
                                    transaction.PurchaseID, grossAmount, transaction.GrossTotal, transaction.Paid),
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
                    using (var ts = new TransactionScope())
                    {
                        var context = new ERPContext();

                        var actualCOGS = context.Ledger_Account_Balances.Where(e => e.LedgerAccount.Name.Equals("Inventory")).FirstOrDefault().Balance2 +
                        context.Ledger_General.Where(e => e.LedgerAccount.Name.Equals("Inventory")).FirstOrDefault().Debit - 
                        context.Ledger_General.Where(e => e.LedgerAccount.Name.Equals("Inventory")).FirstOrDefault().Credit; // changge beginningbalaance

                        decimal calculatedCOGS = 0;

                        var purchaseTransactionLines = context.PurchaseTransactionLines
                        .Include("PurchaseTransaction")
                        .Where(e => e.SoldOrReturned < e.Quantity).ToList();

                        foreach (var purchase in purchaseTransactionLines)
                        {
                            var availableQuantity = purchase.Quantity - purchase.SoldOrReturned;
                            var purchaseLineNetTotal = purchase.PurchasePrice - purchase.Discount;
                            if (purchaseLineNetTotal == 0) continue;
                            var fractionOfTransactionDiscount = ((availableQuantity * purchaseLineNetTotal) / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                            var fractionOfTransactionTax = ((availableQuantity * purchaseLineNetTotal) / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                            calculatedCOGS += (availableQuantity * purchaseLineNetTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                        }

                        if (actualCOGS != calculatedCOGS)
                        {
                           if (MessageBox.Show(string.Format("Actual Inventory: {0} \n Calculated Inventory: {1} \n Difference: {2} \n Fix?", actualCOGS, calculatedCOGS, actualCOGS - calculatedCOGS), "Error", MessageBoxButton.YesNo) ==
                                MessageBoxResult.Yes)
                            {
                                var newTransaction = new LedgerTransaction();
                                DatabaseLedgerHelper.AddTransaction(context, newTransaction, UtilityMethods.GetCurrentDate().Date, "Inventory Adjustment", "Inventory Adjustment");
                                context.SaveChanges();

                                if (actualCOGS < calculatedCOGS)
                                {
                                    DatabaseLedgerHelper.AddTransactionLine(context, newTransaction, "Inventory", "Debit", calculatedCOGS - actualCOGS);
                                    DatabaseLedgerHelper.AddTransactionLine(context, newTransaction, "Cost of Goods Sold", "Credit", calculatedCOGS - actualCOGS);
                                    MessageBox.Show(string.Format("Increased inventory by {0}", calculatedCOGS - actualCOGS), "Fixed", MessageBoxButton.OK);
                                }

                                else
                                {
                                    DatabaseLedgerHelper.AddTransactionLine(context, newTransaction, "Cost of Goods Sold", "Debit", actualCOGS - calculatedCOGS);
                                    DatabaseLedgerHelper.AddTransactionLine(context, newTransaction, "Inventory", "Credit", actualCOGS - calculatedCOGS);
                                    MessageBox.Show(string.Format("Decreased inventory by {0}", actualCOGS - calculatedCOGS), "Fixed", MessageBoxButton.OK);
                                }

                                context.SaveChanges();
                            }
                        }

                        ts.Complete();
                    }

                    MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                }));
            }
        }

        public ICommand CheckStockCommand
        {
            get
            {
                return _checkStockCommand ?? (_checkStockCommand = new RelayCommand(() =>
                {
                    using (var context = new ERPContext())
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
                                .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID))
                                .FirstOrDefault();

                                var actualBalance = stock == null ? 0 : stock.Pieces;
                                var calculatedBalance = GetBeginningBalance(item, warehouse, UtilityMethods.GetCurrentDate().Date);
                                var difference = actualBalance - calculatedBalance;
                                if (difference != 0)
                                {
                                    if (MessageBox.Show(string.Format("{0}-{1} {2} \n Actual: {3} \n Calculated: {4}", item.ItemID, warehouse.ID, item.Name, actualBalance, calculatedBalance), "Error", MessageBoxButton.YesNo)
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
                    using (var context = new ERPContext())
                    {
                        
                        var items = context.Inventory.ToList();
                        foreach (var item in items)
                        {
                            var purchaseLines = context.PurchaseTransactionLines.Where(e => e.ItemID.Equals(item.ItemID)).ToList();
                            var soldOrReturned = 0;
                            foreach (var line in purchaseLines)
                            {
                                soldOrReturned += line.SoldOrReturned;
                            }

                            var salesLines = context.SalesTransactionLines.Where(e => e.SalesTransaction.InvoiceIssued != null && e.ItemID.Equals(item.ItemID)).ToList();
                            var salesReturnLines = context.SalesReturnTransactionLines.Where(e => e.ItemID.Equals(item.ItemID)).ToList();
                            var purchaseReturnLines = context.PurchaseReturnTransactionLines.Where(e => e.ItemID.Equals(item.ItemID)).ToList();
                            var adjustmentLines = context.AdjustStockTransactionLines.Where(e => e.AdjustStockTransactionID.Substring(0, 2).Equals("SA") && e.ItemID.Equals(item.ItemID) && e.Quantity < 0).ToList();
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
                                MessageBox.Show(string.Format("{0} \n Actual: {1} \n Calculated: {2}", item.ItemID, soldOrReturned, soldOrReturnedQuantity), "Error", MessageBoxButton.OK);
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
                    using (var context = new ERPContext())
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
                                    MessageBox.Show(string.Format("Check {0} - {1}", transaction.ID, line.Seq), "Error", MessageBoxButton.OK);
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
                                    MessageBox.Show(string.Format("Check {0} - {1}", transaction.ID, line.Seq), "Error", MessageBoxButton.OK);
                                }
                            }

                            if ((totalDebit - totalCredit) != 0)
                            {
                                MessageBox.Show(string.Format("Trasaction {0}: \n Total Debit: {1} \n Total Credit: {2}", transaction.ID, totalDebit, totalCredit), "Error", MessageBoxButton.OK);
                            }
                        }


                        if ((d - c) != 0)
                        {
                            MessageBox.Show(string.Format("Total Debit: {0} \n Total Credit: {1}", d, c), "Error", MessageBoxButton.OK);
                        }

                        var transactionLines = context.Ledger_Transaction_Lines
                        .Include("LedgerTransaction")
                        .ToList();

                        if (transactionLines.Count != count)
                        {
                            MessageBox.Show(string.Format("Count: {0}/{1}", transactionLines.Count, count), "Error", MessageBoxButton.OK);
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
                                MessageBox.Show(string.Format("Check {0} - {1}", line.LedgerTransactionID, line.Seq), "Error", MessageBoxButton.OK);
                            }

                            if (!found)
                            {
                                MessageBox.Show(string.Format("Check {0} - {1}", line.LedgerTransactionID, line.Seq), "Error", MessageBoxButton.OK);
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
                                    MessageBox.Show(string.Format("Check {0}", h.LedgerTransactionID), "Error", MessageBoxButton.OK);
                                }

                            }
                        }

                        MessageBox.Show(string.Format("Check done. \n Total Lines: {0}", transactionLines.Count), "Successful", MessageBoxButton.OK);
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
                    using (var context = new ERPContext())
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
                            foreach (var line in a.LedgerTransactionLines.Where(e => e.LedgerTransaction.Date.Month == 3))
                            {
                                count++;
                                if (line.Amount < 0) MessageBox.Show(string.Format("Check {0} - {1}", line.LedgerTransactionID, line.Seq), "Error", MessageBoxButton.OK);

                                if (line.Seq == "Debit") totalDebit += line.Amount;
                                else if (line.Seq == "Credit") totalCredit += line.Amount;

                                else
                                {
                                    MessageBox.Show(string.Format("Check {0} - {1}", line.LedgerTransactionID, line.Seq), "Error", MessageBoxButton.OK);
                                }
                            }

                            if ((totalDebit - a.LedgerGeneral.Debit) != 0 || (totalCredit - a.LedgerGeneral.Credit) != 0)
                            {
                                if (MessageBox.Show(string.Format("Account {0}: \n Total Debit: {1}/{2} \n Total Credit: {3}/{4} \n Fix?", 
                                    a.Name, totalDebit, a.LedgerGeneral.Debit, totalCredit, a.LedgerGeneral.Credit), "Error", MessageBoxButton.YesNo)
                                    == MessageBoxResult.Yes)
                                {
                                    a.LedgerGeneral.Debit = totalDebit;
                                    a.LedgerGeneral.Credit = totalCredit;
                                    context.SaveChanges();
                                }
                            }
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
                            MessageBox.Show(string.Format("Th {0}/{1}", accounts.Count, ledgerGenerals.Count), "Error", MessageBoxButton.OK);
                        }

                        if (totalD != totalC)
                        {
                            MessageBox.Show(string.Format("Total Debit: {0} \n Total Credit: {1}", totalD, totalC), "Error", MessageBoxButton.OK);
                        }

                        MessageBox.Show(string.Format("Check done. \n Total Lines: {0}", count), "Successful", MessageBoxButton.OK);
                    }
                }));
            }
        }

        public ICommand CheckPastCommand
        {
            get
            {
                return _checkPastCommand ?? (_checkPastCommand = new RelayCommand(() =>
                {
                    using (var context = new ERPContext())
                    {
                        var transactions = context.SalesTransactions.Where(e => e.InvoiceIssued == null && e.Paid > 0).ToList();

                        foreach (var t in transactions)
                        {
                 
                                MessageBox.Show(string.Format("{0}", t.SalesTransactionID), "Error", MessageBoxButton.OK);
                            
                        }

                        MessageBox.Show("Check done.", "Successful", MessageBoxButton.OK);
                    }
                }));
            }
        }

        private int GetPeriodBeginningBalance(Item item, Warehouse warehouse, int year, int month)
        {
            var beginningBalance = 0;
            using (var context = new ERPContext())
            {
                var stockBalance = context.StockBalances.Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.Year == year).FirstOrDefault();

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

        private int GetBeginningBalance(Item item, Warehouse warehouse, DateTime fromDate)
        {
            var monthDate = fromDate.AddDays(-fromDate.Date.Day + 1);
            var balance = GetPeriodBeginningBalance(item, warehouse, fromDate.Year, fromDate.Month);

            using (var context = new ERPContext())
            {
                var purchaseLines = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Include("PurchaseTransaction.Supplier")
                    .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.PurchaseTransaction.Date >= monthDate && e.PurchaseTransaction.Date <= fromDate &&
                    !e.PurchaseTransactionID.Substring(0, 2).Equals("SA") && !e.PurchaseTransaction.Supplier.Name.Equals("-"))
                    .ToList();

                var purchaseReturnLines = context.PurchaseReturnTransactionLines
                    .Include("PurchaseReturnTransaction")
                    .Include("PurchaseReturnTransaction.PurchaseTransaction.Supplier")
                    .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.PurchaseReturnTransaction.Date >= monthDate && e.PurchaseReturnTransaction.Date <= fromDate)
                    .ToList();

                var salesLines = context.SalesTransactionLines
                    .Include("SalesTransaction")
                    .Include("SalesTransaction.Customer")
                    .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.SalesTransaction.Date >= monthDate && e.SalesTransaction.Date <= fromDate)
                    .ToList();

                var salesReturnLines = context.SalesReturnTransactionLines
                    .Include("SalesReturnTransaction")
                    .Include("SalesReturnTransaction.SalesTransaction.Customer")
                    .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.SalesReturnTransaction.Date >= monthDate && e.SalesReturnTransaction.Date <= fromDate)
                    .ToList();

                var stockAdjustmentLines = context.AdjustStockTransactionLines
                    .Include("AdjustStockTransaction")
                    .Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID) && e.AdjustStockTransaction.Date >= monthDate && e.AdjustStockTransaction.Date <= fromDate)
                    .ToList();

                var moveStockTransactions = context.MoveStockTransactions
                    .Include("FromWarehouse")
                    .Include("ToWarehouse")
                    .Include("MoveStockTransactionLines")
                    .Include("MoveStockTransactionLines.Item")
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
            }

            return balance;
        }
    }
}
