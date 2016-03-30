namespace PutraJayaNT.ViewModels.Reports
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using Utilities;
    using Inventory;
    using Item;
    using Purchase;
    using Suppliers;

    class PurchasesReportVM : ViewModelBase
    {
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<ItemVM> _supplierItems;
        ObservableCollection<PurchaseTransactionLineVM> _displayLines;

        DateTime _fromDate;
        DateTime _toDate;

        Supplier _selectedSupplier;
        ItemVM _selectedItem;

        decimal _total;

        public PurchasesReportVM()
        {
            _suppliers = new ObservableCollection<Supplier>();
            _supplierItems = new ObservableCollection<ItemVM>();
            _displayLines = new ObservableCollection<PurchaseTransactionLineVM>();
            _fromDate = UtilityMethods.GetCurrentDate().Date.AddDays(-UtilityMethods.GetCurrentDate().Day + 1);
            _toDate = UtilityMethods.GetCurrentDate().Date;

            UpdateSuppliers();
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get
            {
                return _suppliers;
            }
        }

        public ObservableCollection<ItemVM> SupplierItems
        {
            get { return _supplierItems; }
        }

        public ObservableCollection<PurchaseTransactionLineVM> DisplayLines
        {
            get { return _displayLines; }
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
                if (_selectedSupplier != null && _selectedItem != null) RefreshDisplaylines();
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
                if (_selectedSupplier != null && _selectedItem != null) RefreshDisplaylines();
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                _supplierItems.Clear();
                SetProperty(ref _selectedSupplier, value, "SelectedSupplier");

                if (value == null)
                {
                    _displayLines.Clear();
                    SelectedItem = null;
                    return;
                }

                UpdateSuppliers();
                UpdateSupplierItems();
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, "SelectedItem");

                if (_selectedItem == null) return;

                RefreshDisplaylines();
             }
        }

        public decimal Total
        {
            get
            {
                _total = 0;

                foreach (var line in _displayLines)
                    _total += line.Total;

                return _total;
            }
        }

        private void UpdateSuppliers()
        {
            _suppliers.Clear();
            _displayLines.Clear();
            _supplierItems.Clear();

            _suppliers.Add(new Supplier { ID = -1, Name = "All" });
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var suppliers = context.Suppliers.Where(e => !e.Name.Equals("-")).OrderBy(e => e.Name);
                foreach (var supplier in suppliers)
                    _suppliers.Add(supplier);
            }
        }

        private void UpdateSupplierItems()
        {
            _supplierItems.Add(new ItemVM { Model = new Item { ItemID = "-1", Name = "All" } });
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var  items = context.Inventory.Include("Suppliers").OrderBy(e => e.Name).ToList();

                if (_selectedSupplier.Name.Equals("All"))
                {
                    foreach (var item in items)
                        _supplierItems.Add(new ItemVM { Model = item });
                }

                else
                {
                    foreach (var item in items)
                    {
                        if (item.Suppliers.Contains(_selectedSupplier))
                            _supplierItems.Add(new ItemVM { Model = item });
                    }
                }
            }
        }

        private void RefreshDisplaylines()
        {
            _displayLines.Clear();

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
                            _displayLines.Add(new PurchaseTransactionLineVM { Model = line });

                    }
                }
            }

            else if (_selectedItem.Name.Equals("All"))
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
                            _displayLines.Add(new PurchaseTransactionLineVM { Model = line });

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
                                _displayLines.Add(new PurchaseTransactionLineVM { Model = line });
                        }
                    }
                }
            }

            OnPropertyChanged("Total");
        }
    }
}
