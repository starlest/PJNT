using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Purchase;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.Reports;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PaymentListVM : ViewModelBase
    {
        ObservableCollection<SupplierVM> _suppliers;
        ObservableCollection<PurchaseTransaction> _purchaseTransactions;

        SupplierVM _selectedSupplier;
        DateTime _fromDate;
        DateTime _toDate;
        ICommand _printCommand;

        public PaymentListVM()
        {
            _suppliers = new ObservableCollection<SupplierVM>();
            _purchaseTransactions = new ObservableCollection<PurchaseTransaction>();
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date;
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

        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_toDate < value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _fromDate, value, "FromDate");
                UpdatePurchaseTransactions();
            }
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (_fromDate > value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _toDate, value, "ToDate");
                UpdatePurchaseTransactions();
            }
        }

        public SupplierVM SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");
                UpdatePurchaseTransactions();
            }
        }

        public ICommand PrintCommand
        {
            get
            {
                return _printCommand ?? (_printCommand = new RelayCommand(() =>
                {
                    if (_purchaseTransactions.Count == 0) return;

                    //var collectionReportWindow = new CollectionReportWindow(_salesTransactions);
                    //collectionReportWindow.Owner = App.Current.MainWindow;
                    //collectionReportWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    //collectionReportWindow.Show();
                }));
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
            List<PurchaseTransaction> purchaseTransactions;
            using (var context = new ERPContext())
            {
                if (_selectedSupplier.Name.Equals("All"))
                {
                    purchaseTransactions = context.PurchaseTransactions
                        .Include("Supplier")
                        .Where(e => !e.Supplier.Name.Equals("-") && e.Paid < e.Total && (e.DueDate >= _fromDate && e.DueDate <= _toDate))
                        .OrderBy(e => e.DueDate)
                        .ThenBy(e => e.Supplier.Name)
                        .ToList();
                }

                else
                {
                    purchaseTransactions = context.PurchaseTransactions
                        .Include("Supplier")
                        .Where(e => e.Supplier.Name.Equals(_selectedSupplier.Name) && e.Paid < e.Total && (e.DueDate >= _fromDate && e.DueDate <= _toDate))
                        .OrderBy(e => e.DueDate)
                        .ThenBy(e => e.Supplier.Name)
                        .ToList();
                }

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
