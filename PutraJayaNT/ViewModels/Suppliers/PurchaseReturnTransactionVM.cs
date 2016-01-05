using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Purchase;
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
        ObservableCollection<WarehouseVM> _warehouses;

        decimal _purchaseReturnTransactionNetTotal;

        string _selectedPurchaseTransactionID;
        PurchaseTransactionLineVM _selectedPurchaseTransactionLine;
        DateTime? _selectedPurchaseTransactionWhen;

        string _purchaseReturnEntryID;
        DateTime _purchaseReturnEntryDate;
        WarehouseVM _purchaseReturnEntryWarehouse;
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
            _warehouses = new ObservableCollection<WarehouseVM>();

            UpdateWarehouses();

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

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }

        #region Purchase Return Transaction Properties
        public decimal PurchaseReturnTransactionNetTotal
        {
            get
            {
                _purchaseReturnTransactionNetTotal = 0;
                foreach (var line in _purchaseReturnTransactionLines)
                {
                    _purchaseReturnTransactionNetTotal += line.Total;
                }
                return _purchaseReturnTransactionNetTotal;
            }
            set { SetProperty(ref _purchaseReturnTransactionNetTotal, value, "PurchaseReturnTransactionNetTotal"); }
        }
        #endregion

        #region Selected Purchase Transaction Properties
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
                    _purchaseReturnTransactionLines.Clear();
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
        #endregion

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

        public WarehouseVM PurchaseReturnEntryWarehouse
        {
            get { return _purchaseReturnEntryWarehouse; }
            set
            {
                SetProperty(ref _purchaseReturnEntryWarehouse, value, "PurchaseReturnEntryWarehouse");
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
                    if (_purchaseReturnEntryProduct == null || _selectedPurchaseTransactionLine == null)
                    {
                        MessageBox.Show("No product is selected", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    var availableReturnQuantity = GetAvailableReturnQuantity();

                    _purchaseReturnEntryQuantity = (int)((_purchaseReturnEntryUnits != null ? _purchaseReturnEntryUnits : 0) * _selectedPurchaseTransactionLine.Item.PiecesPerUnit)
                    + (_purchaseReturnEntryPieces != null ? _purchaseReturnEntryPieces : 0);

                    if (_purchaseReturnEntryQuantity > availableReturnQuantity
                    || _purchaseReturnEntryQuantity <= 0)
                    {
                        MessageBox.Show(string.Format("The valid return quantity is {0} units {1} pieces",
                            availableReturnQuantity / _selectedPurchaseTransactionLine.Item.PiecesPerUnit,
                            availableReturnQuantity % _selectedPurchaseTransactionLine.Item.PiecesPerUnit),
                            "Invalid Quantity Input", MessageBoxButton.OK);
                        return;
                    }

                    // Look if the line exists in the PurchaseReturnTransactionLines already
                    foreach (var line in _purchaseReturnTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_selectedPurchaseTransactionLine.Item.ItemID) &&
                        line.Warehouse.ID.Equals(_selectedPurchaseTransactionLine.Warehouse.ID) &&
                        line.ReturnWarehouse.ID.Equals(_purchaseReturnEntryWarehouse.ID) &&
                        line.PurchasePrice.Equals(_selectedPurchaseTransactionLine.PurchasePrice) &&
                        line.Discount.Equals(_selectedPurchaseTransactionLine.Discount))
                        {
                            line.Quantity += (int)_purchaseReturnEntryQuantity;
                            line.NetDiscount = GetNetDiscount(_selectedPurchaseTransactionLine, line.Quantity);
                            line.UpdateTotal();
                            OnPropertyChanged("PurchaseReturnTransactionNetTotal");
                            return;
                        }
                    }

                    var purchasePrice = _selectedPurchaseTransactionLine.PurchasePrice / _selectedPurchaseTransactionLine.Item.PiecesPerUnit;

                    var fractionOfTransaction = ((int)_purchaseReturnEntryQuantity * (_selectedPurchaseTransactionLine.PurchasePrice - _selectedPurchaseTransactionLine.Discount) / _selectedPurchaseTransactionLine.Item.PiecesPerUnit)
                    / _selectedPurchaseTransactionLine.PurchaseTransaction.GrossTotal;
                    var fractionOfTransactionDiscount = (fractionOfTransaction * _selectedPurchaseTransactionLine.PurchaseTransaction.Discount) / (int)_purchaseReturnEntryQuantity;
                    var discount = (_selectedPurchaseTransactionLine.Discount / _selectedPurchaseTransactionLine.Item.PiecesPerUnit) + fractionOfTransactionDiscount;

                    var pr = new PurchaseReturnTransactionLine
                    {
                        PurchaseReturnTransaction = Model,
                        Item = _selectedPurchaseTransactionLine.Item,
                        Warehouse = _selectedPurchaseTransactionLine.Warehouse,
                        ReturnWarehouse = _purchaseReturnEntryWarehouse.Model,
                        PurchasePrice = purchasePrice,
                        Discount = _selectedPurchaseTransactionLine.Discount / _selectedPurchaseTransactionLine.Item.PiecesPerUnit,
                        NetDiscount = discount,
                        Total = (purchasePrice - discount) * (int)_purchaseReturnEntryQuantity,
                        Quantity = (int)_purchaseReturnEntryQuantity
                    };

                    _purchaseReturnTransactionLines.Add(new PurchaseReturnTransactionLineVM { Model = pr, AvailableReturnQuantity = availableReturnQuantity });
                    OnPropertyChanged("PurchaseReturnTransactionNetTotal");
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
                        Model.Date = _purchaseReturnEntryDate;

                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            var purchaseTransaction = context
                            .PurchaseTransactions
                            .Include("Supplier")
                            .Where(e => e.PurchaseID.Equals(Model.PurchaseTransaction.PurchaseID))
                            .FirstOrDefault();

                            Model.PurchaseTransaction = purchaseTransaction;
                            Model.NetTotal = _purchaseReturnTransactionNetTotal;
                            var user = App.Current.FindResource("CurrentUser") as User;
                            Model.User = context.Users.Where(e => e.Username.Equals(user.Username)).FirstOrDefault();
                            context.PurchaseReturnTransactions.Add(Model);

                            // Calcculate the total amount of Purchase Return
                            // ,record the transaction and decrease the item's quantity in the database
                            foreach (var line in _purchaseReturnTransactionLines)
                            {
                                var stock = context.Stocks
                                .Where(e => e.Item.ItemID.Equals(line.Item.ItemID) &&
                                e.Warehouse.ID.Equals(line.ReturnWarehouse.ID))
                                .FirstOrDefault();

                                // Check if there is enough stock for this return line
                                if (stock == null || stock.Pieces < line.Quantity)
                                {
                                    MessageBox.Show(string.Format("{0} has only {1} units {2} pieces left.", 
                                        line.Item.Name, stock.Pieces / line.Item.PiecesPerUnit, stock.Pieces % line.Item.PiecesPerUnit), 
                                        "Invalid Quantity", MessageBoxButton.OK);
                                    return;
                                }

                                line.Item = context.Inventory
                                .Where(e => e.ItemID.Equals(line.Item.ItemID))
                                .FirstOrDefault();

                                line.Warehouse = context.Warehouses
                                .Where(e => e.ID.Equals(line.Warehouse.ID))
                                .FirstOrDefault();

                                line.ReturnWarehouse = context.Warehouses
                                .Where(e => e.ID.Equals(line.ReturnWarehouse.ID))
                                .FirstOrDefault();

                                context.PurchaseReturnTransactionLines.Add(line.Model);

                                // Decrease the returned quantity from stock
                                stock.Pieces -= line.Quantity;
                                if (stock.Pieces == 0) context.Stocks.Remove(stock);
                                // Increase the SoldOrReturned value for the corresponding purchase transaction line
                                var purchaseTransactionLine = context.PurchaseTransactionLines
                                .Where(e => e.PurchaseTransactionID.Equals(Model.PurchaseTransaction.PurchaseID) &&
                                e.ItemID.Equals(stock.ItemID) && e.WarehouseID.Equals(line.Warehouse.ID)
                                && e.PurchasePrice.Equals(line.PurchasePrice / line.Item.PiecesPerUnit) && e.Discount.Equals(line.Discount / line.Item.PiecesPerUnit))
                                .FirstOrDefault();
                                purchaseTransactionLine.SoldOrReturned += line.Quantity;
                            }

                            Model.PurchaseTransaction.Supplier.PurchaseReturnCredits += _purchaseReturnTransactionNetTotal;

                            // Record the corresponding ledger transactions in the database
                            var ledgerTransaction1 = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction1, DateTime.Now, _purchaseReturnEntryID, "Purchase Return");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, string.Format("{0} Accounts Payable", Model.PurchaseTransaction.Supplier.Name), "Debit", _purchaseReturnTransactionNetTotal);
                            LedgerDBHelper.AddTransactionLine(context, ledgerTransaction1, "Inventory", "Credit", _purchaseReturnTransactionNetTotal);
              
                            context.SaveChanges();
                            ts.Complete();
                        }

                        ResetTransaction();
                        Model = new PurchaseReturnTransaction();
                        Model.Date = DateTime.Now.Date;
                        SetPurchaseReturnTransactionID();
                    }
                }));
            }
        }

        #region Helper Methods
        private void UpdateWarehouses()
        {
            _warehouses.Clear();

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();

                foreach (var warehouse in warehouses)
                    _warehouses.Add(new WarehouseVM { Model = warehouse });
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
            foreach (var warehouse in _warehouses)
            {
                if (warehouse.ID.Equals(_selectedPurchaseTransactionLine.Warehouse.ID))
                {
                    PurchaseReturnEntryWarehouse = warehouse;
                    break;
                }
            }
            PurchaseReturnEntryProduct = _selectedPurchaseTransactionLine.Item.Name;
            PurchaseReturnEntryUnits = _selectedPurchaseTransactionLine.Units - (_selectedPurchaseTransactionLine.SoldOrReturned / _selectedPurchaseTransactionLine.Item.PiecesPerUnit);
            PurchaseReturnEntryPieces = _selectedPurchaseTransactionLine.Pieces - (_selectedPurchaseTransactionLine.SoldOrReturned % _selectedPurchaseTransactionLine.Item.PiecesPerUnit);
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

        private void ResetTransaction()
        {
            SelectedPurchaseTransactionID = null;
            SelectedPurchaseTransactionWhen = null;
            SelectedPurchaseTransactionLine = null;
            PurchaseReturnEntryDate = DateTime.Now.Date;
            PurchaseReturnEntryProduct = null;
            PurchaseReturnEntryUnits = null;
            PurchaseReturnEntryPieces = null;
            _purchaseReturnTransactionLines.Clear();
            _purchaseTransactionLines.Clear();

            OnPropertyChanged("PurchaseReturnTransactionNetTotal");
        }

        private int GetAvailableReturnQuantity()
        {
            var quantity = _selectedPurchaseTransactionLine.Quantity - _selectedPurchaseTransactionLine.SoldOrReturned;

            foreach (var line in _purchaseReturnTransactionLines)
            {
                if (line.Item.ItemID.Equals(_selectedPurchaseTransactionLine.Item.ItemID) &&
                    line.Warehouse.ID.Equals(_selectedPurchaseTransactionLine.Warehouse.ID) &&
                    line.Discount.Equals(_selectedPurchaseTransactionLine.Discount) &&
                    line.PurchasePrice.Equals(_selectedPurchaseTransactionLine.PurchasePrice))
                    quantity -= line.Quantity;    
            }
            return quantity;
        }

        private decimal GetNetDiscount(PurchaseTransactionLineVM line, int quantity)
        {
            var lineDiscount = line.Discount / line.Item.PiecesPerUnit;
            var linePurchasePrice = line.PurchasePrice / line.Item.PiecesPerUnit;
            var fractionOfTransaction = (quantity * (linePurchasePrice - lineDiscount)) / line.PurchaseTransaction.GrossTotal;
            var fractionOfTransactionDiscount = (fractionOfTransaction * line.PurchaseTransaction.Discount) / quantity;
            var discount = (lineDiscount + fractionOfTransactionDiscount) * line.Item.PiecesPerUnit;
            return discount;
        }
        #endregion
    }
}
