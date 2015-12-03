using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels
{
    class PurchaseReturnTransactionVM : ViewModelBase<PurchaseReturnTransaction>
    {
        ObservableCollection<PurchaseTransactionLine> _purchaseTransactionLines;
        ObservableCollection<PurchaseReturnTransactionLine> _purchaseReturnTransactionLines;

        string _selectedPurchaseTransactionID;
        PurchaseTransactionLine _selectedPurchaseTransactionLine;
        DateTime? _selectedPurchaseTransactionWhen;

        string _purchaseReturnEntryID;
        DateTime _purchaseReturnEntryDate;
        string _purchaseReturnEntryProduct;
        int? _purchaseReturnEntryQuantity;
        ICommand _purchaseReturnEntryAddCommand;

        ICommand _newCommand;
        ICommand _confirmCommand;

        public PurchaseReturnTransactionVM()
        {
            Model = new PurchaseReturnTransaction();

            _purchaseTransactionLines = new ObservableCollection<PurchaseTransactionLine>();
            _purchaseReturnTransactionLines = new ObservableCollection<PurchaseReturnTransactionLine>();

            _purchaseReturnEntryDate = DateTime.Now.Date;

            SetPurchaseReturnTransactionID();
        }

        public ObservableCollection<PurchaseTransactionLine> PurchaseTransactionLines
        {
            get { return _purchaseTransactionLines; }
        }

        public ObservableCollection<PurchaseReturnTransactionLine> PurchaseReturnTransactionLines
        {
            get { return _purchaseReturnTransactionLines; }
        }

        public string SelectedPurchaseTransactionID
        {
            get { return _selectedPurchaseTransactionID; }
            set
            {
                SetProperty(ref _selectedPurchaseTransactionID, value, "SelectedPurchaseTransactionID");
                if (UpdatePurchaseTransactionLines())
                {
                    SelectedPurchaseTransactionWhen = _purchaseTransactionLines.FirstOrDefault().PurchaseTransaction.Date;
                    Model.PurchaseTransaction = _purchaseTransactionLines.FirstOrDefault().PurchaseTransaction;
                }
                else
                {
                    SelectedPurchaseTransactionWhen = null;
                }
            }
        }

        public DateTime? SelectedPurchaseTransactionWhen
        {
            get { return _selectedPurchaseTransactionWhen; }
            set { SetProperty(ref _selectedPurchaseTransactionWhen, value, "SelectedPurchaseTransactionWhen"); }
        }

        public PurchaseTransactionLine SelectedPurchaseTransactionLine
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
                    .Where(e => e.PurchaseID.Equals(_selectedPurchaseTransactionID))
                    .Include("PurchaseTransaction")
                    .Include("PurchaseTransaction.Supplier")
                    .Include("Item")
                    .ToList();

                if (lines.Count > 0) found = true;

                foreach (var line in lines)
                    _purchaseTransactionLines.Add(line);
            }

            return found;
        }

        private void UpdateReturnEntryProperties()
        {
            PurchaseReturnEntryProduct = _selectedPurchaseTransactionLine.Item.Name;
            PurchaseReturnEntryQuantity = _selectedPurchaseTransactionLine.Quantity;
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

        // -------------------- Return Entry Properties -------------------- //

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

        public int? PurchaseReturnEntryQuantity
        {
            get { return _purchaseReturnEntryQuantity; }
            set { SetProperty(ref _purchaseReturnEntryQuantity, value, "PurchaseReturnEntryQuantity"); }
        }

        public ICommand PurchaseReturnEntryAddCommand
        {
            get
            {
                return _purchaseReturnEntryAddCommand ?? (_purchaseReturnEntryAddCommand = new RelayCommand(() =>
                {
                    var availableReturnQuantity = _selectedPurchaseTransactionLine.Quantity;
                    var availableStock = _selectedPurchaseTransactionLine.Item.Stock;
                    using (var context = new ERPContext())
                    {
                        var returnedItems = context.PurchaseReturnTransactionLines
                        .Where(e => e.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID
                        .Equals(Model.PurchaseTransaction.PurchaseID) && e.ItemID.Equals(_selectedPurchaseTransactionLine.ItemID));

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

                    if (_purchaseReturnEntryQuantity > _selectedPurchaseTransactionLine.Quantity
                    || _purchaseReturnEntryQuantity > availableReturnQuantity
                    || _purchaseReturnEntryQuantity > availableStock
                    || _purchaseReturnEntryQuantity <= 0)
                    {
                        MessageBox.Show("Please enter the right amount of quantity.", "Invalid Quantity Input", MessageBoxButton.OK);
                        return;
                    }

                    // Look if the line exists in the SalesReturnTransactionLines already
                    foreach (var line in _purchaseReturnTransactionLines)
                    {
                        if (line.Item.ItemID.Equals(_selectedPurchaseTransactionLine.ItemID))
                        {
                            if ((line.Quantity + _purchaseReturnEntryQuantity) > _selectedPurchaseTransactionLine.Quantity ||
                            (line.Quantity + _purchaseReturnEntryQuantity) > availableReturnQuantity ||
                            (line.Quantity + _purchaseReturnEntryQuantity) > availableStock ||
                            (line.Quantity + _purchaseReturnEntryQuantity) <= 0)
                            {
                                MessageBox.Show("Please enter the right amount of quantity.", "Invalid Quantity Input", MessageBoxButton.OK);
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
                        PurchasePrice = _selectedPurchaseTransactionLine.PurchasePrice,
                        Quantity = (int)_purchaseReturnEntryQuantity
                    };

                    _purchaseReturnTransactionLines.Add(pr);

                }));
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
                        Model.Date = (DateTime)_selectedPurchaseTransactionWhen;

                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();                          

                            context.PurchaseTransactions.Attach(Model.PurchaseTransaction);
                            context.PurchaseReturnTransactions.Add(Model);

                            decimal totalAmount = 0;
                            // Calcculate the total amount of Purchase Return
                            // ,record the transaction and decrease the item's quantity in the database
                            foreach (var line in _purchaseReturnTransactionLines)
                            {
                                totalAmount += line.PurchasePrice * line.Quantity;
                                context.Inventory.Attach(line.Item);
                                context.PurchaseReturnTransactionLines.Add(line);

                                line.Item.Stock -= line.Quantity;
                                ((IObjectContextAdapter)context).ObjectContext.
                                ObjectStateManager.ChangeObjectState(line.Item, EntityState.Modified);
                            }

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
            PurchaseReturnEntryQuantity = null;
            _purchaseReturnTransactionLines.Clear();
            _purchaseTransactionLines.Clear();
        }
    }
}
