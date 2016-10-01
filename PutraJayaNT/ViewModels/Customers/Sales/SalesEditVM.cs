namespace ECRP.ViewModels.Customers.Sales
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Inventory;
    using Models.Sales;
    using Models.Salesman;
    using MVVMFramework;
    using Utilities;
    using ViewModels.Sales;

    public class SalesEditVM : ViewModelBase
    {
        private readonly SalesVM _parentVM;
        private readonly SalesTransactionLineVM _selectedLine;
        public Action CloseWindow { get; set; }

        #region Edit Line Backing Fields
        private AlternativeSalesPrice _selectedAlternativeSalesPrice;
        private int _editLineUnits;
        private int? _editLineSecondaryUnits;
        private int _editLinePieces;
        private decimal _editLineDiscount;
        private decimal _editLineSalesPrice;
        private Salesman _editLineSalesman;
        private ICommand _editConfirmCommand;

        #endregion

        public SalesEditVM(SalesVM parentVM)
        {
            _parentVM = parentVM;
            _selectedLine = _parentVM.SelectedLine;

            Salesmans = new ObservableCollection<Salesman>();
            Warehouses = new ObservableCollection<Warehouse>();
            AlternativeSalesPrices = new ObservableCollection<AlternativeSalesPrice>();

            UpdateSalesmans();
            UpdateWarehouses();
            UpdateAlternativeSalesPrices();
            SetEditProperties();
        }

        public bool InvoiceNotIssued => _parentVM.InvoiceNotIssued;

        public ObservableCollection<Salesman> Salesmans { get; }

        public ObservableCollection<Warehouse> Warehouses { get; }

        public ObservableCollection<AlternativeSalesPrice> AlternativeSalesPrices { get; }

        public AlternativeSalesPrice SelectedAlternativeSalesPrice
        {
            get { return _selectedAlternativeSalesPrice; }
            set
            {
                SetProperty(ref _selectedAlternativeSalesPrice, value, () => SelectedAlternativeSalesPrice);
                if (_selectedAlternativeSalesPrice == null) return;
                EditLineSalesPrice = _selectedAlternativeSalesPrice.SalesPrice;
                EditLineDiscount = 0;
            }
        }

        #region Line Properties
        public int EditLineUnits
        {
            get { return _editLineUnits; }
            set { SetProperty(ref _editLineUnits, value, () => EditLineUnits); }
        }

        public int? EditLineSecondaryUnits
        {
            get { return _editLineSecondaryUnits; }
            set { SetProperty(ref _editLineSecondaryUnits, value, () => EditLineSecondaryUnits); }
        }

        public int EditLinePieces
        {
            get { return _editLinePieces; }
            set { SetProperty(ref _editLinePieces, value, () => EditLinePieces); }
        }

        public decimal EditLineDiscount
        {
            get { return _editLineDiscount; }
            set { SetProperty(ref _editLineDiscount, value, () => EditLineDiscount); }
        }

        public decimal EditLineSalesPrice
        {
            get { return _editLineSalesPrice; }
            set { SetProperty(ref _editLineSalesPrice, value, () => EditLineSalesPrice); }
        }

        public Salesman EditLineSalesman
        {
            get { return _editLineSalesman; }
            set { SetProperty(ref _editLineSalesman, value, () => EditLineSalesman); }
        }
        #endregion

        public ICommand EditConfirmCommand
        {
            get
            {
                return _editConfirmCommand ?? (_editConfirmCommand = new RelayCommand(() =>
                {
                    if (!IsEditedQuantityValid() || !ArePriceAndDiscountFieldsValid()) return;

                    var isLineCombinable = false;

                    var salesTransactionLinesList = _parentVM.DisplayedSalesTransactionLines.ToList();
                    var editingLineIndex = salesTransactionLinesList.IndexOf(_selectedLine);
                    // Run a check to see if this line can be combined with another line of the same in transaction
                    foreach (var line in salesTransactionLinesList)
                    {
                        if (!CompareLineToEditProperties(line.Model)) continue;
                        if (editingLineIndex == salesTransactionLinesList.IndexOf(line)) continue;
                        line.Quantity += _selectedLine.Quantity;
                        _parentVM.DisplayedSalesTransactionLines.Remove(_selectedLine);
                        isLineCombinable = true;
                        break;
                    }

                    if (!isLineCombinable) AssignEditPropertiesToSelectedLine();

                    _parentVM.UpdateTransactionTotal();
                    CloseWindow();
                }));
            }
        }

        #region Helper Methods
        private void UpdateSalesmans()
        {
            Salesmans.Clear();   
            using (var context = UtilityMethods.createContext())
            {
                var salesmansFromDatabase = context.Salesmans.Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name);

                foreach (var salesman in salesmansFromDatabase)
                    Salesmans.Add(salesman);
            }
        }

        private void UpdateWarehouses()
        {
            Warehouses.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var warehousesFromDatabase = context.Warehouses.OrderBy(warehouse => warehouse.Name);

                foreach (var warehouse in warehousesFromDatabase)
                    Warehouses.Add(warehouse);
            }
        }

        private void UpdateAlternativeSalesPrices()
        {
            AlternativeSalesPrices.Clear();

            using (var context = UtilityMethods.createContext())
            {
                var alternativeSalesPrices =
                    context.AlternativeSalesPrices.Where(
                        altSalesPrice => altSalesPrice.ItemID.Equals(_selectedLine.Item.ItemID));
                foreach (var altSalesPrice in alternativeSalesPrices)
                    AlternativeSalesPrices.Add(altSalesPrice);
            }
        }

        private void SetEditProperties()
        {
            EditLineUnits = _selectedLine.Units;
            EditLineSecondaryUnits = _selectedLine.SecondaryUnits;
            EditLinePieces = _selectedLine.Pieces;
            EditLineSalesPrice = _selectedLine.SalesPrice;
            EditLineDiscount = _selectedLine.Discount;
            EditLineSalesman = _selectedLine.Salesman;
        }

        private bool IsEditedQuantityValid()
        {
            var availableQuantity = _parentVM.GetAvailableQuantity(_selectedLine.Item, _selectedLine.Warehouse);

            var oldSecondaryUnitsAsPieces = (int) (_selectedLine.SecondaryUnits == null
                ? 0
                : _selectedLine.SecondaryUnits*_selectedLine.Item.PiecesPerSecondaryUnit);
            var oldQuantity = _selectedLine.Units*_selectedLine.Item.PiecesPerUnit + oldSecondaryUnitsAsPieces +
                              _selectedLine.Pieces;
            var newSecondaryUnitsAsPieces = (int) (_editLineSecondaryUnits == null
                ? 0
                : _editLineSecondaryUnits*_selectedLine.Item.PiecesPerSecondaryUnit);
            var newQuantity = _editLineUnits*_selectedLine.Item.PiecesPerUnit + newSecondaryUnitsAsPieces +
                              _editLinePieces;
            var quantityDifference = newQuantity - oldQuantity;

            if (availableQuantity - quantityDifference >= 0) return true;

            if (_editLineSecondaryUnits == null)
                MessageBox.Show(
                    $"{_selectedLine.Item.Name} has only {availableQuantity/_selectedLine.Item.PiecesPerUnit + _selectedLine.Units} units, " +
                    $"{availableQuantity%_selectedLine.Item.PiecesPerUnit + _selectedLine.Pieces} pieces available.",
                    "Insufficient Stock", MessageBoxButton.OK);
            else
                MessageBox.Show(
                    $"{_selectedLine.Item.Name} has only {availableQuantity/_selectedLine.Item.PiecesPerUnit + _selectedLine.Units} units, " +
                    $"{availableQuantity%_selectedLine.Item.PiecesPerUnit/_selectedLine.Item.PiecesPerSecondaryUnit + _selectedLine.SecondaryUnits} secondary units, " +
                    $"{availableQuantity%_selectedLine.Item.PiecesPerSecondaryUnit%_selectedLine.Item.PiecesPerUnit + _selectedLine.Pieces} pieces available.",
                    "Insufficient Stock", MessageBoxButton.OK);

            return false;
        }

        private bool ArePriceAndDiscountFieldsValid()
        {
            if (_editLineSalesPrice >= 0 && _editLineDiscount >= 0 &&
                _editLineDiscount <= _editLineSalesPrice)
                return true;
            MessageBox.Show("Please check that the price and discount fields are valid.", "Invalid Field(s)", MessageBoxButton.OK);
            return false;
        }

        private void AssignEditPropertiesToSelectedLine()
        {
            var secondaryUnitsAsPieces = (int) (_editLineSecondaryUnits == null ? 0
                : _editLineSecondaryUnits *_selectedLine.Item.PiecesPerSecondaryUnit);
            _selectedLine.Quantity = _editLineUnits * _selectedLine.Item.PiecesPerUnit + secondaryUnitsAsPieces + _editLinePieces;
            _selectedLine.Discount = _editLineDiscount;
            _selectedLine.SalesPrice = _editLineSalesPrice;
            _selectedLine.Salesman = _editLineSalesman;
        }

        private bool CompareLineToEditProperties(SalesTransactionLine line)
        {
            return line.Item.ItemID.Equals(_selectedLine.Item.ItemID) && line.Warehouse.ID.Equals(_selectedLine.Warehouse.ID)
                && line.SalesPrice.Equals(_editLineSalesPrice/_selectedLine.Item.PiecesPerUnit)
                && line.Discount.Equals(_editLineDiscount/_selectedLine.Item.PiecesPerUnit);
        }
        #endregion
    }
}
