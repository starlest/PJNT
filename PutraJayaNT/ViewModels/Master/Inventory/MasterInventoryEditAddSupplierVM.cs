namespace PutraJayaNT.ViewModels.Master.Inventory
{
    using ViewModels.Suppliers;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using MVVMFramework;
    using Utilities;

    public class MasterInventoryEditAddSupplierVM : ViewModelBase
    {
        private readonly ObservableCollection<SupplierVM> _editSuppliers;
        private SupplierVM _selectedSupplierForAddition;
        private ICommand _editAddSupplierConfirmCommand;

        public MasterInventoryEditAddSupplierVM(ObservableCollection<SupplierVM> editSuppliers)
        {
            _editSuppliers = editSuppliers;
            SuppliersAvailableForAddition = new ObservableCollection<SupplierVM>();
            LoadSuppliersAvailableForAddition();
            _selectedSupplierForAddition = SuppliersAvailableForAddition.First();
        }

        public ObservableCollection<SupplierVM> SuppliersAvailableForAddition { get; }

        public SupplierVM SelectedSupplierForAddition
        {
            get { return _selectedSupplierForAddition; }
            set { SetProperty(ref _selectedSupplierForAddition, value, "SelectedSupplierForAddition"); }
        }

        public ICommand EditAddSupplierConfirmCommand
        {
            get
            {
                return _editAddSupplierConfirmCommand ?? (_editAddSupplierConfirmCommand = new RelayCommand(() =>
                {
                    _editSuppliers.Add(_selectedSupplierForAddition);
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        #region Helper Methods
        private void LoadSuppliersAvailableForAddition()
        {
            SuppliersAvailableForAddition.Clear();
            using (var context = new ERPContext())
            {
                var allSuppliersFromDatabase = context.Suppliers.Where(
                    supplier => !supplier.Name.Equals("-"))
                    .OrderBy(supplier => supplier.Name)
                    .ToList();
                foreach (var supplier in from supplier in allSuppliersFromDatabase
                    let supplierVM = new SupplierVM {Model = supplier}
                    where !_editSuppliers.Contains(supplierVM)
                    select supplier)
                    SuppliersAvailableForAddition.Add(new SupplierVM { Model = supplier });
            }
        }
        #endregion
    }
}
