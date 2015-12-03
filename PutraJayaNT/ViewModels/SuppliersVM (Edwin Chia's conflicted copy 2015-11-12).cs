using MVVMFramework;
using PUJASM.ERP.Models;
using PUJASM.ERP.Models.Accounting;
using PUJASM.ERP.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PUJASM.ERP.ViewModels
{
    class SuppliersVM : ViewModelBase
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

        public SuppliersVM()
        {
            _suppliers = new ObservableCollection<SupplierVM>();
            _categories = new ObservableCollection<string>();
            _items = new ObservableCollection<ItemVM>();
            _displayedSuppliers = new ObservableCollection<SupplierVM>();

            _categories.Add("All");

            using (UnitOfWork uow = new UnitOfWork())
            {
                var items = uow.ItemRepository.GetAll();
                foreach (var item in items)
                {
                    if (!_categories.Contains(item.Category))
                        _categories.Add(item.Category);
                }
            }

            SelectedCategory = "All";

            UpdateItems();
            UpdateSuppliers();
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
                        using (UnitOfWork uow = new UnitOfWork())
                        {
                            var categorySuppliers = new List<Supplier>();
                            var items = uow.ItemRepository.GetAll();
                            var suppliers = uow.SupplierRepository.GetAll();
                            foreach (var item in items)
                            {
                                if (item.Category == value)
                                {
                                    if (!categorySuppliers.Contains(item.Supplier))
                                        categorySuppliers.Add(item.Supplier);
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

                    using (UnitOfWork uow = new UnitOfWork())
                    {
                        var items = uow.ItemRepository.GetAll();
                        foreach (var item in items)
                        {
                            if (item.ID == value.ID)
                            {
                                _displayedSuppliers.Add(new SupplierVM { Model = item.Supplier });
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
            set
            {
                SetProperty(ref _newEntryName, value, () => NewEntryName);
            }
        }

        public string NewEntryAddress
        {
            get { return _newEntryAddress; }
            set
            {
                SetProperty(ref _newEntryAddress, value, () => NewEntryAddress);
            }
        }

        public int? NewEntryGSTID
        {
            get { return _newEntryGSTID; }
            set
            {
                SetProperty(ref _newEntryGSTID, value, () => NewEntryGSTID);
            }
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

                        // Add a ledger account for this customer in the database
                        LedgerAccount account = new LedgerAccount
                        {
                            Name = _newEntryName + " Accounts Payable",
                            Class = "Liability"
                        };

                        uow.LedgerAccountRepository.Add(account);
                        uow.LedgerGeneralRepository.Add(new LedgerGeneral
                        {
                            PeriodYear = 2015,
                            Period = 1,
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
                    _items.Add(new ItemVM { Model = item });
                }
            }
        }

        private void UpdateSuppliers()
        {
            // Update suppliers from database
            _suppliers.Clear();
            using (UnitOfWork uow = new UnitOfWork())
            {
                var suppliers = uow.SupplierRepository.GetAll();
                foreach (Supplier s in suppliers)
                {
                    SupplierVM svm = new SupplierVM { Model = s };
                    _suppliers.Add(svm);
                }
            }
        }
    }
}
