using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System;
using System.Transactions;

namespace PutraJayaNT.ViewModels
{
    class PurchasePaymentVM : ViewModelBase
    {
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<PurchaseTransaction> _supplierUnpaidPurchases;
        ObservableCollection<PurchaseTransactionLineVM> _selectedPurchaseLines;
        ObservableCollection<string> _paymentModes;

        decimal? _total;
        decimal? _remaining;
        decimal? _pay;

        Supplier _selectedSupplier;
        PurchaseTransaction _selectedPurchase;
        string _selectedPaymentMode;

        ICommand _confirmPaymentCommand;

        public PurchasePaymentVM()
        {
            _suppliers = new ObservableCollection<Supplier>();
            _supplierUnpaidPurchases = new ObservableCollection<PurchaseTransaction>();
            _selectedPurchaseLines = new ObservableCollection<PurchaseTransactionLineVM>();
            _paymentModes = new ObservableCollection<string>();

            _paymentModes.Add("Cash");
            using (var context = new ERPContext())
            {
                var bankAccounts = context.Ledger_Accounts
                    .Where(e => e.Name.Contains("Bank"));
                foreach (var bank in bankAccounts)
                    _paymentModes.Add(bank.Name);
            }

            RefreshSuppliers();
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get
            {
                RefreshSuppliers();
                return _suppliers;
            }
        }

        public ObservableCollection<PurchaseTransaction> SupplierUnpaidPurchases
        {
            get { return _supplierUnpaidPurchases; }
        }

        public ObservableCollection<PurchaseTransactionLineVM> SelectedPurchaseLines
        {
            get { return _selectedPurchaseLines; }
        }

        public ObservableCollection<string> PaymentModes
        {
            get { return _paymentModes; }
        }

        public decimal? Total
        {
            get { return _total; }
            set
            {
                SetProperty(ref _total, value, "Total");
            }
        }

        public decimal? Remaining
        {
            get { return _remaining; }
            set
            {
                SetProperty(ref _remaining, value, "Remaining");
            }
        }

        public decimal? Pay
        {
            get { return _pay; }
            set
            {
                if (value <= 0 || value > _remaining)
                {
                    MessageBox.Show("Please input the correct amount", "Wrong Value", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _pay, value, "Pay");
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                _selectedPurchaseLines.Clear();

                if (value == null)
                {
                    SelectedPurchase = null;
                }

                else
                {
                    _supplierUnpaidPurchases.Clear();
                    RefreshSuppliers();

                    using (var context = new ERPContext())
                    {
                        var unpaidPurchases = context.PurchaseTransactions
                            .Where(e => e.Supplier.ID == value.ID && e.Paid < e.Total)
                            .Include("Supplier").Include("PurchaseTransactionLines.Item");

                        foreach (var purchase in unpaidPurchases)
                            _supplierUnpaidPurchases.Add(purchase);
                    }
                }

                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");
            }
        }

        public PurchaseTransaction SelectedPurchase
        {
            get { return _selectedPurchase; }
            set
            {
                if (value == null) _selectedPurchaseLines.Clear();

                else
                {
                    _selectedPurchaseLines.Clear();
                    foreach (var line in value.PurchaseTransactionLines)
                    {
                        var lineVM = new PurchaseTransactionLineVM { Model = line };
                        _selectedPurchaseLines.Add(lineVM);
                    }

                    Total = value.Total;
                    Remaining = value.Total - value.Paid;
                }

                SetProperty(ref _selectedPurchase, value, "SelectedPurchase");
            }
        }

        public string SelectedPaymentMode
        {
            get { return _selectedPaymentMode; }
            set { SetProperty(ref _selectedPaymentMode, value, "SelectedPaymentMode"); }
        }

        public ICommand ConfirmPaymentCommand
        {
            get
            {
                return _confirmPaymentCommand ?? (_confirmPaymentCommand = new RelayCommand(() =>
                {
                    if (_pay == null)
                    {
                        MessageBox.Show("Please enter payment amount", "Empty Field", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm Payment?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var ts = new TransactionScope())
                        {
                            var context = new ERPContext();

                            _selectedPurchase.Paid += (decimal) _pay;
                            context.PurchaseTransactions.Attach(_selectedPurchase);
                            ((IObjectContextAdapter)context)
                            .ObjectContext.ObjectStateManager.ChangeObjectState(_selectedPurchase, EntityState.Modified);

                            var accountsPayableName = _selectedSupplier.Name + " Accounts Payable";
                            var transaction = new LedgerTransaction();

                            LedgerDBHelper.AddTransaction(context, transaction, DateTime.Now.Date, _selectedPurchase.PurchaseID.ToString(), "Purchase Payment");
                            context.SaveChanges();

                            LedgerDBHelper.AddTransactionLine(context, transaction, accountsPayableName, "Debit", (decimal) _pay);
                            LedgerDBHelper.AddTransactionLine(context, transaction, _selectedPaymentMode, "Credit", (decimal) _pay);
                            context.SaveChanges();

                            ts.Complete();
                        }

                        Total = null;
                        Remaining = null;
                        Pay = null;
                        SelectedSupplier = null;
                        _supplierUnpaidPurchases.Clear();
                    }

                }));
            }
        }

        public void RefreshSuppliers()
        {
            _suppliers.Clear();
            using (var context = new ERPContext())
            {
                var suppliers = context.Suppliers;
                foreach (var supplier in suppliers)
                    _suppliers.Add(supplier);
            }
        }
    }
}
