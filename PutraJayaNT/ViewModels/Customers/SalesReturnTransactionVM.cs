using MVVMFramework;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using PutraJayaNT.Models.Inventory;
using System.Data.Entity.Infrastructure;
using PutraJayaNT.Models;
using PutraJayaNT.Reports;
using PutraJayaNT.Reports.Windows;

namespace PutraJayaNT.ViewModels.Customers
{
    public class SalesReturnTransactionVM : ViewModelBase<SalesReturnTransaction>
    {
        ObservableCollection<SalesTransactionLineVM> _salesTransactionLines;
        ObservableCollection<SalesReturnTransactionLineVM> _salesReturnTransactionLines;

        bool _notEditing;

        string _salesReturnTransactionID;
        decimal _salesReturnTransactionNetTotal;

        string _selectedSalesTransactionID;
        SalesTransactionLineVM _selectedSalesTransactionLine;
        CustomerVM _selectedSalesTransactionCustomer;
        DateTime? _selectedSalesTransactionWhen;

        DateTime _salesReturnEntryDate;
        string _salesReturnEntryProduct;
        int? _salesReturnEntryUnits;
        int? _salesReturnEntryPieces;
        decimal? _salesReturnEntryPrice;
        ICommand _salesReturnEntryAddCommand;

        ICommand _newCommand;
        ICommand _printCommand;
        ICommand _confirmCommand;

        public SalesReturnTransactionVM()
        {
            _notEditing = true;

            Model = new SalesReturnTransaction();

            _salesTransactionLines = new ObservableCollection<SalesTransactionLineVM>();
            _salesReturnTransactionLines = new ObservableCollection<SalesReturnTransactionLineVM>();

            _salesReturnEntryDate = DateTime.Now.Date;

            SetSalesReturnTransactionID();
        }

        public ObservableCollection<SalesTransactionLineVM> SalesTransactionLines
        {
            get { return _salesTransactionLines; }
        }

        public ObservableCollection<SalesReturnTransactionLineVM> SalesReturnTransactionLines
        {
            get { return _salesReturnTransactionLines; }
        }

        public bool NotEditing
        {
            get { return _notEditing; }
            set { SetProperty(ref _notEditing, value, "NotEditing"); }
        }

        #region Sales Return Transaction Properties
        public string SalesReturnTransactionID
        {
            get { return _salesReturnTransactionID; }
            set
            {
                using (var context = new ERPContext())
                {
                    var sr = context.SalesReturnTransactions
                        .Include("TransactionLines")
                        .Include("TransactionLines.Warehouse")
                        .Include("TransactionLines.Item")
                        .Where(e => e.SalesReturnTransactionID.Equals(value))
                        .FirstOrDefault();

                    if (sr == null)
                    {
                        MessageBox.Show("Transaction does not exists.", "Invalid Transaction ID", MessageBoxButton.OK);
                        return;
                    }

                    Model = sr;
                    SelectedSalesTransactionID = Model.SalesTransaction.SalesTransactionID;
                
                    foreach (var line in Model.TransactionLines)
                        _salesReturnTransactionLines.Add(new SalesReturnTransactionLineVM { Model = line });

                    OnPropertyChanged("SalesReturnTransactionNetTotal");
                }

                SetProperty(ref _salesReturnTransactionID, value, "SalesReturnTransactionID");
                NotEditing = false;
            }
        }

        public decimal SalesReturnTransactionNetTotal
        {
            get
            {
                _salesReturnTransactionNetTotal = 0;
                foreach (var line in _salesReturnTransactionLines)
                    _salesReturnTransactionNetTotal += line.Total;
                return _salesReturnTransactionNetTotal;
            }
            set { SetProperty(ref _salesReturnTransactionNetTotal, value, "SalesReturnTransactionNetTotal"); }
        }
        #endregion

