using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Suppliers;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels
{
    class PurchasesReportVM : ViewModelBase
    {
        ObservableCollection<Supplier> _suppliers;
        ObservableCollection<ItemVM> _supplierItems;
        ObservableCollection<PurchaseTransactionLineVM> _displayLines;

        DateTime _fromDate;
        DateTime _toDate;

        Supplier _selectedSupplier;
        ItemVM _selectedItem;

        public PurchasesReportVM()
        {
            _suppliers = new ObservableCollection<Supplier>();
            _supplierItems = new ObservableCollection<ItemVM>();
            _displayLines = new ObservableCollection<PurchaseTransactionLineVM>();
            _fromDate = DateTime.Now.Date;
            _toDate = DateTime.Now.Date.AddDays(1);
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get
            {
                RefreshSuppliers();
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

                RefreshSuppliers();
            
                _supplierItems.Add(new ItemVM { Model = new Item { ItemID = "-1", Name = "All" } });
                using (var context = new ERPContext())
                {
                    var items = context.Inventory
                        .OrderBy(e => e.Name)
                        .Include("Suppliers");

                    foreach (var item in items)
                    {
                        if (item.Suppliers.Contains(_selectedSupplier))
                            _supplierItems.Add(new ItemVM { Model = item });
                    }
                }
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

        public void RefreshSuppliers()
        {
            _suppliers.Clear();
            _displayLines.Clear();
            _supplierItems.Clear();
            using (var context = new ERPContext())
            {
                var suppliers = context.Suppliers.OrderBy(e => e.Name);
                foreach (var supplier in suppliers)
                    _suppliers.Add(supplier);
            }
        }

        public void RefreshDisplaylines()
        {
            _displayLines.Clear();

            if (_selectedItem.Name.Equals("All"))
            {
                using (var context = new ERPContext())
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
                            _displayLines.Add(new PurchaseTransactionLineVM { Model = line });

                    }
                }
            }

            else
            {
                using (var context = new ERPContext())
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
        }
    }
}
