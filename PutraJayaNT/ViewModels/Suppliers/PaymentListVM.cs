using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Purchase;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PaymentListVM : ViewModelBase
    {
        ObservableCollection<SupplierVM> _suppliers;
        ObservableCollection<PurchaseTransaction> _purchaseTransactions;

        SupplierVM _selectedSupplier;
        DateTime _date;
        decimal _total;

        public PaymentListVM()
        {
            _suppliers = new ObservableCollection<SupplierVM>();
            _purchaseTransactions = new ObservableCollection<PurchaseTransaction>();
            _date = DateTime.Now.Date;
            UpdateSuppliers();
        }

        public ObservableCollection<SupplierVM> Suppliers
        {
            get { return _suppliers; }
        }

        public ObservableCollection<PurchaseTransaction> PurchaseTransactions
        {
            get { return _purchaseTransactions; }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                SetProperty(ref _date, value, "Date");
                if (_selectedSupplier != null) UpdatePurchaseTransactions();
            }
        }

        public SupplierVM SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");
                UpdatePurchaseTransactions();
                OnPropertyChanged("Total");
            }
        }

        public decimal Total
        {
            get
            {
                _total = 0;

                foreach (var line in _purchaseTransactions)
                {
                    _total += line.Total;
                }

                return _total;
            }
        }
        #region Helper Methods 
        private void UpdateSuppliers()
        {
            _suppliers.Clear();
            _suppliers.Add(new SupplierVM { Model = new Supplier { ID = -1, Name = "All" } });
            using (var context = new ERPContext())
            {
                var suppliers = context.Suppliers.Where(e => !e.Name.Equals("-")).ToList();

                foreach (var supplier in suppliers)
                    _suppliers.Add(new SupplierVM { Model = supplier });
            }
        }

        private void UpdatePurchaseTransactions()
        {
            _purchaseTransactions.Clear();
            Func<PurchaseTransaction, bool> query;

            using (var context = new ERPContext())
            {
                if (_selectedSupplier.Name.Equals("All"))
                    query = e => !e.Supplier.Name.Equals("-") && e.Paid < e.Total && e.DueDate <= _date;

                else
                    query = e => e.Supplier.Name.Equals(_selectedSupplier.Name) && e.Paid < e.Total && e.DueDate <= _date;

                var purchaseTransactions = context.PurchaseTransactions
                    .Include("Supplier")
                    .Where(query)
                    .OrderBy(e => e.DueDate)
                    .ThenBy(e => e.Supplier.Name)
                    .ToList();

                foreach (var t in purchaseTransactions)
                {
                    t.Remaining = t.Total - t.Paid;
                    _purchaseTransactions.Add(t);
                }
            }
        }
        #endregion
    }
}
