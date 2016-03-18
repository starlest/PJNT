namespace PutraJayaNT.ViewModels.Customers.Sales
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Inventory;
    using Item;
    using Models.Inventory;
    using Models.Sales;
    using MVVMFramework;
    using Models.Salesman;
    using Utilities;
    using ViewModels.Sales;

    public class SalesNewEntryVM : ViewModelBase
    {
        private readonly SalesVM _parentVM;

        #region Backing Fields
        private string _remainingStock;
        private ItemVM _newEntryProduct;
        private Warehouse _newEntryWarehouse;
        private AlternativeSalesPrice _newEntrySelectedAlternativeSalesPrice;
        private decimal _newEntryPrice;
        private decimal? _newEntryDiscountPercent;
        private decimal _newEntryDiscount;
        private string _newEntryUnitName;
        private int? _newEntryPiecesPerUnit;
        private int? _newEntryUnits;
        private int? _newEntryPieces;
        private Salesman _newEntrySalesman;
        private bool _newEntrySubmitted;

        private ICommand _confirmNewEntryCommand;
        private ICommand _cancelNewEntryCommand;
        #endregion

        public SalesNewEntryVM(SalesVM parentVM)
        {
            _parentVM = parentVM;

            Warehouses = new ObservableCollection<Warehouse>();
            WarehouseProducts = new ObservableCollection<ItemVM>();
            Salesmans = new ObservableCollection<Salesman>();
            NewEntryAlternativeSalesPrices = new ObservableCollection<AlternativeSalesPrice>();

            UpdateWarehouses();
            UpdateSalesmans();

            _remainingStock = "0/0";
        }

        #region Collections
        public ObservableCollection<Warehouse> Warehouses { get; }

        public ObservableCollection<ItemVM> WarehouseProducts { get; }

        public ObservableCollection<Salesman> Salesmans { get; }

        public ObservableCollection<AlternativeSalesPrice> NewEntryAlternativeSalesPrices { get; }
        #endregion

        #region Properties
        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, () => NewEntryProduct);
                if (_newEntryProduct == null) return;
                SetNewEntryProductPropertiesToUI();
            }
        }

        public Warehouse NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set
            {
                SetProperty(ref _newEntryWarehouse, value, () => NewEntryWarehouse);

                if (_newEntryWarehouse == null)
                {
                    WarehouseProducts.Clear();
                    return;
                }

                UpdateWarehouseProducts();
            }
        }

        public AlternativeSalesPrice NewEntrySelectedAlternativeSalesPrice
        {
            get { return _newEntrySelectedAlternativeSalesPrice; }
            set
            {
                SetProperty(ref _newEntrySelectedAlternativeSalesPrice, value, () => NewEntrySelectedAlternativeSalesPrice);
                if (_newEntrySelectedAlternativeSalesPrice == null) return;
                NewEntryPrice = _newEntrySelectedAlternativeSalesPrice.SalesPrice;
                NewEntryDiscount = 0;
            }
        }

        public decimal NewEntryPrice
        {
            get { return _newEntryPrice; }
            set
            {
                SetProperty(ref _newEntryPrice, value, () => NewEntryPrice);
                NewEntryDiscount = 0;
            }
        }

        public decimal? NewEntryDiscountPercent
        {
            get { return _newEntryDiscountPercent; }
            set
            {
                if (value != null && (value < 0 || value > 100))
                {
                    MessageBox.Show("Please enter a value from the range of 0 - 100.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _newEntryDiscountPercent, value, () => NewEntryDiscountPercent);
                if (_newEntryDiscountPercent == null) return;
                NewEntryDiscount = (decimal) _newEntryDiscountPercent / 100 * _newEntryPrice;
                NewEntryDiscountPercent = null;
            }
        }

        public decimal NewEntryDiscount
        {
            get { return _newEntryDiscount; }
            set
            {
                if (value < 0 || value > _newEntryPrice)
                {
                    MessageBox.Show($"Please enter a value from the range of 0 - {_newEntryPrice}.", "Invalid Range", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _newEntryDiscount, value, () => NewEntryDiscount);
            }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, () => NewEntryUnitName); }
        }

        public int? NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, () => NewEntryPiecesPerUnit); }
        }

        public int? NewEntryUnits
        {
            get { return _newEntryUnits; }
            set { SetProperty(ref _newEntryUnits, value, () => NewEntryUnits); }
        }

        public int? NewEntryPieces
        {
            get { return _newEntryPieces; }
            set { SetProperty(ref _newEntryPieces, value, () => NewEntryPieces); }
        }

        public string RemainingStock
        {
            get { return _remainingStock; }
            set { SetProperty(ref _remainingStock, value, () => RemainingStock); }
        }

        public Salesman NewEntrySalesman
        {
            get { return _newEntrySalesman; }
            set { SetProperty(ref _newEntrySalesman, value, () => NewEntrySalesman); }
        }

        public bool NewEntrySubmitted
        {
            get { return _newEntrySubmitted; }
            set { SetProperty(ref _newEntrySubmitted, value, () => NewEntrySubmitted); }
        }
        #endregion

        #region Commands
        public ICommand ConfirmNewEntryCommand
        {
            get
            {
                return _confirmNewEntryCommand ?? (_confirmNewEntryCommand = new RelayCommand(() =>
                {
                    if (!AreAllFieldsFilled() || !IsNewEntryQuantityValid()) return;
                    AddNewEntryToTransaction();
                    SubmitNewEntry();
                    ResetEntryFields();
                }));
            }
        }

        public ICommand CancelNewEntryCommand
        {
            get
            {
                return _cancelNewEntryCommand ?? (_cancelNewEntryCommand = new RelayCommand(() =>
                {
                    ResetEntryFields();
                    NewEntryWarehouse = null;
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void SetNewEntryProductPropertiesToUI()
        {
            NewEntryPrice = _newEntryProduct.SalesPrice;
            NewEntryUnitName = _newEntryProduct.UnitName;
            NewEntryPiecesPerUnit = _newEntryProduct.PiecesPerUnit;
            var remainingStock = _parentVM.GetAvailableQuantity(_newEntryProduct.Model, _newEntryWarehouse);
            RemainingStock =
                $"{remainingStock / _newEntryProduct.PiecesPerUnit}/{remainingStock % _newEntryProduct.PiecesPerUnit}";

            UpdateNewEntryAlternativeSalesPrices();
        }

        private void UpdateWarehouses()
        {
            var oldSelectedWarehouse = _newEntryWarehouse;

            Warehouses.Clear();
            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();

                foreach (var warehouse in warehouses)
                    Warehouses.Add(warehouse);
            }

            UpdateSelectedWarehouse(oldSelectedWarehouse);
        }

        private void UpdateSelectedWarehouse(Warehouse oldSelectedWarehouse)
        {
            if (oldSelectedWarehouse == null) return;
            NewEntryWarehouse = oldSelectedWarehouse;
        }

        private void UpdateWarehouseProducts()
        {
            WarehouseProducts.Clear();

            using (var context = new ERPContext())
            {
                var stocks = context.Stocks
                    .Include("Item")
                    .Include("Warehouse")
                    .Where(item => item.Pieces > 0 && item.WarehouseID.Equals(_newEntryWarehouse.ID))
                    .OrderBy(item => item.Item.Name)
                    .ToList();

                foreach (var stock in stocks)
                    WarehouseProducts.Add(new ItemVM { Model = stock.Item });
            }
        }

        private void UpdateSalesmans()
        {
            Salesmans.Clear();
            using (var context = new ERPContext())
            {
                var salesmans = context.Salesmans.OrderBy(salesman => salesman.Name);
                foreach (var salesman in salesmans.Where(salesman => !salesman.Name.Equals(" ")))
                    Salesmans.Add(salesman);
            }
        }

        private void UpdateNewEntryAlternativeSalesPrices()
        {
            NewEntryAlternativeSalesPrices.Clear();
            using (var context = new ERPContext())
            {
                var alternativeSalesPrices = context.AlternativeSalesPrices.Where(e => e.ItemID.Equals(_newEntryProduct.ID)).OrderBy(price => price.Name);
                foreach (var alt in alternativeSalesPrices)
                    NewEntryAlternativeSalesPrices.Add(alt);
            }
        }

        private bool AreAllFieldsFilled()
        {
            if (_newEntryProduct != null && (_newEntryUnits != null || _newEntryPieces != null)
                && _newEntrySalesman != null) return true;
            MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private bool IsNewEntryQuantityValid()
        {
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits * _newEntryProduct.PiecesPerUnit ?? 0);
            var availableQuantity = _parentVM.GetAvailableQuantity(_newEntryProduct.Model, _newEntryWarehouse);

            if (availableQuantity >= newEntryQuantity && newEntryQuantity > 0) return true;
            MessageBox.Show(
                $"{_newEntryProduct.Name} has only {availableQuantity / _newEntryProduct.PiecesPerUnit} units, " +
                $"{availableQuantity % _newEntryProduct.PiecesPerUnit} pieces left.",
                "Insufficient Stock", MessageBoxButton.OK);
            return false;
        }

        private void AddNewEntryToTransaction()
        {
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits * _newEntryProduct.PiecesPerUnit ?? 0);
            foreach (var line in _parentVM.DisplayedSalesTransactionLines)
            {
                if (!line.Item.ItemID.Equals(_newEntryProduct.Model.ItemID) ||
                    !line.Warehouse.ID.Equals(_newEntryWarehouse.ID) || !_newEntryPrice.Equals(line.SalesPrice) ||
                    !_newEntryDiscount.Equals(line.Discount)) continue;

                line.Quantity += newEntryQuantity;
                line.UpdateTotal();
                ResetEntryFields();
                return;
            }
            var salesTransactionLineVM = MakeNewEntryLineVM();
            _parentVM.DisplayedSalesTransactionLines.Add(salesTransactionLineVM);

        }

        private SalesTransactionLineVM MakeNewEntryLineVM()
        {
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits * _newEntryProduct.PiecesPerUnit ?? 0);
            return new SalesTransactionLineVM
            {
                Model = new SalesTransactionLine
                {
                    Item = _newEntryProduct.Model,
                    SalesTransaction = _parentVM.Model,
                    SalesPrice = _newEntryPrice / _newEntryProduct.PiecesPerUnit,
                    Quantity = newEntryQuantity,
                    Warehouse = _newEntryWarehouse,
                    Discount = _newEntryDiscount / _newEntryProduct.PiecesPerUnit,
                    Total = (_newEntryPrice - _newEntryDiscount) / _newEntryProduct.PiecesPerUnit * newEntryQuantity,
                    Salesman = _newEntrySalesman
                }
            };
        }

        private void SubmitNewEntry()
        {
            NewEntrySubmitted = true;
            NewEntrySubmitted = false;
        }

        public void ResetEntryFields()
        {
            NewEntryPiecesPerUnit = null;
            NewEntryProduct = null;
            NewEntryPrice = 0;
            NewEntryDiscount = 0;
            NewEntryUnitName = null;
            NewEntryPieces = null;
            NewEntryUnits = null;
            RemainingStock = "0/0";
            NewEntrySalesman = null;
            NewEntrySelectedAlternativeSalesPrice = null;
            _parentVM.UpdateTransactionTotal();

            WarehouseProducts.Clear();
            NewEntryAlternativeSalesPrices.Clear();

            UpdateWarehouses();
            UpdateSalesmans();
        }
        #endregion
    }
}
