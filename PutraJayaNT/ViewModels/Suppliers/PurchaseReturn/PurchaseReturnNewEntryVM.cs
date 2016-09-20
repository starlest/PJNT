namespace ECRP.ViewModels.Suppliers.PurchaseReturn
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.Purchase;
    using MVVMFramework;
    using Utilities;
    using ViewModels.Purchase;

    internal class PurchaseReturnNewEntryVM : ViewModelBase
    {
        private readonly PurchaseReturnVM _parentVM;

        #region Backing Fields
        private WarehouseVM _purchaseReturnEntryWarehouse;
        private string _purchaseReturnEntryProduct;
        private int _purchaseReturnEntryUnits;
        private int _purchaseReturnEntryPieces;
        private string _purchaseReturnEntryAvailableQuantity;
        private decimal _purchaseReturnEntryPrice;
        private ICommand _purchaseReturnEntryAddCommand;
        #endregion

        public PurchaseReturnNewEntryVM(PurchaseReturnVM parentVM)
        {
            _parentVM = parentVM;
            _purchaseReturnEntryAvailableQuantity = "0/0";
        }

        public ObservableCollection<WarehouseVM> Warehouses => _parentVM.Warehouses;

        public ObservableCollection<PurchaseReturnTransactionLineVM> PurchaseReturnTransactionLines => _parentVM.PurchaseReturnTransactionLines;

        #region Return Entry Properties
        public WarehouseVM PurchaseReturnEntryWarehouse
        {
            get { return _purchaseReturnEntryWarehouse; }
            set
            {
                SetProperty(ref _purchaseReturnEntryWarehouse, value, () => PurchaseReturnEntryWarehouse);
                if (_purchaseReturnEntryWarehouse == null) return;
                SetPurchaseReturnEntryAvailableQuantity();
            }
        }

        public string PurchaseReturnEntryProduct
        {
            get { return _purchaseReturnEntryProduct; }
            set { SetProperty(ref _purchaseReturnEntryProduct, value, () => PurchaseReturnEntryProduct); }
        }

        public int PurchaseReturnEntryUnits
        {
            get { return _purchaseReturnEntryUnits; }
            set { SetProperty(ref _purchaseReturnEntryUnits, value, () => PurchaseReturnEntryUnits); }
        }

        public int PurchaseReturnEntryPieces
        {
            get { return _purchaseReturnEntryPieces; }
            set { SetProperty(ref _purchaseReturnEntryPieces, value, "PurchaseReturnEntryPieces"); }
        }

        public string PurchaseReturnEntryAvailableQuantity
        {
            get { return _purchaseReturnEntryAvailableQuantity; }
            set { SetProperty(ref _purchaseReturnEntryAvailableQuantity, value, () => PurchaseReturnEntryAvailableQuantity); }
        }

        public decimal PurchaseReturnEntryPrice
        {
            get { return _purchaseReturnEntryPrice; }
            set { SetProperty(ref _purchaseReturnEntryPrice, value, "PurchaseReturnEntryPrice"); }
        }

        public ICommand PurchaseReturnEntryAddCommand
        {
            get
            {
                return _purchaseReturnEntryAddCommand ?? (_purchaseReturnEntryAddCommand = new RelayCommand(() =>
                {
                    if (!IsTherePurchaseTransactionLineSelected() || !IsReturnEntryPriceValid() || !IsReturnEntryQuantityValid()) return;
                    AddEntryToPurchaseReturnTransactionLines();
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private bool IsTherePurchaseTransactionLineSelected()
        {
            if (_purchaseReturnEntryProduct != null && _parentVM.SelectedPurchaseTransactionLine != null) return true;
            MessageBox.Show("No product is selected", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private bool IsReturnEntryPriceValid()
        {
            var maximumReturnPrice = _parentVM.SelectedPurchaseTransactionLine.PurchasePrice - _parentVM.SelectedPurchaseTransactionLine.GetNetDiscount();
            if (_purchaseReturnEntryPrice >= 0 && _purchaseReturnEntryPrice <= maximumReturnPrice) return true;
            MessageBox.Show($"The valid return price is 0 - {maximumReturnPrice}", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private bool IsReturnEntryQuantityValid()
        {
            var availableReturnQuantity = GetAvailableReturnQuantity();
            var purchaseReturnEntryQuantity = _purchaseReturnEntryUnits * _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit + _purchaseReturnEntryPieces;
            if (purchaseReturnEntryQuantity <= availableReturnQuantity && purchaseReturnEntryQuantity > 0)
                return true;
            MessageBox.Show(
                $"The valid return quantity is {availableReturnQuantity / _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit} units {availableReturnQuantity % _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit} pieces",
                "Invalid Quantity Input", MessageBoxButton.OK);
            return false;
        }

        private void SetPurchaseReturnEntryAvailableQuantity()
        {
            var availableQuantity = GetAvailableReturnQuantity();
            PurchaseReturnEntryAvailableQuantity = availableQuantity/
                                                   _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit + "/" +
                                                   availableQuantity%
                                                   _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit;
        }

        private int GetAvailableReturnQuantity()
        {
            var availableQuantity = _parentVM.SelectedPurchaseTransactionLine.Quantity - _parentVM.SelectedPurchaseTransactionLine.SoldOrReturned;
            var stock = UtilityMethods.GetRemainingStock(_parentVM.SelectedPurchaseTransactionLine.Item,
                _purchaseReturnEntryWarehouse.Model);
            if (stock < availableQuantity) availableQuantity = stock;
            foreach (var line in PurchaseReturnTransactionLines)
            {
                if (line.Item.ItemID.Equals(_parentVM.SelectedPurchaseTransactionLine.Item.ItemID) &&
                    line.Warehouse.ID.Equals(_parentVM.SelectedPurchaseTransactionLine.Warehouse.ID) &&
                    line.Discount.Equals(_parentVM.SelectedPurchaseTransactionLine.Discount) &&
                    line.PurchasePrice.Equals(_parentVM.SelectedPurchaseTransactionLine.PurchasePrice))
                {
                    availableQuantity -= line.Quantity;
                }
            }
            return availableQuantity;
        }

        private bool IsPurchaseReturnTransactionLineCombinableWithNewEntry(PurchaseReturnTransactionLineVM line)
        {
            return line.Item.ItemID.Equals(_parentVM.SelectedPurchaseTransactionLine.Item.ItemID) &&
                   line.Warehouse.ID.Equals(_parentVM.SelectedPurchaseTransactionLine.Warehouse.ID) &&
                   line.ReturnWarehouse.ID.Equals(_purchaseReturnEntryWarehouse.ID) &&
                   line.PurchasePrice.Equals(_parentVM.SelectedPurchaseTransactionLine.PurchasePrice) &&
                   line.Discount.Equals(_parentVM.SelectedPurchaseTransactionLine.Discount) &&
                   line.ReturnPrice.Equals(_purchaseReturnEntryPrice);
        }

        private void AddEntryToPurchaseReturnTransactionLines()
        {
            foreach (var line in PurchaseReturnTransactionLines.Where(IsPurchaseReturnTransactionLineCombinableWithNewEntry))
            {
                CombinePurchaseReturnLineWithNewEntry(line);
                return;
            }
            var newEntryPurchaseReturnLineVM = MakeNewEntryPurchaseReturnTransactionLine();
            PurchaseReturnTransactionLines.Add(newEntryPurchaseReturnLineVM);
            _parentVM.PurchaseReturnTransactionNetTotal += newEntryPurchaseReturnLineVM.Total;
        }

        private void CombinePurchaseReturnLineWithNewEntry(PurchaseReturnTransactionLineVM line)
        {
            var purchaseReturnEntryQuantity = _purchaseReturnEntryUnits * _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit + _purchaseReturnEntryPieces;
            line.Quantity += purchaseReturnEntryQuantity;
            line.Total += purchaseReturnEntryQuantity * _purchaseReturnEntryPrice;
            _parentVM.PurchaseReturnTransactionNetTotal += purchaseReturnEntryQuantity * _purchaseReturnEntryPrice;
        }

        private PurchaseReturnTransactionLineVM MakeNewEntryPurchaseReturnTransactionLine()
        {
            var purchaseReturnEntryQuantity = _purchaseReturnEntryUnits * _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit + _purchaseReturnEntryPieces;
            var purchaseReturnTransactionLine = new PurchaseReturnTransactionLine
            {
                PurchaseReturnTransaction = _parentVM.Model,
                Item = _parentVM.SelectedPurchaseTransactionLine.Item,
                Warehouse = _parentVM.SelectedPurchaseTransactionLine.Warehouse,
                PurchasePrice = _parentVM.SelectedPurchaseTransactionLine.PurchasePrice / _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit,
                Discount = _parentVM.SelectedPurchaseTransactionLine.Discount / _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit,
                ReturnPrice = _purchaseReturnEntryPrice / _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit,
                Quantity = purchaseReturnEntryQuantity,
                Total = purchaseReturnEntryQuantity * _purchaseReturnEntryPrice / _parentVM.SelectedPurchaseTransactionLine.Item.PiecesPerUnit,
                ReturnWarehouse = _purchaseReturnEntryWarehouse.Model
            };
            return new PurchaseReturnTransactionLineVM { Model = purchaseReturnTransactionLine };
        }

        public void ResetEntryFields()
        {
            _parentVM.SelectedPurchaseTransactionLine = null;
            PurchaseReturnEntryProduct = null;
            PurchaseReturnEntryUnits = 0;
            PurchaseReturnEntryPieces = 0;
            PurchaseReturnEntryPrice = 0;
            PurchaseReturnEntryAvailableQuantity = "0/0";
            PurchaseReturnEntryWarehouse = null;
        }
        #endregion
    }
}
