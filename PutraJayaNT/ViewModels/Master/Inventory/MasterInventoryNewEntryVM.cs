using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;

namespace PutraJayaNT.ViewModels.Master.Inventory
{
    public class MasterInventoryNewEntryVM : ViewModelBase
    {

        #region Backing Fields
        private string _newEntryID;
        private string _newEntryName;
        private Category _newEntryCategory;
        private Supplier _newEntrySupplier;
        private int _newEntryPiecesPerUnit;
        private string _newEntryUnitName;
        private decimal? _newEntryPurchasePrice;
        private decimal? _newEntrySalesPrice;
        private ICommand _newEntryCommand;
        private ICommand _cancelEntryCommand;

        private MasterInventoryVM _parentVM;
        #endregion

        public MasterInventoryNewEntryVM(MasterInventoryVM parentVM)
        {
            _parentVM = parentVM;
            
            _newEntryPiecesPerUnit = 1;
        }

        #region Properties
        public string NewEntryID
        {
            get { return _newEntryID; }
            set { SetProperty(ref _newEntryID, value, "NewEntryID"); }
        }

        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, "NewEntryName"); }
        }

        public Category NewEntryCategory
        {
            get { return _newEntryCategory; }
            set { SetProperty(ref _newEntryCategory, value, "NewEntryCategory"); }
        }

        public Supplier NewEntrySupplier
        {
            get { return _newEntrySupplier; }
            set { SetProperty(ref _newEntrySupplier, value, "NewEntrySupplier"); }
        }

        public int NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, "NewEntryPiecesPerUnit"); }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, "NewEntryUnitName"); }
        }

        public decimal? NewEntrySalesPrice
        {
            get { return _newEntrySalesPrice; }
            set { SetProperty(ref _newEntrySalesPrice, value, "NewEntrySalesPrice"); }
        }

        public decimal? NewEntryPurchasePrice
        {
            get { return _newEntryPurchasePrice; }
            set { SetProperty(ref _newEntryPurchasePrice, value, "NewEntryPurchasePrice"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!AreAllEntryFieldsFilled()) return;
                    if (MessageBox.Show("Confirm adding this product?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                    var newEntryItem = MakeNewEntryItem();
                    AddItemToDatabase(newEntryItem);
                    ResetEntryFields();
                }));
            }
        }

        public ICommand CancelEntryCommand => _cancelEntryCommand ?? (_cancelEntryCommand = new RelayCommand(ResetEntryFields));
        #endregion

        #region Helper Methods
        private static void AttachItemSupplierToDatabaseContext(ERPContext context, Item item)
        {
            var supplier = item.Suppliers.First();
            item.Suppliers.RemoveAt(0);
            supplier = context.Suppliers.FirstOrDefault(e => e.ID.Equals(supplier.ID));
            item.Suppliers.Add(supplier);
        }

        private static void AttachItemCategoryToDatabaseContext(ERPContext context, Item item)
        {
            item.Category = context.Categories.First(category => category.ID.Equals(item.Category.ID));
        }

        private static void AddItemToDatabaseContext(ERPContext context, Item item)
        {
            AttachItemSupplierToDatabaseContext(context, item);
            AttachItemCategoryToDatabaseContext(context, item);
            context.Inventory.Add(item);
        }

        private static void AddItemToDatabase(Item item)
        {
            using (var context = new ERPContext())
            {
                AddItemToDatabaseContext(context, item);
                context.SaveChanges();
            }
        }

        private Item MakeNewEntryItem()
        {
            Debug.Assert(_newEntryPurchasePrice != null, "_newEntryPurchasePrice != null");
            Debug.Assert(_newEntrySalesPrice != null, "_newEntrySalesPrice != null");
            var newEntryItem = new Item
            {
                ItemID = _newEntryID,
                Name = _newEntryName,
                Category = _newEntryCategory,
                UnitName = _newEntryUnitName,
                PiecesPerUnit = _newEntryPiecesPerUnit,
                PurchasePrice = (decimal) _newEntryPurchasePrice/_newEntryPiecesPerUnit,
                SalesPrice = (decimal) _newEntrySalesPrice/_newEntryPiecesPerUnit
            };
            newEntryItem.Suppliers.Add(_newEntrySupplier);
            return newEntryItem;
        }

        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryID != null || _newEntryName != null ||
                _newEntryCategory != null || _newEntrySupplier != null ||
                _newEntrySalesPrice == null || _newEntryPurchasePrice != null ||
                _newEntryPiecesPerUnit <= 0 || _newEntryUnitName != null)
                return true;

            MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private void ResetEntryFields()
        {
            NewEntryID = null;
            NewEntryName = null;
            NewEntryCategory = null;
            NewEntrySupplier = null;
            NewEntryPiecesPerUnit = 1;
            NewEntryUnitName = null;
            NewEntrySalesPrice = null;
            NewEntryPurchasePrice = null;
        }
        #endregion
    }
}
