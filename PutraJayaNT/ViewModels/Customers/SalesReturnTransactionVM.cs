﻿using MVVMFramework;
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

namespace PutraJayaNT.ViewModels.Customers
{
    class SalesReturnTransactionVM : ViewModelBase<SalesReturnTransaction>
    {
        ObservableCollection<SalesTransactionLineVM> _salesTransactionLines;
        ObservableCollection<SalesReturnTransactionLineVM> _salesReturnTransactionLines;

        string _selectedSalesTransactionID;
        SalesTransactionLineVM _selectedSalesTransactionLine;
        CustomerVM _selectedSalesTransactionCustomer;
        DateTime? _selectedSalesTransactionWhen;

        string _salesReturnEntryID;
        DateTime _salesReturnEntryDate;
        string _salesReturnEntryProduct;
        int? _salesReturnEntryUnits;
        int? _salesReturnEntryPieces;
        ICommand _salesReturnEntryAddCommand;

        ICommand _newCommand;
        ICommand _confirmCommand;

        public SalesReturnTransactionVM()
        {
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
                        _salesTransactionLines.Add(new SalesTransactionLineVM { Model = line});

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

        public ICommand SalesReturnEntryAddCommand
        {
            get
            {
                return _salesReturnEntryAddCommand ?? (_salesReturnEntryAddCommand = new RelayCommand(() =>
                {
                    if (_salesReturnEntryProduct == null)
                    {
                        MessageBox.Show("No product is selected", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    var availableReturnQuantity = GetAvailableReturnQuantity(_selectedSalesTransactionLine);
                    int quantity = ((_salesReturnEntryUnits != null ? (int) _salesReturnEntryUnits : 0) * _selectedSalesTransactionLine.Item.PiecesPerUnit) + (_salesReturnEntryPieces != null ? (int) _salesReturnEntryPieces : 0);

                    if (quantity > _selectedSalesTransactionLine.Quantity 
                    || quantity > availableReturnQuantity
                    || quantity <= 0)
                    {
                        MessageBox.Show(string.Format("The available return amount for {0} is {1} units {2} pieces.", 
                            _selectedSalesTransactionLine.Item.Name, 
                            (availableReturnQuantity / _selectedSalesTransactionLine.Item.PiecesPerUnit), 
                            (availableReturnQuantity % _selectedSalesTransactionLine.Item.PiecesPerUnit)),
                            "Invalid Quantity Input", MessageBoxButton.OK);
                        return;
                    }

                    // Look if the line exists in the SalesReturnTransactionLines already
                    foreach (var line in _salesReturnTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_selectedSalesTransactionLine.Item.ItemID))
                        {
                            if ((line.Quantity + quantity) > _selectedSalesTransactionLine.Quantity ||
                            (line.Quantity + quantity) > availableReturnQuantity ||
                            (line.Quantity + quantity) <= 0)
                            {
                                MessageBox.Show("Please enter the right amount of quantity.", "Invalid Quantity Input", MessageBoxButton.OK);
                                return;
                            }

                            line.Quantity += quantity;
                            line.CostOfGoodsSold = GetSalesReturnTransactionLineCOGS(line);

                            return;
                        }
                    }

                    var sr = new SalesReturnTransactionLine
                    {
                        SalesReturnTransaction = Model,
                        Item = _selectedSalesTransactionLine.Item,
                        Warehouse = _selectedSalesTransactionLine.Warehouse,
                        SalesPrice = _selectedSalesTransactionLine.SalesPrice / _selectedSalesTransactionLine.Item.PiecesPerUnit,
                        Quantity = quantity
                    };
                    var vm = new SalesReturnTransactionLineVM { Model = sr };
                    sr.CostOfGoodsSold = GetSalesReturnTransactionLineCOGS(vm);
                    _salesReturnTransactionLines.Add(vm);
                }));
            }
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

            return availableReturnQuantity;
        }

        private decimal GetSalesReturnTransactionLineCOGS(SalesReturnTransactionLineVM sr)
        {
            using (var context = new ERPContext())
            {
                decimal amount = 0;
                var purchases = context.PurchaseTransactionLines
                    .Where(e => e.ItemID.Equals(sr.Item.ItemID) && e.SoldOrReturned > 0)
                    .OrderByDescending(e => e.PurchaseTransactionID)
                    .ToList();

                var tracker = sr.Quantity;
                foreach (var purchase in purchases)
                {
                    if (purchase.SoldOrReturned >= tracker)
                    {
                        amount += purchase.PurchasePrice * tracker;
                        // remember to change at save transaction instead purchase.SoldOrReturned -= tracker;
                        break;
                    }
                    else if (purchase.SoldOrReturned < tracker)
                    {
                        amount += purchase.PurchasePrice * purchase.SoldOrReturned;
                        // remember to change at save transaction instead purchase.SoldOrReturned = 0;
                        tracker -= purchase.SoldOrReturned;
                    }
                }

                return amount;
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

                            decimal totalAmount = 0;
                            decimal totalCOGS = 0;
                            // Calcculate the total amount of Sales Return, total amount of Cost of Goods Sold
                            // ,record the transaction and increase the item's quantity in the database
                            Model.Date = DateTime.Now.Date;
                            context.SalesTransactions.Attach(Model.SalesTransaction);
                            context.SalesReturnTransactions.Add(Model);
                            foreach (var line in _salesReturnTransactionLines)
                            {
                                var item = context.Inventory.Where(e => e.ItemID.Equals(line.Item.ItemID)).FirstOrDefault();
                                var warehouse = context.Warehouses.Where(e => e.ID.Equals(line.Warehouse.ID)).FirstOrDefault();

                                totalCOGS += line.CostOfGoodsSold;
                                totalAmount += (line.SalesPrice / line.Item.PiecesPerUnit)  * line.Quantity;
                                line.Item = item;
                                line.Warehouse = warehouse;
                                context.SalesReturnTransactionLines.Add(line.Model);

                                // Decrease SoldOrReturned for affected purchase lines
                                decimal amount = 0;
                                var purchases = context.PurchaseTransactionLines
                                    .Where(e => e.ItemID.Equals(line.Item.ItemID) && e.SoldOrReturned > 0)
                                    .OrderByDescending(e => e.PurchaseTransactionID)
                                    .ToList();

                                var tracker = line.Quantity;
                                foreach (var purchase in purchases)
                                {
                                    if (purchase.SoldOrReturned >= tracker)
                                    {
                                        amount += purchase.PurchasePrice * tracker;
                                        purchase.SoldOrReturned -= tracker;
                                        break;
                                    }
                                    else if (purchase.SoldOrReturned < tracker)
                                    {
                                        amount += purchase.PurchasePrice * purchase.SoldOrReturned;
                                        tracker -= purchase.SoldOrReturned;
                                        purchase.SoldOrReturned = 0;
                                    }
                                }

                                // Increase the Customer's Sales Return Credits
                                var customer = context.Customers.Where(e => e.ID.Equals(Model.SalesTransaction.Customer.ID)).FirstOrDefault();
                                customer.SalesReturnCredits += totalAmount;

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
                                }
                            }

                            // Record the corresponding ledger transactions in the database
                            var ledgerTransaction1 = new LedgerTransaction();
                            var ledgerTransaction2 = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction1, DateTime.Now, _salesReturnEntryID, "Sales Return");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Sales Returns and Allowances", "Debit", totalAmount);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, string.Format("{0} Accounts Receivable", Model.SalesTransaction.Customer.Name), "Credit", totalAmount);

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction2, DateTime.Now, _salesReturnEntryID, "Sales Return");
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

        private void ResetTransaction()
        {
            Model.SalesTransaction = null;
            SelectedSalesTransactionID = null;
            SelectedSalesTransactionWhen = null;
            SalesReturnEntryDate = DateTime.Now;
            SalesReturnEntryProduct = null;
            SalesReturnEntryUnits = null;
            SalesReturnEntryPieces = null;
            _salesReturnTransactionLines.Clear();
            _salesTransactionLines.Clear();
        }
    }
}
