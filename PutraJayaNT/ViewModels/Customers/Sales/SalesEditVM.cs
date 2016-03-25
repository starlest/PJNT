namespace PutraJayaNT.ViewModels.Customers.Sales
{
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

        #region Edit Line Backing Fields
        private AlternativeSalesPrice _selectedAlternativeSalesPrice;
        private int _editLineUnits;
        private int _editLinePieces;
        private decimal _editLineDiscount;
        private decimal _editLineSalesPrice;
        private Salesman _editLineSalesman;
        private Warehouse _editLineWarehouse;
        private ICommand _editConfirmCommand;

        #endregion

        public SalesEditVM(SalesVM parentVM)
        {
            _parentVM = parentVM;
            _selectedLine = _parentVM.SelectedLine;

            Salesmans = new ObservableCollection<Salesman>();
            AlternativeSalesPrices = new ObservableCollection<AlternativeSalesPrice>();

            UpdateSalesmans();
            UpdateAlternativeSalesPrices();
            SetEditProperties();
        }

        public bool InvoiceNotIssued => _parentVM.InvoiceNotIssued;

        public ObservableCollection<Salesman> Salesmans { get; }

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
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        #region Helper Methods
        private void UpdateSalesmans()
        {
            Salesmans.Clear();   
            using (var context = new ERPContext())
            {
                var salesmansFromDatabase = context.Salesmans.Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name);

                foreach (var salesman in salesmansFromDatabase)
                    Salesmans.Add(salesman);
            }
        }

        private void UpdateAlternativeSalesPrices()
        {
            AlternativeSalesPrices.Clear();

            using (var context = new ERPContext())
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
            _editLineUnits = _selectedLine.Units;
            _editLinePieces = _selectedLine.Pieces;
            _editLineSalesPrice = _selectedLine.SalesPrice;
            _editLineDiscount = _selectedLine.Discount;
            _editLineSalesman = _selectedLine.Salesman;
            _editLineWarehouse = _selectedLine.Warehouse;
        }

        private bool IsEditedQuantityValid()
        {
            var availableQuantity = _parentVM.GetAvailableQuantity(_selectedLine.Item, _selectedLine.Warehouse);

            var oldQuantity = _selectedLine.Units * _selectedLine.Item.PiecesPerUnit + _selectedLine.Pieces;
            var newQuantity = _editLineUnits * _selectedLine.Item.PiecesPerUnit + _editLinePieces;
            var quantityDifference = newQuantity - oldQuantity;

            if (availableQuantity - quantityDifference >= 0) return true;
            MessageBox.Show(
                $"{_selectedLine.Item.Name} has only {availableQuantity / _selectedLine.Item.PiecesPerUnit + _selectedLine.Units} units, " +
                $"{availableQuantity % _selectedLine.Item.PiecesPerUnit + _selectedLine.Pieces} pieces available.",
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
            _selectedLine.Quantity = _editLineUnits * _selectedLine.Item.PiecesPerUnit + _editLinePieces;
            _selectedLine.Discount = _editLineDiscount;
            _selectedLine.SalesPrice = _editLineSalesPrice;
            _selectedLine.Salesman = _editLineSalesman;
            _selectedLine.Warehouse = _editLineWarehouse;
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
