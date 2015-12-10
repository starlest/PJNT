using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PurchaseReturnTransactionVM : ViewModelBase<PurchaseReturnTransaction>
    {
        ObservableCollection<PurchaseTransactionLineVM> _purchaseTransactionLines;
        ObservableCollection<PurchaseReturnTransactionLineVM> _purchaseReturnTransactionLines;

        string _selectedPurchaseTransactionID;
        PurchaseTransactionLineVM _selectedPurchaseTransactionLine;
        DateTime? _selectedPurchaseTransactionWhen;

        string _purchaseReturnEntryID;
        DateTime _purchaseReturnEntryDate;
        string _purchaseReturnEntryProduct;
        int? _purchaseReturnEntryQuantity;
        int? _purchaseReturnEntryUnits;
        int? _purchaseReturnEntryPieces;
        ICommand _purchaseReturnEntryAddCommand;

        ICommand _newCommand;
        ICommand _confirmCommand;

        public PurchaseReturnTransactionVM()
        {
            Model = new PurchaseReturnTransaction();

            _purchaseTransactionLines = new ObservableCollection<PurchaseTransactionLineVM>();
            _purchaseReturnTransactionLines = new ObservableCollection<PurchaseReturnTransactionLineVM>();

            _purchaseReturnEntryDate = DateTime.Now.Date;

            SetPurchaseReturnTransactionID();
        }

        public ObservableCollection<PurchaseTransactionLineVM> PurchaseTransactionLines
        {
            get { return _purchaseTransactionLines; }
        }

        public ObservableCollection<PurchaseReturnTransactionLineVM> PurchaseReturnTransactionLines
        {
            get { return _purchaseReturnTransactionLines; }
        }

        public string SelectedPurchaseTransactionID
        {
            get { return _selectedPurchaseTransactionID; }
            set
            {
                SetProperty(ref _selectedPurchaseTransactionID, value, "SelectedPurchaseTransactionID");

                if (_selectedPurchaseTransactionID == null) return;

                if (UpdatePurchaseTransactionLines())
                {
                    SelectedPurchaseTransactionWhen = _purchaseTransactionLines.FirstOrDefault().PurchaseTransaction.Date;
                    Model.PurchaseTransaction = _purchaseTransactionLines.FirstOrDefault().PurchaseTransaction;
                }
                else
                    SelectedPurchaseTransactionWhen = null;
            }
        }

        public DateTime? SelectedPurchaseTransactionWhen
        {
            get { return _selectedPurchaseTransactionWhen; }
            set { SetProperty(ref _selectedPurchaseTransactionWhen, value, "SelectedPurchaseTransactionWhen"); }
        }

        public PurchaseTransactionLineVM SelectedPurchaseTransactionLine
        {
            get { return _selectedPurchaseTransactionLine; }
            set
            {
                SetProperty(ref _selectedPurchaseTransactionLine, value, "SelectedPurchaseTransactionLine");
                if (_selectedPurchaseTransactionLine != null)
                    UpdateReturnEntryProperties();
            }
        }

        private bool UpdatePurchaseTransactionLines()
        {
            var found = false;

            _purchaseTransactionLines.Clear();

            using (var context = new ERPContext())
            {
                var lines = context.PurchaseTransactionLines
                    .Where(e => e.PurchaseTransactionID.Equals(_selectedPurchaseTransactionID))
                    .Include("PurchaseTransaction")
                    .Include("Warehouse")
                    .Include("PurchaseTransaction.Supplier")
                    .Include("Item")
                    .Include("Item.Stocks")
                    .ToList();

                if (lines.Count > 0) found = true;

                foreach (var line in lines)
                    _purchaseTransactionLines.Add(new PurchaseTransactionLineVM { Model = line });
            }

            return found;
        }

        private void UpdateReturnEntryProperties()
        {
            PurchaseReturnEntryProduct = _selectedPurchaseTransactionLine.Item.Name;
            PurchaseReturnEntryUnits = _selectedPurchaseTransactionLine.Units;
            PurchaseReturnEntryPieces = _selectedPurchaseTransactionLine.Pieces;
        }

        private void SetPurchaseReturnTransactionID()
        {

            var year = _purchaseReturnEntryDate.Year;
            var month = _purchaseReturnEntryDate.Month;

            var newEntryID = "PR" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastEntryID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from PurchaseReturnTransaction in context.PurchaseReturnTransactions
                           where PurchaseReturnTransaction.PurchaseReturnTransactionID.CompareTo(newEntryID.ToString()) >= 0
                           orderby PurchaseReturnTransaction.PurchaseReturnTransactionID descending
                           select PurchaseReturnTransaction.PurchaseReturnTransactionID);
                if (IDs.Count() != 0) lastEntryID = IDs.First();
            }

            if (lastEntryID != null) newEntryID = "PR" + (Convert.ToInt64(lastEntryID.Substring(2)) + 1).ToString();

            Model.PurchaseReturnTransactionID = newEntryID;
            _purchaseReturnEntryID = newEntryID;
        }

        #region Return Entry Properties
        public DateTime PurchaseReturnEntryDate
        {
            get { return _purchaseReturnEntryDate; }
            set
            {
                SetProperty(ref _purchaseReturnEntryDate, value, "PurchaseReturnEntryDate");
                SetPurchaseReturnTransactionID();
            }
        }

        public string PurchaseReturnEntryProduct
        {
            get { return _purchaseReturnEntryProduct; }
            set { SetProperty(ref _purchaseReturnEntryProduct, value, "PurchaseReturnEntryProduct"); }
        }

        public int? PurchaseReturnEntryUnits
        {
            get { return _purchaseReturnEntryUnits; }
            set { SetProperty(ref _purchaseReturnEntryUnits, value, "PurchaseReturnEntryUnits"); }
        }

        public int? PurchaseReturnEntryPieces
        {
            get { return _purchaseReturnEntryPieces; }
            set { SetProperty(ref _purchaseReturnEntryPieces, value, "PurchaseReturnEntryPieces"); }
        }

        public ICommand PurchaseReturnEntryAddCommand
        {
            get
            {
                return _purchaseReturnEntryAddCommand ?? (_purchaseReturnEntryAddCommand = new RelayCommand(() =>
                {
                    var availableReturnQuantity = _selectedPurchaseTransactionLine.Quantity;
                    var availablePieces = GetStock(_selectedPurchaseTransactionLine.Item, _selectedPurchaseTransactionLine.Warehouse);

                    using (var context = new ERPContext())
                    {
                        var returnedItems = context.PurchaseReturnTransactionLines
                        .Where(e => e.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID
                        .Equals(Model.PurchaseTransaction.PurchaseID) && e.ItemID.Equals(_selectedPurchaseTransactionLine.Item.ItemID));

                        if (returnedItems.Count() != 0)
                        {
                            foreach (var item in returnedItems)
                            {
                                availableReturnQuantity -= item.Quantity;
                            }
                        }
                    }

                    if (_purchaseReturnEntryProduct == null)
                    {
                        MessageBox.Show("No product is selected", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    _purchaseReturnEntryQuantity = (int) ((_purchaseReturnEntryUnits != null ? _purchaseReturnEntryUnits : 0) * _selectedPurchaseTransactionLine.Item.PiecesPerUnit) 
                    + (_purchaseReturnEntryPieces != null ? _purchaseReturnEntryPieces : 0);

                    if (_purchaseReturnEntryQuantity > _selectedPurchaseTransactionLine.Quantity
                    || _purchaseReturnEntryQuantity > availableReturnQuantity
                    || _purchaseReturnEntryQuantity > availablePieces
                    || _purchaseReturnEntryQuantity <= 0)
                    {
                        MessageBox.Show(string.Format("The valid return quantity is {0} units {1} pieces",
                            availableReturnQuantity / _selectedPurchaseTransactionLine.Item.PiecesPerUnit,
                            availableReturnQuantity %  _selectedPurchaseTransactionLine.Item.PiecesPerUnit),
                            "Invalid Quantity Input", MessageBoxButton.OK);
                        return;
                    }

                    // Look if the line exists in the PurchaseReturnTransactionLines already
                    foreach (var line in _purchaseReturnTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_selectedPurchaseTransactionLine.Item.ItemID))
                        {
                            if ((line.Quantity + _purchaseReturnEntryQuantity) > _selectedPurchaseTransactionLine.Quantity ||
                            (line.Quantity + _purchaseReturnEntryQuantity) > availableReturnQuantity ||
                            (line.Quantity + _purchaseReturnEntryQuantity) > availablePieces ||
                            (line.Quantity + _purchaseReturnEntryQuantity) <= 0)
                            {
                                MessageBox.Show(string.Format("The valid return quantity is {0} units {1} pieces",
                                    availableReturnQuantity / _selectedPurchaseTransactionLine.Item.PiecesPerUnit,
                                    availableReturnQuantity % _selectedPurchaseTransactionLine.Item.PiecesPerUnit),
                                    "Invalid Quantity Input", MessageBoxButton.OK);
                                return;
                            }

                            line.Quantity += (int)_purchaseReturnEntryQuantity;

                            return;
                        }
                    }

                    var pr = new PurchaseReturnTransactionLine
                    {
                        PurchaseReturnTransaction = Model,
                        Item = _selectedPurchaseTransactionLine.Item,
                        Warehouse = _selectedPurchaseTransactionLine.Warehouse,
                        PurchasePrice = _selectedPurchaseTransactionLine.PurchasePrice / _selectedPurchaseTransactionLine.Item.PiecesPerUnit,
                        Quantity = (int)_purchaseReturnEntryQuantity
                    };

                    _purchaseReturnTransactionLines.Add(new PurchaseReturnTransactionLineVM { Model = pr, AvailableReturnQuantity = availableReturnQuantity });

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
                        Model.Date = (DateTime)_selectedPurchaseTransactionWhen;

                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            var purchaseTransaction = context
                            .PurchaseTransactions
                            .Include("Supplier")
                            .Where(e => e.PurchaseID.Equals(Model.PurchaseTransaction.PurchaseID))
                            .FirstOrDefault();

                            Model.PurchaseTransaction = purchaseTransaction;

                            context.PurchaseReturnTransactions.Add(Model);

                            decimal totalAmount = 0;
                            // Calcculate the total amount of Purchase Return
                            // ,record the transaction and decrease the item's quantity in the database
                            foreach (var line in _purchaseReturnTransactionLines)
                            {
                                var stock = context.Stocks
                                .Where(e => e.Item.ItemID.Equals(line.Item.ItemID) &&
                                e.Warehouse.ID.Equals(line.Warehouse.ID))
                                .FirstOrDefault();

                                // Check if there is enough stock for this return line
                                if (stock.Pieces < line.Quantity)
                                {
                                    MessageBox.Show(string.Format("{0} has only {1} units {2} pieces left.", 
                                        line.Item.Name, stock.Pieces / line.Item.PiecesPerUnit, stock.Pieces % line.Item.PiecesPerUnit), 
                                        "Invalid Quantity", MessageBoxButton.OK);
                                    return;
                                }

                                totalAmount += (line.PurchasePrice /line.Item.PiecesPerUnit) * line.Quantity;

                                line.Item = context.Inventory
                                .Where(e => e.ItemID.Equals(line.Item.ItemID))
                                .FirstOrDefault();

                                line.Warehouse = context.Warehouses
                                .Where(e => e.ID.Equals(line.Warehouse.ID))
                                .FirstOrDefault();

                                context.PurchaseReturnTransactionLines.Add(line.Model);

                                // Decrease the returned quantity from stock
                                stock.Pieces -= line.Quantity;
                            }

                            Model.PurchaseTransaction.Supplier.PurchaseReturnCredits += totalAmount;

                            // Record the corresponding ledger transactions in the database
                            var ledgerTransaction1 = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction1, DateTime.Now, _purchaseReturnEntryID, "Purchase Return");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, string.Format("{0} Accounts Payable", Model.PurchaseTransaction.Supplier.Name), "Dedit", totalAmount);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Inventory", "Crebit", totalAmount);
              
                            context.SaveChanges();
                            ts.Complete();
                        }

                        ResetTransaction();
                        Model = new PurchaseReturnTransaction();
                        Model.Date = _purchaseReturnEntryDate;
                        SetPurchaseReturnTransactionID();
                    }
                }));
            }
        }

        private void ResetTransaction()
        {
            SelectedPurchaseTransactionID = null;
            PurchaseReturnEntryDate = DateTime.Now;
            PurchaseReturnEntryProduct = null;
            PurchaseReturnEntryUnits = null;
            PurchaseReturnEntryPieces = null;
            _purchaseReturnTransactionLines.Clear();
            _purchaseTransactionLines.Clear();
        }

        private int GetStock(Item item, Warehouse warehouse)
        {
            int s = 0;
            using (var context = new ERPContext())
            {
                var stock = context.Stocks.Where(e => e.ItemID.Equals(item.ItemID) && e.WarehouseID.Equals(warehouse.ID)).FirstOrDefault();
                s = stock.Pieces;
            }
            return s;
        }
    }
}
