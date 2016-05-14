namespace PutraJayaNT.ViewModels.Reports
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using Utilities;
    using Item;
    using Purchase;

    internal class PurchasesReportVM : ViewModelBase
    {
        #region Backing Fields
        private DateTime _fromDate;
        private DateTime _toDate;
        private Supplier _selectedSupplier;
        private ItemVM _selectedItem;
        private decimal _total;
        private int _units;
        private ICommand _displayCommand;
        private ICommand _clearCommand;

        #endregion

        public PurchasesReportVM()
        {
            Suppliers = new ObservableCollection<Supplier>();
            SupplierItems = new ObservableCollection<ItemVM>();
            DisplayLines = new ObservableCollection<PurchaseTransactionLineVM>();
            _fromDate = UtilityMethods.GetCurrentDate().Date.AddDays(-UtilityMethods.GetCurrentDate().Day + 1);
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateSuppliers();
        }

        #region Collections
        public ObservableCollection<Supplier> Suppliers { get; }

        public ObservableCollection<ItemVM> SupplierItems { get; }

        public ObservableCollection<PurchaseTransactionLineVM> DisplayLines { get; }
        #endregion

        #region Properties
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
                SetProperty(ref _fromDate, value, () => FromDate);
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
                SetProperty(ref _toDate, value, () => ToDate);
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                SupplierItems.Clear();
                SetProperty(ref _selectedSupplier, value, () => SelectedSupplier);
                if (_selectedSupplier == null) return;
                SelectedItem = null;
                UpdateSupplierItems();
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value, () => SelectedItem); }
        }

        public decimal Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value, () => Total); }
        }

        public int Units
        {
            get { return _units; }
            set { SetProperty(ref _units, value, () => Units); }
        }
        #endregion

        #region Commands
        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (_selectedItem != null) RefreshDisplaylines();
                }));
            }
        }

        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand(ResetUI));
        #endregion

        #region Helper Methods
        private void UpdateSuppliers()
        {
            Suppliers.Clear();
            DisplayLines.Clear();
            SupplierItems.Clear();

            Suppliers.Add(new Supplier { ID = -1, Name = "All" });
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var suppliers = context.Suppliers.Where(e => !e.Name.Equals("-")).OrderBy(e => e.Name);
                foreach (var supplier in suppliers)
                    Suppliers.Add(supplier);
            }
        }

        private void UpdateSupplierItems()
        {
            SupplierItems.Add(new ItemVM { Model = new Item { ItemID = "-1", Name = "All" } });
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var  items = context.Inventory.Include("Suppliers").OrderBy(e => e.Name).ToList();

                if (_selectedSupplier.Name.Equals("All"))
                {
                    foreach (var item in items)
                        SupplierItems.Add(new ItemVM { Model = item });
                }

                else
                {
                    foreach (var item in items)
                    {
                        if (item.Suppliers.Contains(_selectedSupplier))
                            SupplierItems.Add(new ItemVM { Model = item });
                    }
                }
            }
        }

        private void RefreshDisplaylines()
        {
            DisplayLines.Clear();

            if (_selectedSupplier.Name.Equals("All") && _selectedItem.Name.Equals("All"))
            {
                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var purchases = context.PurchaseTransactions
                        .Where(e => e.Date >= _fromDate && e.Date <= _toDate && !e.Supplier.Name.Equals("-"))
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .Include("PurchaseTransactionLines.PurchaseTransaction");

                    foreach (var purchase in purchases)
                    {
                        foreach (var line in purchase.PurchaseTransactionLines)
                            DisplayLines.Add(new PurchaseTransactionLineVM { Model = line });

                    }
                }
            }

            else if (_selectedSupplier.Name.Equals("All") && !_selectedItem.Name.Equals("All"))
            {
                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var purchases = context.PurchaseTransactions
                        .Where(e => e.Date >= _fromDate && e.Date <= _toDate && !e.Supplier.Name.Equals("-"))
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .Include("PurchaseTransactionLines.PurchaseTransaction");

                    foreach (var purchase in purchases)
                    {
                        foreach (var line in purchase.PurchaseTransactionLines)
                        {
                            if (line.ItemID.Equals(_selectedItem.ID))
                                DisplayLines.Add(new PurchaseTransactionLineVM { Model = line });
                        }
                    }
                }
            }


            else if (!_selectedSupplier.Name.Equals("All") && _selectedItem.Name.Equals("All"))
            {
                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var purchases = context.PurchaseTransactions
                        .Where(e => e.Supplier.ID == _selectedSupplier.ID && e.Date >= _fromDate && e.Date <= _toDate && !e.Supplier.Name.Equals("-"))
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .Include("PurchaseTransactionLines.PurchaseTransaction");

                    foreach (var purchase in purchases)
                    {
                        foreach (var line in purchase.PurchaseTransactionLines)
                            DisplayLines.Add(new PurchaseTransactionLineVM { Model = line });

                    }
                }
            }

            else
            {
                using (var context = new ERPContext(UtilityMethods.GetDBName()))
                {
                    var purchases = context.PurchaseTransactions
                        .Where(e => e.Supplier.ID == _selectedSupplier.ID && e.Date >= _fromDate && e.Date <= _toDate)
                        .Include("PurchaseTransactionLines")
                        .Include("PurchaseTransactionLines.Item")
                        .Include("PurchaseTransactionLines.Warehouse")
                        .Include("PurchaseTransactionLines.PurchaseTransaction");


                    foreach (var purchase in purchases)
                    {
                        foreach (var line in purchase.PurchaseTransactionLines)
                        {
                            if (line.ItemID.Equals(_selectedItem.ID))
                                DisplayLines.Add(new PurchaseTransactionLineVM { Model = line });
                        }
                    }
                }
            }

            UpdateTotal();
            UpdateUnits();
        }

        private void UpdateTotal()
        {
            _total = 0;
            foreach (var line in DisplayLines)
                _total += line.Total;
            OnPropertyChanged("Total");
        }

        private void UpdateUnits()
        {
            _units = 0;
            foreach (var line in DisplayLines)
                _units += line.Quantity / line.Item.PiecesPerUnit;
            OnPropertyChanged("Units");
        }

        private void ResetUI()
        {
            DisplayLines.Clear();
            SelectedItem = null;
            SupplierItems.Clear();
            SelectedSupplier = null;
            UpdateSuppliers();
            Total = 0;
            Units = 0;
        }
        #endregion
    }
}
