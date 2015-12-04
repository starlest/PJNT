using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    class SalesReturnTransactionVM : ViewModelBase<SalesReturnTransaction>
    {
        ObservableCollection<SalesTransactionLine> _salesTransactionLines;
        ObservableCollection<SalesReturnTransactionLine> _salesReturnTransactionLines;

        string _selectedSalesTransactionID;
        SalesTransactionLine _selectedSalesTransactionLine;
        DateTime? _selectedSalesTransactionWhen;

        string _salesReturnEntryID;
        DateTime _salesReturnEntryDate;
        string _salesReturnEntryProduct;
        int? _salesReturnEntryQuantity;
        ICommand _salesReturnEntryAddCommand;

        ICommand _newCommand;
        ICommand _confirmCommand;

        public SalesReturnTransactionVM()
        {
            Model = new SalesReturnTransaction();

            _salesTransactionLines = new ObservableCollection<SalesTransactionLine>();
            _salesReturnTransactionLines = new ObservableCollection<SalesReturnTransactionLine>();

            _salesReturnEntryDate = DateTime.Now.Date;

            SetSalesReturnTransactionID();
        }

        public ObservableCollection<SalesTransactionLine> SalesTransactionLines
        {
            get { return _salesTransactionLines; }
        }

        public ObservableCollection<SalesReturnTransactionLine> SalesReturnTransactionLines
        {
            get { return _salesReturnTransactionLines; }
        }

        public string SelectedSalesTransactionID
        {
            get { return _selectedSalesTransactionID; }
            set
            {
                SetProperty(ref _selectedSalesTransactionID, value, "SelectedSalesTransactionID");
                if (UpdateSalesTransactionLines())
                {
                    SelectedSalesTransactionWhen = _salesTransactionLines.FirstOrDefault().SalesTransaction.When;
                    Model.SalesTransaction = _salesTransactionLines.FirstOrDefault().SalesTransaction;
                }
                else
                {
                    SelectedSalesTransactionWhen = null;
                }
            }
        }

        public DateTime? SelectedSalesTransactionWhen
        {
            get { return _selectedSalesTransactionWhen; }
            set { SetProperty(ref _selectedSalesTransactionWhen, value, "SelectedSalesTransactionWhen"); }
        }

        public SalesTransactionLine SelectedSalesTransactionLine
        {
            get { return _selectedSalesTransactionLine; }
            set
            {
                SetProperty(ref _selectedSalesTransactionLine, value, "SelectedSalesTransactionLine");
                if (_selectedSalesTransactionLine != null)
                    UpdateReturnEntryProperties();
            }
        }

        private bool UpdateSalesTransactionLines()
        {
            var found = false;

            _salesTransactionLines.Clear();

            using (var context = new ERPContext())
            {
                var lines = context.SalesTransactionLines
                    .Where(e => e.SalesTransactionID.Equals(_selectedSalesTransactionID))
                    .Include("SalesTransaction")
                    .Include("Item")
                    .ToList();

                if (lines.Count > 0) found = true;

                foreach (var line in lines)
                    _salesTransactionLines.Add(line);
            }

            return found;
        }

        private void UpdateReturnEntryProperties()
        {
            SalesReturnEntryProduct = _selectedSalesTransactionLine.Item.Name;
            SalesReturnEntryQuantity = _selectedSalesTransactionLine.Quantity;
        }

        private void SetSalesReturnTransactionID()
        {

            var year = _salesReturnEntryDate.Year;
            var month = _salesReturnEntryDate.Month;

            var newEntryID = "SR" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastEntryID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from SalesReturnTransaction in context.SalesReturnTransactions
                           where SalesReturnTransaction.SalesReturnTransactionID.CompareTo(newEntryID.ToString()) >= 0
                           orderby SalesReturnTransaction.SalesReturnTransactionID descending
                           select SalesReturnTransaction.SalesReturnTransactionID);
                if (IDs.Count() != 0) lastEntryID = IDs.First();
            }

            if (lastEntryID != null) newEntryID = "SR" + (Convert.ToInt64(lastEntryID.Substring(2)) + 1).ToString();

            Model.SalesReturnTransactionID = newEntryID;
            _salesReturnEntryID = newEntryID;
        }

        // -------------------- Return Entry Properties -------------------- //

        public DateTime SalesReturnEntryDate
        {
            get { return _salesReturnEntryDate; }
            set
            {
                SetProperty(ref _salesReturnEntryDate, value, "SalesReturnEntryDate");
                SetSalesReturnTransactionID();
            }
        }

        public string SalesReturnEntryProduct
        {
            get { return _salesReturnEntryProduct; }
            set { SetProperty(ref _salesReturnEntryProduct, value, "SalesReturnEntryProduct"); }
        }

        public int? SalesReturnEntryQuantity
        {
            get { return _salesReturnEntryQuantity; }
            set { SetProperty(ref _salesReturnEntryQuantity, value, "SalesReturnEntryQuantity"); }
        }

        public ICommand SalesReturnEntryAddCommand
        {
            get
            {
                return _salesReturnEntryAddCommand ?? (_salesReturnEntryAddCommand = new RelayCommand(() =>
                {
                    var availableReturnQuantity = _selectedSalesTransactionLine.Quantity;
                    using (var context = new ERPContext())
                    {
                        var returnedItems = context.SalesReturnTransactionLines
                        .Where(e => e.SalesReturnTransaction.SalesTransaction.SalesTransactionID
                        .Equals(Model.SalesTransaction.SalesTransactionID) && e.ItemID.Equals(_selectedSalesTransactionLine.ItemID));

                        if (returnedItems.Count() != 0)
                        {
                            foreach (var item in returnedItems)
                            {
                                availableReturnQuantity -= item.Quantity;
                            }
                        }
                    }
                    
                    if (_salesReturnEntryProduct == null)
                    {
                        MessageBox.Show("No product is selected", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (_salesReturnEntryQuantity > _selectedSalesTransactionLine.Quantity 
                    || _salesReturnEntryQuantity > availableReturnQuantity
                    || _salesReturnEntryQuantity <= 0)
                    {
                        MessageBox.Show("Please enter the right amount of quantity.", "Invalid Quantity Input", MessageBoxButton.OK);
                        return;
                    }

                    // Look if the line exists in the SalesReturnTransactionLines already
                    foreach (var line in _salesReturnTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_selectedSalesTransactionLine.ItemID))
                        {
                            if ((line.Quantity + _salesReturnEntryQuantity) > _selectedSalesTransactionLine.Quantity ||
                            (line.Quantity + _salesReturnEntryQuantity) > availableReturnQuantity ||
                            (line.Quantity + _salesReturnEntryQuantity) <= 0)
                            {
                                MessageBox.Show("Please enter the right amount of quantity.", "Invalid Quantity Input", MessageBoxButton.OK);
                                return;
                            }

                            line.Quantity += (int) _salesReturnEntryQuantity;
                            line.CostOfGoodsSold = GetSalesReturnTransactionLinePurchasePrice(line);

                            return;
                        }
                    }

                    var sr = new SalesReturnTransactionLine
                    {
                        SalesReturnTransaction = Model,
                        Item = _selectedSalesTransactionLine.Item,
                        SalesPrice = _selectedSalesTransactionLine.SalesPrice,
                        Quantity = (int)_salesReturnEntryQuantity
                    };
                    sr.CostOfGoodsSold = GetSalesReturnTransactionLinePurchasePrice(sr);
                    _salesReturnTransactionLines.Add(sr);

                }));
            }
        }

        private decimal GetSalesReturnTransactionLinePurchasePrice(SalesReturnTransactionLine sr)
        {
            using (var context = new ERPContext())
            {
                decimal amount = 0;
                var latestPurchases = context.PurchaseTransactionLines
                    .Where(e => e.ItemID.Equals(sr.Item.ItemID))
                    .OrderByDescending(e => e.PurchaseID)
                    .ToList();

                var lineQuantityTracker = sr.Quantity;
                var PiecesTracker = sr.Item.Pieces;
                bool found = false;
                for (var i = 0; i < latestPurchases.Count; i++)
                {
                    if (PiecesTracker > latestPurchases[i].Quantity)
                    {
                        PiecesTracker -= latestPurchases[i].Quantity;
                        continue;
                    }

                    if (!found)
                    {
                        found = true;
                        if ((lineQuantityTracker + PiecesTracker) <= latestPurchases[i].Quantity)
                        {
                            amount += lineQuantityTracker * latestPurchases[i].PurchasePrice;
                            break;
                        }
                        else
                        {
                            amount += (latestPurchases[i].Quantity - PiecesTracker) * latestPurchases[i].PurchasePrice;
                            lineQuantityTracker -= (latestPurchases[i].Quantity - PiecesTracker);
                        }
                        continue;
                    }

                    else
                    {
                        if (lineQuantityTracker <= latestPurchases[i].Quantity)
                        {
                            amount += latestPurchases[i].PurchasePrice * lineQuantityTracker;
                            break;
                        }
                        else
                        {
                            amount += latestPurchases[i].Quantity * latestPurchases[i].PurchasePrice;
                            lineQuantityTracker -= latestPurchases[i].Quantity;
                        }
                    }
                }
                return amount;
            }
        }

        // ----------------------------------------------------------------- //

        public ICommand NewCommand
        {
            get
            {
                return _newCommand ?? (_newCommand = new RelayCommand(() =>
                {
                    ResetTransaction();
                }));
            }
        }

        public ICommand ConfirmCommand
        {
            get
            {
                return _confirmCommand ?? (_confirmCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm transaction?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            decimal totalAmount = 0;
                            decimal totalCOGS = 0;
                            // Calcculate the total amount of Sales Return, total amount of Cost of Goods Sold
                            // ,record the transaction and increase the item's quantity in the database
                            context.SalesTransactions.Attach(Model.SalesTransaction);
                            context.SalesReturnTransactions.Add(Model);
                            foreach (var line in _salesReturnTransactionLines)
                            {
                                totalCOGS += line.CostOfGoodsSold;
                                totalAmount += line.SalesPrice * line.Quantity;
                                context.Inventory.Attach(line.Item);
                                context.SalesReturnTransactionLines.Add(line);

                                line.Item.Pieces += line.Quantity;
                                ((IObjectContextAdapter)context).ObjectContext.
                                ObjectStateManager.ChangeObjectState(line.Item, EntityState.Modified);
                            }

                            // Record the corresponding ledger transactions in the database
                            var ledgerTransaction1 = new LedgerTransaction();
                            var ledgerTransaction2 = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction1, DateTime.Now, _salesReturnEntryID, "Sales Return");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Sales Returns and Allowances", "Debit", totalAmount);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Cash", "Credit", totalAmount);

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction2, DateTime.Now, _salesReturnEntryID, "Sales Return");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Inventory", "Debit", totalCOGS);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Cost of Goods Sold", "Credit", totalCOGS);

                            context.SaveChanges();
                            ts.Complete();
                        }

                        ResetTransaction();
                        Model = new SalesReturnTransaction();
                        Model.Date = _salesReturnEntryDate;
                        SetSalesReturnTransactionID();
                    }
                }));
            }
        }

        private void ResetTransaction()
        {
            SelectedSalesTransactionID = null;
            SalesReturnEntryDate = DateTime.Now;
            SalesReturnEntryProduct = null;
            SalesReturnEntryQuantity = null;
            _salesReturnTransactionLines.Clear();
            _salesTransactionLines.Clear();
        }
    }
}
