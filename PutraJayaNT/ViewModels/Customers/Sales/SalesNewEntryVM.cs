namespace ECRP.ViewModels.Customers.Sales
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.Inventory;
    using Models.Sales;
    using Models.Salesman;
    using MVVMFramework;
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
        private string _newEntryUnit;
        private string _newEntrySecondaryUnit;
        private bool _isSecondaryUnitUsed;
        private int? _newEntryUnits;
        private int? _newEntrySecondaryUnits;
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

            _remainingStock = "0/0/0";
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

        public string NewEntryUnit
        {
            get { return _newEntryUnit; }
            set { SetProperty(ref _newEntryUnit, value, () => NewEntryUnit); }
        }

        public bool IsSecondaryUnitUsed
        {
            get { return _isSecondaryUnitUsed; }
            set { SetProperty(ref _isSecondaryUnitUsed, value, () => IsSecondaryUnitUsed); }
        }

        public string NewEntrySecondaryUnit
        {
            get { return _newEntrySecondaryUnit; }
            set { SetProperty(ref _newEntrySecondaryUnit, value, () => NewEntrySecondaryUnit); }
        }

        public int? NewEntryUnits
        {
            get { return _newEntryUnits; }
            set { SetProperty(ref _newEntryUnits, value, () => NewEntryUnits); }
        }

        public int? NewEntrySecondaryUnits
        {
            get { return _newEntrySecondaryUnits; }
            set { SetProperty(ref _newEntrySecondaryUnits, value, () => NewEntrySecondaryUnits); }
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
                    if (!AreAllFieldsFilled() || !IsNewEntryQuantityValid() || !ArePriceAndDiscountFieldsValid()) return;
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
            NewEntryUnit = _newEntryProduct.PiecesPerSecondaryUnit != 0 ? _newEntryProduct.UnitName + "/" + 
                _newEntryProduct.PiecesPerUnit / _newEntryProduct.PiecesPerSecondaryUnit : 
                _newEntryProduct.UnitName + "/" + _newEntryProduct.PiecesPerUnit;
            NewEntrySecondaryUnit = _newEntryProduct.PiecesPerSecondaryUnit  != 0 ?_newEntryProduct.SecondaryUnitName + "/" + _newEntryProduct.PiecesPerSecondaryUnit : null;
            var remainingStock = _parentVM.GetAvailableQuantity(_newEntryProduct.Model, _newEntryWarehouse);
            if (_newEntryProduct.PiecesPerSecondaryUnit == 0)
            {
                RemainingStock =
                    $"{remainingStock/_newEntryProduct.PiecesPerUnit}/0/{remainingStock%_newEntryProduct.PiecesPerUnit}";
                IsSecondaryUnitUsed = false;
            }
            else
            {
                RemainingStock =
                    $"{remainingStock/_newEntryProduct.PiecesPerUnit}/{remainingStock%_newEntryProduct.PiecesPerUnit/_newEntryProduct.PiecesPerSecondaryUnit}/" +
                    $"{remainingStock%_newEntryProduct.PiecesPerUnit%_newEntryProduct.PiecesPerSecondaryUnit}";
                IsSecondaryUnitUsed = true;
            }
            UpdateNewEntryAlternativeSalesPrices();
        }

        private void UpdateWarehouses()
        {
            var oldSelectedWarehouse = _newEntryWarehouse;

            Warehouses.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var warehouses = context.Warehouses.OrderBy(warehouse => warehouse.Name);
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

            using (var context = UtilityMethods.createContext())
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
            using (var context = UtilityMethods.createContext())
            {
                var salesmans = context.Salesmans.OrderBy(salesman => salesman.Name);
                foreach (var salesman in salesmans.Where(salesman => !salesman.Name.Equals(" ")))
                    Salesmans.Add(salesman);
            }
        }

        private void UpdateNewEntryAlternativeSalesPrices()
        {
            NewEntryAlternativeSalesPrices.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var alternativeSalesPrices = context.AlternativeSalesPrices.Where(e => e.ItemID.Equals(_newEntryProduct.ID)).OrderBy(price => price.Name);
                foreach (var alt in alternativeSalesPrices)
                    NewEntryAlternativeSalesPrices.Add(alt);
            }
        }

        private bool AreAllFieldsFilled()
        {
            if (_newEntryProduct != null && (_newEntryUnits != null || _newEntrySecondaryUnits != null || _newEntryPieces != null)
                && _newEntrySalesman != null) return true;
            MessageBox.Show("Please enter all fields", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private bool IsNewEntryQuantityValid()
        {
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits * _newEntryProduct.PiecesPerUnit ?? 0) + 
                (_newEntrySecondaryUnits * _newEntryProduct.PiecesPerSecondaryUnit ?? 0);
            var availableQuantity = _parentVM.GetAvailableQuantity(_newEntryProduct.Model, _newEntryWarehouse);

            if (availableQuantity >= newEntryQuantity && newEntryQuantity > 0) return true;
            MessageBox.Show(
                $"{_newEntryProduct.Name} has only {availableQuantity / _newEntryProduct.PiecesPerUnit} units, " +
                $"{availableQuantity % _newEntryProduct.PiecesPerUnit} pieces left.",
                "Insufficient Stock", MessageBoxButton.OK);
            return false;
        }

        private bool ArePriceAndDiscountFieldsValid()
        {
            if (_newEntryPrice >= 0 && _newEntryDiscount >= 0 && _newEntryDiscount <= _newEntryPrice)
                return true;
            MessageBox.Show("Please check that the price and discount fields are valid.", "Invalid Field(s)", MessageBoxButton.OK);
            return false;
        }

        private void AddNewEntryToTransaction()
        {
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits * _newEntryProduct.PiecesPerUnit ?? 0) +
                (_newEntrySecondaryUnits * _newEntryProduct.PiecesPerSecondaryUnit ?? 0);
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
            var newEntryQuantity = (_newEntryPieces ?? 0) + (_newEntryUnits * _newEntryProduct.PiecesPerUnit ?? 0) +
                (_newEntrySecondaryUnits * _newEntryProduct.PiecesPerSecondaryUnit ?? 0);
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
            IsSecondaryUnitUsed = false;
            NewEntryProduct = null;
            NewEntryPrice = 0;
            NewEntryDiscount = 0;
            NewEntryUnit = null;
            NewEntrySecondaryUnit = null;
            NewEntryUnits = null;
            NewEntrySecondaryUnits = null;
            NewEntryPieces = null;
            RemainingStock = "0/0/0";
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
