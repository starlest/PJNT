using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace PutraJayaNT.ViewModels
{
    class MasterSuppliersVM : ViewModelBase
    {
        ObservableCollection<SupplierVM> _suppliers;
        ObservableCollection<string> _categories;
        ObservableCollection<ItemVM> _items;
        ObservableCollection<SupplierVM> _displayedSuppliers;

        SupplierVM _selectedSupplier;
        string _selectedCategory;
        ItemVM _selectedItem;
        SupplierVM _selectedLine;

        string _newEntryName;
        string _newEntryAddress;
        int? _newEntryGSTID;

        ICommand _newEntryCommand;
        ICommand _cancelEntryCommand;
        ICommand _activateCommand;

        // Edit Window Properties
        ICommand _editCommand;
        ICommand _cancelEditCommand;
        ICommand _confirmEditCommand;

        string _editName;
        string _editAddress;
        int _editGSTID;

        bool _isEditWindowNotOpen;
        Visibility _editWindowVisibility;

        public MasterSuppliersVM()
        {
            _suppliers = new ObservableCollection<SupplierVM>();
            _categories = new ObservableCollection<string>();
            _items = new ObservableCollection<ItemVM>();
            _displayedSuppliers = new ObservableCollection<SupplierVM>();

            _categories.Add("All");

            using (var context = new ERPContext())
            {
                var items = context.Inventory.Include("Category");
                foreach (var item in items)
                {
                    if (!_categories.Contains(item.Category.Name))
                        _categories.Add(item.Category.Name);
                }
            }

            SelectedCategory = "All";

            UpdateItems();
            UpdateSuppliers();

            _editWindowVisibility = Visibility.Collapsed;
            _isEditWindowNotOpen = true;
        }

        public ObservableCollection<SupplierVM> Suppliers
        {
            get { return _suppliers; }
        }

        public ObservableCollection<string> Categories
        {
            get { return _categories; }
        }

        public ObservableCollection<ItemVM> Items
        {
            get { return _items; }
        }

        public ObservableCollection<SupplierVM> DisplayedSuppliers
        {
            get { return _displayedSuppliers; }
        }

        public SupplierVM SelectedSupplier
        {
            get
            {
                return _selectedSupplier;
            }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedSupplier, null, () => SelectedSupplier);
                    return;
                }

                if (_suppliers.Contains(value))
                {
                    SelectedCategory = null;
                    SelectedItem = null;

                    UpdateSuppliers();

                    _displayedSuppliers.Clear();
                    foreach (var supplier in _suppliers)
                    {
                        if (supplier.ID == value.ID)
                        {
                            _displayedSuppliers.Add(supplier);
                            SetProperty(ref _selectedSupplier, supplier, () => SelectedSupplier);
                            break;
                        }
                    }
                }
            }
        }

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedCategory, null, () => SelectedCategory);
                    return;
                }

                UpdateSuppliers();

                if (_categories.Contains(value))
                {
                    SelectedSupplier = null;
                    SelectedItem = null;

                    SetProperty(ref _selectedCategory, value, () => SelectedCategory);
                    _displayedSuppliers.Clear();
                    
                    if (value == "All")
                    {
                        foreach (var supplier in _suppliers)
                        {
                            _displayedSuppliers.Add(supplier);
                        }
                    }

                    else
                    {
                        using (var context = new ERPContext())
                        {
                            var categorySuppliers = new List<Supplier>();
                            var items = context.Inventory.Include("Category").Include("Suppliers");
                            var suppliers = context.Suppliers;
                            foreach (var item in items)
                            {
                                if (item.Category.Name == value)
                                {
                                    foreach (var supplier in item.Suppliers)
                                    {
                                        if (!categorySuppliers.Contains(supplier))
                                            categorySuppliers.Add(supplier);
                                    }
                                }
                            }

                            foreach (var supplier in categorySuppliers)
                            {
                                _displayedSuppliers.Add(new SupplierVM { Model = supplier });
                            }
                        }
                    }                    
                }
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedItem, null, () => SelectedItem);
                    return;
                }

                UpdateItems();
                if (_items.Contains(value))
                {
                    SelectedSupplier = null;
                    SelectedCategory = null;

                    SetProperty(ref _selectedItem, value, () => SelectedItem);
                    _displayedSuppliers.Clear();

                    using (var context = new ERPContext())
                    {
                        var items = context.Inventory.Include("Suppliers");
                        foreach (var item in items)
                        {
                            if (item.ItemID == value.ID)
                            {
                                foreach (var supplier in item.Suppliers)
                                {
                                    _displayedSuppliers.Add(new SupplierVM { Model = supplier });
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public SupplierVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, () => NewEntryName); }
        }

        public string NewEntryAddress
        {
            get { return _newEntryAddress; }
            set { SetProperty(ref _newEntryAddress, value, () => NewEntryAddress); }
        }

        public int? NewEntryGSTID
        {
            get { return _newEntryGSTID; }
            set { SetProperty(ref _newEntryGSTID, value, () => NewEntryGSTID); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                if (_newEntryName == null || _newEntryAddress == null)
                {
                    MessageBox.Show("Please enter supplier's Name and Address", "Missing Fields", MessageBoxButton.OK);
                    return;
                }

                if (MessageBox.Show("Confirm adding this supplier?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var newSupplier = new Supplier { Name = _newEntryName, Address = _newEntryAddress };
                    if (_newEntryGSTID != null) newSupplier.GSTID = (int)_newEntryGSTID;


                    using (UnitOfWork uow = new UnitOfWork())
                    {
                            // Add supplier to the database
                            uow.SupplierRepository.Add(newSupplier);

                            // Add a ledger account for this supplier in the database
                            LedgerAccount account = new LedgerAccount
                            {
                                Name = _newEntryName + " Accounts Payable",
                                Notes = "Accounts Payable",
                                Class = "Liability"
                            };

                            uow.LedgerAccountRepository.Add(account);
                            uow.LedgerGeneralRepository.Add(new LedgerGeneral
                            {
                                LedgerAccount = account,
                                PeriodYear = 2015,
                                Period = 1,
                            });

                            // Add an account Balance for this supplier in the database
                            uow.LedgerAccountBalanceRepository.Add(new LedgerAccountBalance
                            {
                                LedgerAccount = account,
                                PeriodYear = DateTime.Now.Year,
                            });

                            uow.SaveChanges();
                        }

                        UpdateSuppliers();
                        SelectedCategory = "All";
                        ResetEntryFields();
                    }
                }));
            }
        }

        public ICommand CancelEntryCommand
        {
            get
            {
                return _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(() =>
                {
                    ResetEntryFields();
                }));
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(() =>
                {
                    if (_selectedLine == null)
                    {
                        MessageBox.Show("Please select a supplier to edit.", "No Selection", MessageBoxButton.OK);
                        return;
                    }
                    else
                    {
                        EditName = SelectedLine.Name;
                        EditAddress = SelectedLine.Address;
                        EditGSTID = SelectedLine.GSTID;
                        EditWindowVisibility = Visibility.Visible;
                        IsEditWindowNotOpen = false;
                    }
                }));
            }
        }

        public ICommand CancelEditCommand
        {
            get
            {
                return _cancelEditCommand ?? (_cancelEditCommand = new RelayCommand(() =>
                {
                    EditWindowVisibility = Visibility.Collapsed;
                    IsEditWindowNotOpen = true;
                }));
            }
        }

        public ICommand ConfirmEditCommand
        {
            get
            {
                return _confirmEditCommand ?? (_confirmEditCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        var searchName = string.Format("{0} Accounts Payable", _selectedLine.Name);
                        _selectedLine.Name = _editName;
                        _selectedLine.Address = _editAddress;
                        _selectedLine.GSTID = _editGSTID;

                        // Update the supplier and its ledger account in the database
                        using (var context = new ERPContext())
                        {
                            context.Suppliers.Attach(_selectedLine.Model);
                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(_selectedLine.Model, EntityState.Modified);

                            var account = context.Ledger_Accounts
                            .Where(e => e.Name == searchName)
                            .FirstOrDefault();
                            account.Name = string.Format("{0} Accounts Payable", _editName);

                            context.Ledger_Accounts.Attach(account);
                            ((IObjectContextAdapter)context).ObjectContext.
                            ObjectStateManager.ChangeObjectState(account, EntityState.Modified);

                            context.SaveChanges();
                        }

                        EditWindowVisibility = Visibility.Collapsed;
                        IsEditWindowNotOpen = true;
                    }
                }));
            }
        }

        public string EditName
        {
            get { return _editName; }
            set { SetProperty(ref _editName, value, () => EditName); }
        }

        public string EditAddress
        {
            get { return _editAddress; }
            set { SetProperty(ref _editAddress, value, () => EditAddress); }
        }

        public int EditGSTID
        {
            get { return _editGSTID; }
            set { SetProperty(ref _editGSTID, value, () => EditGSTID); }
        }

        public Visibility EditWindowVisibility
        {
            get { return _editWindowVisibility; }
            set { SetProperty(ref _editWindowVisibility, value, "EditWindowVisibility"); }
        }

        public bool IsEditWindowNotOpen
        {
            get { return _isEditWindowNotOpen; }
            set { SetProperty(ref _isEditWindowNotOpen, value, "IsEditWindowNotOpen"); }
        }

        public ICommand ActivateCommand
        {
            get
            {
                return _activateCommand ?? (_activateCommand = new RelayCommand(() =>
                {
                    if (_selectedLine != null && MessageBox.Show(string.Format("Confirm activation/deactivation of {0}?", SelectedLine.Name), "Confirmation", MessageBoxButton.YesNo)
                        == MessageBoxResult.Yes)
                    {
                        using (var context = new ERPContext())
                        {
                            var supplier = context.Suppliers
                            .Include(e => e.Items)
                            .Where(e => e.ID == _selectedLine.ID)
                            .FirstOrDefault<Supplier>();

                            if (_selectedLine.Active == true)
                            {
                                _selectedLine.Active = false;
                                _selectedLine.Items.Clear();
                                supplier.Active = false;
                                foreach (var item in supplier.Items.ToList())
                                    supplier.Items.Remove(item);
                            }
                            else
                            {
                                _selectedLine.Active = true;
                                supplier.Active = true;
                            }

                            context.SaveChanges();
                        }
                    }

                }));
            }
        }

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryAddress = null;
            NewEntryGSTID = null;
        }

        private void UpdateItems()
        {
            // Update items from database
            _items.Clear();
            using (UnitOfWork uow = new UnitOfWork())
            {
                var items = uow.ItemRepository.GetAll();
                foreach (var item in items)
                {
                    if (item.Active == true)
                        _items.Add(new ItemVM { Model = item });
                }
            }
        }

        private void UpdateSuppliers()
        {
            // Update suppliers from database
            _suppliers.Clear();
            using (var context = new ERPContext())
            {
                var suppliers = context.Suppliers.Include("Items");
                foreach (Supplier s in suppliers)
                {
                    SupplierVM svm = new SupplierVM { Model = s };
                    _suppliers.Add(svm);
                }
            }
        }
    }
}
