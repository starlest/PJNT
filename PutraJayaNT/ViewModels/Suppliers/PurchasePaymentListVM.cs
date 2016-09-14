using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Purchase;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels.Suppliers
{
    using System.Windows.Input;

    internal class PurchasePaymentListVM : ViewModelBase
    {
        private SupplierVM _selectedSupplier;
        private bool _isPaidChecked;
        private DateTime _dueFrom;
        private DateTime _dueTo;
        private decimal _total;
        private ICommand _displayCommand;

        public PurchasePaymentListVM()
        {
            Suppliers = new ObservableCollection<SupplierVM>();
            DisplayedPurchaseTransactions = new ObservableCollection<PurchaseTransaction>();
            var currentDate = UtilityMethods.GetCurrentDate().Date;
            _dueFrom = currentDate.AddDays(-currentDate.Day + 1);
            _dueTo = currentDate;
            UpdateSuppliers();
        }

        public ObservableCollection<SupplierVM> Suppliers { get; }

        public ObservableCollection<PurchaseTransaction> DisplayedPurchaseTransactions { get; }

        public bool IsPaidChecked
        {
            get { return _isPaidChecked; }
            set { SetProperty(ref _isPaidChecked, value, () => IsPaidChecked); }
        }

        public DateTime DueFrom
        {
            get { return _dueFrom; }
            set
            {
                if (_dueTo < value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _dueFrom, value, () => DueFrom);
            }
        }

        public DateTime DueTo
        {
            get { return _dueTo; }
            set
            {
                if (_dueFrom > value)
                {
                    MessageBox.Show("Please select a valid date range.", "Invalid Date Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _dueTo, value, () => DueTo);
            }
        }

        public SupplierVM SelectedSupplier
        {
            get { return _selectedSupplier; }
            set { SetProperty(ref _selectedSupplier, value, () => SelectedSupplier); }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, () => Total); }
        }

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (_selectedSupplier != null) UpdateDisplayedPurchaseTransactions();
                    UpdateSuppliers();
                }));
            }
        }

        #region Helper Methods 
        private void UpdateSuppliers()
        {
            var oldSelectedSupplier = _selectedSupplier;

            Suppliers.Clear();
            Suppliers.Add(new SupplierVM { Model = new Supplier { ID = -1, Name = "All" } });
            using (var context = UtilityMethods.createContext())
            {
                var suppliers = context.Suppliers.Where(supplier => !supplier.Name.Equals("-") && supplier.Active).ToList();
                foreach (var supplier in suppliers)
                    Suppliers.Add(new SupplierVM { Model = supplier });
            }

            UpdateSelectedSupplier(oldSelectedSupplier);
        }

        private void UpdateSelectedSupplier(SupplierVM oldSelectedSupplier)
        {
            if (oldSelectedSupplier == null) return;
            SelectedSupplier = Suppliers.SingleOrDefault(supplier => supplier.ID.Equals(oldSelectedSupplier.ID));
        }

        private void UpdateDisplayedPurchaseTransactions()
        {
            DisplayedPurchaseTransactions.Clear();

            using (var context = UtilityMethods.createContext())
            {
                Func<PurchaseTransaction, bool> searchQuery;

                if (_selectedSupplier.Name.Equals("All") && !_isPaidChecked)
                    searchQuery = transaction => !transaction.Supplier.Name.Equals("-") && transaction.Paid < transaction.Total && transaction.DueDate <= _dueTo;

                else if (!_selectedSupplier.Name.Equals("All") && !_isPaidChecked)
                    searchQuery = transaction => transaction.Supplier.Name.Equals(_selectedSupplier.Name) && transaction.Paid < transaction.Total && transaction.DueDate <= _dueTo;

                else if (_selectedSupplier.Name.Equals("All") && _isPaidChecked)
                    searchQuery = transaction => !transaction.Supplier.Name.Equals("-") && transaction.Paid >= transaction.Total && transaction.DueDate >= _dueFrom && transaction.DueDate <= _dueTo;
                
                else
                    searchQuery = transaction => transaction.Supplier.Name.Equals(_selectedSupplier.Name) && transaction.Paid >= transaction.Total && transaction.DueDate >= _dueFrom && transaction.DueDate <= _dueTo;

                var purchaseTransactions = context.PurchaseTransactions
                    .Include("Supplier")
                    .Where(searchQuery)
                    .OrderBy(transaction => transaction.DueDate)
                    .ThenBy(transaction => transaction.Supplier.Name)
                    .ToList();

                _total = 0;
                foreach (var purchaseTransaction in purchaseTransactions)
                {
                    purchaseTransaction.Remaining = purchaseTransaction.Total - purchaseTransaction.Paid;
                    _total += purchaseTransaction.Remaining;
                    DisplayedPurchaseTransactions.Add(purchaseTransaction);
                }
                UpdateUITotal();
            }
        }

        private void UpdateUITotal()
        {
            OnPropertyChanged("Total");
        }
        #endregion
    }
}
