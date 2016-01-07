using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Inventory
{
    class MoveStockVM : ViewModelBase
    {
        ObservableCollection<ItemVM> _products;
        ObservableCollection<WarehouseVM> _warehouses;

        ItemVM _selectedProduct;
        WarehouseVM _fromWarehouse;
        WarehouseVM _toWarehouse;
        string _remainingStock;
        int? _units;
        int? _pieces;
        ICommand _submitCommand;

        public MoveStockVM()
        {
            _products = new ObservableCollection<ItemVM>();
            _warehouses = new ObservableCollection<WarehouseVM>();

            UpdateProducts();
            UpdateWarehouses();
        }

        #region Collections
        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
        }

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }
        #endregion

        public ItemVM SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                if (value == null) return;

                UpdateProducts();

                SetProperty(ref _selectedProduct, value, "SelectedProduct");

                if (_fromWarehouse != null) SetRemainingStock();
            }
        }

        public WarehouseVM FromWarehouse
        {
            get { return _fromWarehouse; }
            set
            {
                SetProperty(ref _fromWarehouse, value, "FromWarehouse");

                if (_fromWarehouse == null) return;

                if (_selectedProduct != null) SetRemainingStock();
            }
        }

        public WarehouseVM ToWarehouse
        {
            get { return _toWarehouse; }
            set { SetProperty(ref _toWarehouse, value, "ToWarehouse"); }
        }

        public string RemainingStock
        {
            get { return _remainingStock; }
            set { SetProperty(ref _remainingStock, value, "RemainingStock"); }
        }

        public int? Units
        {
            get { return _units; }
            set { SetProperty(ref _units, value, "Units"); }
        }

        public int? Pieces
        {
            get { return _pieces; }
            set { SetProperty(ref _pieces, value, "Pieces"); }
        }

        public ICommand SubmitCommand
        {
            get
            {
                return _submitCommand ?? (_submitCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm movement?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

                    if (_selectedProduct == null || _fromWarehouse == null || _toWarehouse == null)
                    {
                        MessageBox.Show("Please enter the missing field(s).", "Missing Field(s)", MessageBoxButton.OK);
                        return;
                    }

                    if (_fromWarehouse.Equals(_toWarehouse))
                    {
                        MessageBox.Show("Cannot move between the same warehouses.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    var units = _units == null ? 0 : (int)_units;
                    var pieces = _pieces == null ? 0 : (int)_pieces;
                    var quantity = (units * _selectedProduct.PiecesPerUnit) + pieces;

                    if (GetRemainingStock(_selectedProduct, _fromWarehouse) == 0 || quantity > GetRemainingStock(_selectedProduct, _fromWarehouse))
                    {
                        MessageBox.Show("Not enough stock to be moved.", "Invalid Command", MessageBoxButton.OK);
                        return;
                    }

                    // Verification
                    if (!UtilityMethods.GetVerification()) return;

                    using (var context = new ERPContext())
                    {
                        var fromStock = context.Stocks.Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(_fromWarehouse.ID)).FirstOrDefault();
                        var toStock = context.Stocks.Where(e => e.ItemID.Equals(_selectedProduct.ID) && e.WarehouseID.Equals(_toWarehouse.ID)).FirstOrDefault();

                        fromStock.Pieces -= quantity;
                        if (toStock != null) toStock.Pieces += quantity;
                        else
                        {
                            var newStock = new Stock
                            {
                                Warehouse = context.Warehouses.Where(e => e.ID.Equals(_toWarehouse.ID)).FirstOrDefault(),
                                Item = context.Inventory.Where(e => e.ItemID.Equals(_selectedProduct.ID)).FirstOrDefault(),
                                Pieces = quantity
                            };
                            context.Stocks.Add(newStock);
                        }

                        context.SaveChanges();
                    }
                    MessageBox.Show(string.Format("Sucessfully moved {0} from {1} to {2}!", _selectedProduct.Name, _fromWarehouse.Name, _toWarehouse.Name), "Sucess", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }

        #region Helper Methods
        private void UpdateProducts()
        {
            _products.Clear();

            using (var context = new ERPContext())
            {
                var products = context.Inventory.Include("Stocks").ToList();
                foreach (var product in products)
                    _products.Add(new ItemVM { Model = product });
            }
        }

        private void UpdateWarehouses()
        {
           _warehouses.Clear();

            using (var context = new ERPContext())
            {
                var warehouses = context.Warehouses.ToList();
                foreach (var warehouse in warehouses)
                    _warehouses.Add(new WarehouseVM { Model = warehouse });
            }
        }

        private void ResetTransaction()
        {
            SelectedProduct = null;
            FromWarehouse = null;
            ToWarehouse = null;
            RemainingStock = null;
            Units = null;
            Pieces = null;
        }

        private int GetRemainingStock(ItemVM item, WarehouseVM warehouse)
        {
            using (var context = new ERPContext())
            {
                var stock = context.Stocks.Where(e => e.ItemID.Equals(item.ID) && e.WarehouseID.Equals(warehouse.ID)).FirstOrDefault();

                if (stock == null) return 0;
                else return stock.Pieces;
            }
        }

        private void SetRemainingStock()
        {
            var remainingStock = GetRemainingStock(_selectedProduct, _fromWarehouse);
            RemainingStock = (remainingStock / _selectedProduct.PiecesPerUnit) + "/" + (remainingStock % _selectedProduct.PiecesPerUnit) + " " + _selectedProduct.UnitName;
        }
        #endregion
    }
}
