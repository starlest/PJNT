using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using PutraJayaNT.Models.Accounting;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Transactions;

namespace PutraJayaNT.ViewModels
{
    class PurchaseTransactionVM : ViewModelBase<PurchaseTransaction>
    {
        ObservableCollection<PurchaseTransactionLineVM> _lines;
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<Item> _supplierItems;

        string _newEntryID;
        Supplier _newEntrySupplier;
        DateTime _newEntryDate;
        DateTime _newEntryDueDate;
        Item _newEntryItem;
        int? _newEntryQuantity;
        decimal? _newEntryPrice;
        decimal? _newEntryTotal;

        bool _newEntrySubmitted;

        ICommand _newEntryCommand;
        ICommand _confirmTransactionCommand;
        ICommand _newTransactionCommand;

        public PurchaseTransactionVM()
        {
            Model = new PurchaseTransaction();
            _lines = new ObservableCollection<PurchaseTransactionLineVM>();
            // Add event handler for this collection
            _lines.CollectionChanged += OnCollectionChanged;
            _suppliers = new ObservableCollection<Supplier>();
            _supplierItems = new ObservableCollection<Item>();

            NewEntryDate = DateTime.Now;
            NewEntryDueDate = DateTime.Now;

            SetTransactionID();
            Model.DueDate = _newEntryDueDate;
            Model.Total = 0;
        }

        public ObservableCollection<PurchaseTransactionLineVM> Lines
        {
            get { return _lines; }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get
            {
                _suppliers.Clear();
                
                using (var uow = new UnitOfWork())
                {
                    var suppliers = uow.SupplierRepository.GetAll();
                    foreach (var supplier in suppliers)
                    {
                        _suppliers.Add(supplier);
                    }
                }

                return _suppliers;
            } 
        } 

        public ObservableCollection<Item> SupplierItems
        {
            get { return _supplierItems; }
        }

        public string NewEntryID
        {
            get { return _newEntryID; }
            set { SetProperty(ref _newEntryID, value, "NewEntryID"); }
        }

        public Supplier NewEntrySupplier
        {
            get { return _newEntrySupplier; }
            set
            {
                if (value == null)
                {
                    _supplierItems.Clear();
                    SetProperty(ref _newEntrySupplier, value, "NewEntrySupplier");
                    return;
                }

                if (_suppliers.Contains(value))
                {
                    // No longer allows user to select another supplier
                    _suppliers.Clear();
                    _suppliers.Add(value);

                    // Display Supplier's Items and IDs
                    _supplierItems.Clear();
                    using (var context = new ERPContext())
                    {
                        var items = context.Inventory
                            .Include("Suppliers");

                        foreach (var item in items)
                        {
                            if (item.Active == true && item.Suppliers.Contains(value))
                                _supplierItems.Add(item);
                        }
                    }

                    SetProperty(ref _newEntrySupplier, value, "NewEntrySupplier");
                    OnPropertyChanged("SupplierItems");

                    Model.Supplier = value;  
                }
            }
        }

        public DateTime NewEntryDate
        {
            get { return _newEntryDate; }
            set
            {
                SetProperty(ref _newEntryDate, value, "NewEntryDate");
                Model.Date = _newEntryDate;
                SetTransactionID();
            }
        }

        public DateTime NewEntryDueDate
        {
            get { return _newEntryDueDate; }
            set
            {
                SetProperty(ref _newEntryDueDate, value, "NewEntryDueDate");
                Model.DueDate = _newEntryDueDate;
            }
        }

        public Item NewEntryItem
        {
            get { return _newEntryItem; }
            set { SetProperty(ref _newEntryItem, value, "NewEntryItem"); }
        }

        public int? NewEntryQuantity
        {
            get { return _newEntryQuantity; }
            set
            {
                SetProperty(ref _newEntryQuantity, value, "NewEntryQuantity");
            }
        }

        public decimal? NewEntryPrice
        {
            get { return _newEntryPrice; }
            set { SetProperty(ref _newEntryPrice, value, "NewEntryPrice"); }
        }

        public decimal? NewEntryTotal
        {
            get
            {
                _newEntryTotal = 0;
                foreach (var line in _lines)
                {
                    _newEntryTotal += line.Total;
                }
                return _newEntryTotal;
            }
        }

