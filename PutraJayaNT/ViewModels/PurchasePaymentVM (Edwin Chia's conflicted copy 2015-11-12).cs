using MVVMFramework;
using PUJASM.ERP.Models;
using PUJASM.ERP.Models.Accounting;
using PUJASM.ERP.Utilities;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace PUJASM.ERP.ViewModels
{
    class PurchasePaymentVM : ViewModelBase
    {
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<PurchaseTransaction> _supplierUnpaidPurchases;
        ObservableCollection<PurchaseTransactionLineVM> _selectedPurchaseLines;

        decimal? _total;
        decimal? _remaining;
        decimal? _pay;

        Supplier _selectedSupplier;
        PurchaseTransaction _selectedPurchase;

        ICommand _confirmPaymentCommand;

        public PurchasePaymentVM()
        {
            _suppliers = new ObservableCollection<Supplier>();
            _supplierUnpaidPurchases = new ObservableCollection<PurchaseTransaction>();
            _selectedPurchaseLines = new ObservableCollection<PurchaseTransactionLineVM>();

            RefreshSuppliers();
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get { return _suppliers; }
        }
        public ObservableCollection<PurchaseTransaction> SupplierUnpaidPurchases
        {
            get { return _supplierUnpaidPurchases; }
        }

        public ObservableCollection<PurchaseTransactionLineVM> SelectedPurchaseLines
        {
            get { return _selectedPurchaseLines; }
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
                        using (var context = new ERPContext())
                        {
                            var ledgerTransaction1 = new LedgerTransaction();
                            var ledgerTransaction2 = new LedgerTransaction();

                            _selectedPurchase.Paid += (decimal) _pay;
                            context.PurchaseTransactions.Attach(_selectedPurchase);
                            ((IObjectContextAdapter)context)
                            .ObjectContext.ObjectStateManager.ChangeObjectState(_selectedPurchase, EntityState.Modified);

                            LedgerDBHelper.AddTransaction(context, ledgerTransaction1,
                                string.Format("{0} Accounts Payable", _selectedPurchase.Supplier.Name),
                                string.Format("Payment Purchase {0}", _selectedPurchase.PurchaseID),
                                "Debit", (decimal) _pay);
                            LedgerDBHelper.AddTransaction(context, ledgerTransaction2, "Cash",
                                string.Format("Payment Purchase {0}", _selectedPurchase.PurchaseID),
                                "Credit", (decimal) _pay);

                            context.SaveChanges();
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