        #region Selected Sales Transaction Properties
        public string SelectedSalesTransactionID
        {
            get { return _selectedSalesTransactionID; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedSalesTransactionID, value, "SelectedSalesTransactionID");
                    return;
                }

                if (UpdateSalesTransactionLines(value))
                {
                    SetProperty(ref _selectedSalesTransactionID, value, "SelectedSalesTransactionID");
                    SelectedSalesTransactionWhen = _salesTransactionLines.FirstOrDefault().SalesTransaction.When;
                    Model.SalesTransaction = _salesTransactionLines.FirstOrDefault().SalesTransaction;
                    SelectedSalesTransactionCustomer = new CustomerVM { Model = Model.SalesTransaction.Customer };
                    _salesReturnTransactionLines.Clear();
                    SelectedSalesTransactionLine = null;
                }
                else
                {
                    MessageBox.Show("Please check if the transaction exists or if invoice has been issued.", "Invalid Sales Transaction", MessageBoxButton.OK);
                }
            }
        }

        public CustomerVM SelectedSalesTransactionCustomer
        {
            get { return _selectedSalesTransactionCustomer; }
            set { SetProperty(ref _selectedSalesTransactionCustomer, value, "SelectedSalesTransactionCustomer"); }
        }

        public DateTime? SelectedSalesTransactionWhen
        {
            get { return _selectedSalesTransactionWhen; }
            set { SetProperty(ref _selectedSalesTransactionWhen, value, "SelectedSalesTransactionWhen"); }
        }

        public SalesTransactionLineVM SelectedSalesTransactionLine
        {
            get { return _selectedSalesTransactionLine; }
            set
            {
                SetProperty(ref _selectedSalesTransactionLine, value, "SelectedSalesTransactionLine");
                if (_selectedSalesTransactionLine != null)
                    UpdateReturnEntryProperties();
            }
        }
        #endregion

        #region Return Entry Properties
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

        public int? SalesReturnEntryUnits
        {
            get { return _salesReturnEntryUnits; }
            set { SetProperty(ref _salesReturnEntryUnits, value, "SalesReturnEntryUnits"); }
        }

        public int? SalesReturnEntryPieces
        {
            get { return _salesReturnEntryPieces; }
            set { SetProperty(ref _salesReturnEntryPieces, value, "SalesReturnEntryPieces"); }
        }

        public decimal? SalesReturnEntryPrice
        {
            get { return _salesReturnEntryPrice; }
            set { SetProperty(ref _salesReturnEntryPrice, value, "SalesReturnEntryPrice"); }
        }

        public ICommand SalesReturnEntryAddCommand
        {
            get
            {
                return _salesReturnEntryAddCommand ?? (_salesReturnEntryAddCommand = new RelayCommand(() =>
                {
                    if (_salesReturnEntryProduct == null || _selectedSalesTransactionLine == null)
                    {
                        MessageBox.Show("No product is selected", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    if (_salesReturnEntryPrice == null || _salesReturnEntryPrice < 0)
                    {
                        MessageBox.Show("Please enter a valid return price.",
                            "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    var availableReturnQuantity = GetAvailableReturnQuantity(_selectedSalesTransactionLine);
                    int quantity = ((_salesReturnEntryUnits != null ? (int) _salesReturnEntryUnits : 0) * _selectedSalesTransactionLine.Item.PiecesPerUnit) + (_salesReturnEntryPieces != null ? (int) _salesReturnEntryPieces : 0);

                    if (quantity > availableReturnQuantity
                    || quantity <= 0)
                    {
                        MessageBox.Show(string.Format("The available return amount for {0} is {1} units {2} pieces.", 
                            _selectedSalesTransactionLine.Item.Name, 
                            (availableReturnQuantity / _selectedSalesTransactionLine.Item.PiecesPerUnit), 
                            (availableReturnQuantity % _selectedSalesTransactionLine.Item.PiecesPerUnit)),
                            "Invalid Quantity Input", MessageBoxButton.OK);
                        return;
                    }

                    // Check if the line already exists in the SalesReturnTransactionLines
                    foreach (var line in _salesReturnTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_selectedSalesTransactionLine.Item.ItemID) &&
                           line.Warehouse.ID.Equals(_selectedSalesTransactionLine.Warehouse.ID) &&
                           line.SalesPrice.Equals(_selectedSalesTransactionLine.SalesPrice) &&
                           line.Discount.Equals(_selectedSalesTransactionLine.Discount) &&
                           line.ReturnPrice.Equals(_salesReturnEntryPrice))
                        {
                            if ((line.Quantity + quantity) > availableReturnQuantity ||
                            (line.Quantity + quantity) <= 0)
                            {
                                MessageBox.Show("Please enter the right amount of quantity.", "Invalid Quantity Input", MessageBoxButton.OK);
                                return;
                            }

                            line.Quantity += quantity;
                            line.ReturnPrice = (decimal) _salesReturnEntryPrice;
                            line.UpdateTotal();
                            line.CostOfGoodsSold = GetSalesReturnTransactionLineCOGS(line);
                            OnPropertyChanged("SalesReturnTransactionNetTotal");
                            return;
                        }
                    }

                    var sr = new SalesReturnTransactionLine
                    {
                        SalesReturnTransaction = Model,
                        Item = _selectedSalesTransactionLine.Item,
                        Warehouse = _selectedSalesTransactionLine.Warehouse,
                        SalesPrice = _selectedSalesTransactionLine.SalesPrice / _selectedSalesTransactionLine.Item.PiecesPerUnit,
                        Discount = _selectedSalesTransactionLine.Discount / _selectedSalesTransactionLine.Item.PiecesPerUnit,
                        ReturnPrice = (decimal) _salesReturnEntryPrice / _selectedSalesTransactionLine.Item.PiecesPerUnit,
                        Quantity = quantity
                    };
                    var vm = new SalesReturnTransactionLineVM { Model = sr };
                    vm.UpdateTotal();
                    sr.CostOfGoodsSold = GetSalesReturnTransactionLineCOGS(vm);
                    _salesReturnTransactionLines.Add(vm);

                    OnPropertyChanged("SalesReturnTransactionNetTotal");
                    SalesReturnEntryPrice = null;
                }));
            }
        }
        #endregion

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

                            decimal totalCOGS = 0;
                            // Calcculate the total amount of Sales Return, total amount of Cost of Goods Sold
                            // ,record the transaction and increase the item's quantity in the database
                            Model.Date = DateTime.Now.Date;
                            Model.NetTotal = _salesReturnTransactionNetTotal;
                            var user = App.Current.FindResource("CurrentUser") as User;
                            Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                            context.SalesTransactions.Attach(Model.SalesTransaction);
                            context.SalesReturnTransactions.Add(Model);
                            foreach (var line in _salesReturnTransactionLines)
                            {
                                var item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                                var warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();

                                totalCOGS += line.CostOfGoodsSold;
                                line.UpdateTotal();
                                line.Item = item;
                                line.Warehouse = warehouse;
                                context.SalesReturnTransactionLines.Add(line.Model);

                                // Decrease SoldOrReturned for affected purchase lines
                                var purchases = context.PurchaseTransactionLines
                                    .Where(e => e.ItemID.Equals(line.Item.ItemID) && e.SoldOrReturned > 0)
                                    .OrderByDescending(e => e.PurchaseTransactionID)
                                    .ToList();

                                var tracker = line.Quantity;
                                foreach (var purchase in purchases)
                                {
                                    if (purchase.SoldOrReturned >= tracker)
                                    {
                                        purchase.SoldOrReturned -= tracker;
                                        break;
                                    }
                                    else if (purchase.SoldOrReturned < tracker)
                                    {
                                        tracker -= purchase.SoldOrReturned;
                                        purchase.SoldOrReturned = 0;
                                    }
                                }

                                // Increase the Customer's Sales Return Credits
                                var customer = context.Customers.Where(e => e.ID.Equals(Model.SalesTransaction.Customer.ID)).FirstOrDefault();
                                customer.SalesReturnCredits += _salesReturnTransactionNetTotal;

                                // Increase the stock
                                var stock = context.Stocks
                                .Where(e => e.Item.ItemID.Equals(line.Item.ItemID) && e.Warehouse.ID.Equals(line.Warehouse.ID))
                                .FirstOrDefault();

                                if (stock != null) stock.Pieces += line.Quantity;

                                else
                                {
                                    var s = new Stock {
                                        Item = item,
                                        Warehouse = warehouse,
                                        Pieces = line.Quantity
                                    };

                                    context.Stocks.Add(s);
                                    context.SaveChanges();
                                }
                            }

                            // Record the corresponding ledger transactions in the database
                            var ledgerTransaction1 = new LedgerTransaction();
                            var ledgerTransaction2 = new LedgerTransaction();

                            if (!LedgerDBHelper.AddTransaction(context, ledgerTransaction1, DateTime.Now.Date, _salesReturnTransactionID, "Sales Return")) return;
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Sales Returns and Allowances", "Debit", _salesReturnTransactionNetTotal);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, string.Format("{0} Accounts Receivable", Model.SalesTransaction.Customer.Name), "Credit", _salesReturnTransactionNetTotal);

                            if (!LedgerDBHelper.AddTransaction(context, ledgerTransaction2, DateTime.Now.Date, _salesReturnTransactionID, "Sales Return")) return;
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Inventory", "Debit", totalCOGS);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction2, "Cost of Goods Sold", "Credit", totalCOGS);

                            context.SaveChanges();

                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(Model.SalesTransaction, EntityState.Detached);

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

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (_salesReturnTransactionLines.Count == 0) return;

                    var salesReturnInvoiceWindow = new SalesReturnInvoiceWindow(this);
                    salesReturnInvoiceWindow.Owner = App.Current.MainWindow;
                    salesReturnInvoiceWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    salesReturnInvoiceWindow.Show();
                }));
            }
        }

        #region Helper Methods
        private bool UpdateSalesTransactionLines(string salesTransactionID)
        {
            using (var context = new ERPContext())
            {
                var transaction = context.SalesTransactions
                    .Include("TransactionLines")
                    .Include("TransactionLines.Item")
                    .Include("TransactionLines.Warehouse")
                    .Include("Customer")
                    .Where(e => e.SalesTransactionID.Equals(salesTransactionID) && e.InvoiceIssued != null)
                    .FirstOrDefault();

                if (transaction == null) return false;

                else
                {
                    _salesTransactionLines.Clear();

                    foreach (var line in transaction.TransactionLines.ToList())
                        _salesTransactionLines.Add(new SalesTransactionLineVM { Model = line });

                    return true;
                }
            }
        }

        private void UpdateReturnEntryProperties()
        {
            SalesReturnEntryProduct = _selectedSalesTransactionLine.Item.Name;
            var availableReturnQuantity = GetAvailableReturnQuantity(_selectedSalesTransactionLine);
            SalesReturnEntryUnits = availableReturnQuantity / _selectedSalesTransactionLine.Item.PiecesPerUnit;
            SalesReturnEntryPieces = availableReturnQuantity % _selectedSalesTransactionLine.Item.PiecesPerUnit;
        }

        private void SetSalesReturnTransactionID()
        {
            var year = _salesReturnEntryDate.Year;
            var month = _salesReturnEntryDate.Month;

            var newEntryID = "MR" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastEntryID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from SalesReturnTransaction in context.SalesReturnTransactions
                           where SalesReturnTransaction.SalesReturnTransactionID.CompareTo(newEntryID.ToString()) >= 0
                           orderby SalesReturnTransaction.SalesReturnTransactionID descending
                           select SalesReturnTransaction.SalesReturnTransactionID);
                if (IDs.Count() != 0) lastEntryID = IDs.First();
            }

            if (lastEntryID != null) newEntryID = "MR" + (Convert.ToInt64(lastEntryID.Substring(2)) + 1).ToString();

            Model.SalesReturnTransactionID = newEntryID;
            _salesReturnTransactionID = newEntryID;
            OnPropertyChanged("SalesReturnTransactionID");
        }

        private int GetAvailableReturnQuantity(SalesTransactionLineVM line)
        {
            var availableReturnQuantity = line.Quantity;

            using (var context = new ERPContext())
            {
                var returnedItems = context.SalesReturnTransactionLines
                .Where(e => e.SalesReturnTransaction.SalesTransaction.SalesTransactionID
                .Equals(Model.SalesTransaction.SalesTransactionID) && e.ItemID.Equals(_selectedSalesTransactionLine.Item.ItemID));

                if (returnedItems.Count() != 0)
                {
                    foreach (var item in returnedItems)
                    {
                        availableReturnQuantity -= item.Quantity;
                    }
                }
            }

            foreach (var l in _salesReturnTransactionLines)
            {
                if (l.Item.ItemID.Equals(_selectedSalesTransactionLine.Item.ItemID) &&
                    l.Warehouse.ID.Equals(_selectedSalesTransactionLine.Warehouse.ID) &&
                    l.SalesPrice.Equals(_selectedSalesTransactionLine.SalesPrice) &&
                    l.Discount.Equals(_selectedSalesTransactionLine.Discount))
                {
                    availableReturnQuantity -= l.Quantity;
                }
            }

            return availableReturnQuantity;
        }

        private decimal GetSalesReturnTransactionLineCOGS(SalesReturnTransactionLineVM sr)
        {
            using (var context = new ERPContext())
            {
                decimal amount = 0;
                var purchases = context.PurchaseTransactionLines
                    .Include("PurchaseTransaction")
                    .Where(e => e.ItemID.Equals(sr.Item.ItemID) && e.SoldOrReturned > 0)
                    .OrderByDescending(e => e.PurchaseTransactionID)
                    .ToList();

                var tracker = sr.Quantity;
                foreach (var purchase in purchases)
                {
                    var purchaseLineTotal = purchase.PurchasePrice - purchase.Discount;

                    if (purchase.SoldOrReturned >= tracker)
                    {
                        if (purchaseLineTotal == 0) break;
                        var fractionOfTransactionDiscount = (tracker * purchaseLineTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                        var fractionOfTransactionTax = (tracker * purchaseLineTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                        amount += (tracker * purchaseLineTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                        break;
                    }
                    else if (purchase.SoldOrReturned < tracker)
                    {
                        tracker -= purchase.SoldOrReturned;
                        if (purchaseLineTotal == 0) continue;
                        var fractionOfTransactionDiscount = (purchase.SoldOrReturned * purchaseLineTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Discount;
                        var fractionOfTransactionTax = (purchase.SoldOrReturned * purchaseLineTotal / purchase.PurchaseTransaction.GrossTotal) * purchase.PurchaseTransaction.Tax;
                        amount += (purchase.SoldOrReturned * purchaseLineTotal) - fractionOfTransactionDiscount + fractionOfTransactionTax;
                    }
                }

                return amount;
            }
        }

        private void ResetTransaction()
        {
            _salesReturnTransactionLines.Clear();
            _salesTransactionLines.Clear();

            NotEditing = true;
            Model.SalesTransaction = null;

            SelectedSalesTransactionID = null;
            SelectedSalesTransactionWhen = null;
            SelectedSalesTransactionCustomer = null;
            SelectedSalesTransactionLine = null;

            SalesReturnEntryDate = DateTime.Now;
            SalesReturnEntryProduct = null;
            SalesReturnEntryUnits = null;
            SalesReturnEntryPieces = null;

            SalesReturnTransactionNetTotal = 0;
        }
        #endregion
    }
}