        public bool NewEntrySubmitted
        {
            get { return _newEntrySubmitted; }
            set { SetProperty(ref _newEntrySubmitted, value, "NewEntrySubmitted"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (_newEntrySupplier == null || _newEntryItem == null 
                    || _newEntryQuantity == null || _newEntryPrice == null) 
                    {
                        MessageBox.Show("Please enter all required fields", "Missing field(s)", MessageBoxButton.OK);
                        return;
                    }

                    // Check if the item exists in one of the lines 
                    foreach (var l in _lines)
                    {
                        if (_newEntryItem.Equals(l.Item) && !_newEntryPrice.Equals(l.PurchasePrice))
                        {
                            l.Quantity += (int) _newEntryQuantity;
                            NewEntryItem = null;
                            NewEntryQuantity = null;
                            NewEntryPrice = null;
                            return;
                        }
                    }

                    var line = new PurchaseTransactionLine
                    {
                        PurchasePrice = (decimal) _newEntryPrice,
                        Quantity = (int) _newEntryQuantity,
                        Total = (decimal) _newEntryPrice * (int) _newEntryQuantity,
                        Item = _newEntryItem,
                        PurchaseTransaction = this.Model
                    };

                    var lineVM = new PurchaseTransactionLineVM { Model = line };
                    _lines.Add(lineVM);

                    // Update Total Amount
                    OnPropertyChanged("NewEntryTotal");

                    // Reset fields
                    NewEntryItem = null;
                    NewEntryQuantity = null;
                    NewEntryPrice = null;

                    NewEntrySubmitted = true;
                    NewEntrySubmitted = false;
                }));
            }
        }

        public ICommand ConfirmTransactionCommand
        {
            get
            {
                return _confirmTransactionCommand ?? (_confirmTransactionCommand = new RelayCommand(() =>
                {
                    if (_lines.Count == 0)
                    {
                        MessageBox.Show("Please input at least one entry.", "Empty Transaction", MessageBoxButton.OK);
                        return;
                    }
                    
                    if (MessageBox.Show("Confirm Purchase?", "Confirmation", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope(TransactionScopeOption.Required))
                        {
                            var context = new ERPContext();

                            // Increase the respective item's stock
                            foreach (var line in this.Model.PurchaseTransactionLines)
                            {
                                Model.Total += line.Total;
                                line.Item.Stock += line.Quantity;
                                context.Inventory.Attach(line.Item);
                                ((IObjectContextAdapter)context)
                                .ObjectContext.ObjectStateManager.ChangeObjectState(line.Item, EntityState.Modified);
                            }

                            Model.Supplier = context.Suppliers.Where(e => e.ID == Model.Supplier.ID).FirstOrDefault();
                            context.PurchaseTransactions.Add(Model);

                            // Record the accounting journal entry for this purchase
                            LedgerTransaction transaction = new LedgerTransaction();
                            string accountsPayableName = _newEntrySupplier + " Accounts Payable";

                            LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, _newEntryID.ToString(), "Purchase");
                            context.SaveChanges();
                            LedgerDBHelper.AddTransactionLine(context, transaction, "Inventory", "Debit", Model.Total);
                            LedgerDBHelper.AddTransactionLine(context, transaction, accountsPayableName, "Credit", Model.Total);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        // Reset the fields for a new transaction
                        Model = new PurchaseTransaction();
                        ResetFields();
                        SetTransactionID();
                        Model.Total = 0;
                    }
                }));
            }
        }

        public ICommand NewTransactionCommand
        {
            get
            {
                return _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(() =>
                {
                        // Reset the fields for a new transaction
                        ResetFields();
                }));
            }
        }

        private void SetTransactionID()
        {
            var month = _newEntryDate.Month;
            var year = _newEntryDate.Year;
            NewEntryID = "P" + ((long) ((year - 2000) * 100 + month) * 1000000).ToString();

            string lastEntryID = null;
            using(var context = new ERPContext())
            {
                var IDs = (from PurchaseTransaction in context.PurchaseTransactions
                           where PurchaseTransaction.PurchaseID.CompareTo(_newEntryID.ToString()) >= 0
                           orderby PurchaseTransaction.PurchaseID descending
                           select PurchaseTransaction.PurchaseID);
                if (IDs.Count() != 0) lastEntryID = IDs.First();
            }

            if (lastEntryID != null) NewEntryID = "P" + (Convert.ToInt64(lastEntryID.Substring(1)) + 1).ToString();

            Model.PurchaseID = _newEntryID;
        }

        private void ResetFields()
        {
            NewEntryItem = null;
            NewEntrySupplier = null;
            NewEntryQuantity = null;
            NewEntryPrice = null;

            Model.Date = _newEntryDate;
            Model.DueDate = _newEntryDueDate;
            Model.Total = 0;

            _supplierItems.Clear();
            Model.PurchaseTransactionLines.Clear();
            _lines.Clear();

            OnPropertyChanged("Suppliers");
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                // Remove the corresponding Transaction Line in the Model's collection
                foreach (PurchaseTransactionLineVM line in e.OldItems)
                {
                    // Unsuscribe to event handler to prevent strong reference
                    line.PropertyChanged -= LinePropertyChangedHandler;
                    foreach (PurchaseTransactionLine l in Model.PurchaseTransactionLines)
                    {
                        if (l.Item.Name == line.Item.Name)
                        {
                            Model.PurchaseTransactionLines.Remove(l);
                            break;
                        }
                    }
                }
            }

            else if (e.NewItems != null)
            {
                // Suscribe an event handler to the line and
                // add the corresponding Transaction Line into the Model's collection too
                foreach (PurchaseTransactionLineVM line in e.NewItems)
                {
                    line.PropertyChanged += LinePropertyChangedHandler;
                    Model.PurchaseTransactionLines.Add(line.Model);
                }
            }

            // Refresh Total Amount
            OnPropertyChanged("NewEntryTotal");
        }

        void LinePropertyChangedHandler(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Total")
                OnPropertyChanged("NewEntryTotal");
        }
    }
}
