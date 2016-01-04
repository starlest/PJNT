using MVVMFramework;
using PutraJayaNT.Models.StockCorrection;
using PutraJayaNT.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PutraJayaNT.ViewModels.Inventory
{
    class DecreaseStockTransactionVM : ViewModelBase<DecreaseStockTransaction>
    {
        ObservableCollection<WarehouseVM> _warehouses;
        ObservableCollection<ItemVM> _products;
        ObservableCollection<DecreaseStockTransactionLineVM> _lines;

        string _newTransactionID;

        WarehouseVM _newEntryWarehouse;
        ItemVM _newEntryProduct;
        string _newEntryUnitName;
        int? _newEntryPiecesPerUnit;
        int? _newEntryUnits;
        int? _newEntryPieces;
        ICommand _newEntryCommand;

        public DecreaseStockTransactionVM()
        {
            _warehouses = new ObservableCollection<WarehouseVM>();
            _products = new ObservableCollection<ItemVM>();
            _lines = new ObservableCollection<DecreaseStockTransactionLineVM>();

            UpdateWarehouses();
            SetTransactionID();
        }

        public ObservableCollection<WarehouseVM> Warehouses
        {
            get { return _warehouses; }
        }

        public ObservableCollection<ItemVM> Products
        {
            get { return _products; }
        }

        #region New Transaction Properties 
        public string NewTransactionID
        {
            get { return _newTransactionID; }
            set { SetProperty(ref _newTransactionID, value, "NewTransactionID"); }
        }
        #endregion

        #region New Entry Properties
        public WarehouseVM NewEntryWarehouse
        {
            get { return _newEntryWarehouse; }
            set
            {
                SetProperty(ref _newEntryWarehouse, value, "NewEntryWarehouse");

                if (_newEntryWarehouse == null) return;

                UpdateProducts();
            }
        }

        public ItemVM NewEntryProduct
        {
            get { return _newEntryProduct; }
            set
            {
                SetProperty(ref _newEntryProduct, value, "NewEntryProduct");

                if (_newEntryProduct == null) return;

                NewEntryUnitName = _newEntryProduct.UnitName;
                NewEntryPiecesPerUnit = _newEntryProduct.PiecesPerUnit;
            }
        }

        public string NewEntryUnitName
        {
            get { return _newEntryUnitName; }
            set { SetProperty(ref _newEntryUnitName, value, "NewEntryUnitName"); }
        }

        public int? NewEntryPiecesPerUnit
        {
            get { return _newEntryPiecesPerUnit; }
            set { SetProperty(ref _newEntryPiecesPerUnit, value, "NewEntryPiecesPerUnit"); }
        }

        public int? NewEntryUnits
        {
            get { return _newEntryUnits; }
            set { SetProperty(ref _newEntryUnits, value, "NewEntryUnits"); }
        }

        public int? NewEntryPieces
        {
            get { return _newEntryPieces; }
            set { SetProperty(ref _newEntryPieces, value, "NewEntryPieces"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        #region Helper Methods
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

        private void UpdateProducts()
        {
            _products.Clear();

            using (var context = new ERPContext())
            {
                var products = context.Stocks
                    .Include("Item")
                    .Where(e => e.WarehouseID.Equals(_newEntryWarehouse.ID))
                    .ToList();

                foreach (var product in products)
                    _products.Add(new ItemVM { Model = product.Item });
            }
        }

        private void SetTransactionID()
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            _newTransactionID = "DS" + ((long)((year - 2000) * 100 + month) * 1000000).ToString();

            string lastTransactionID = null;
            using (var context = new ERPContext())
            {
                var IDs = (from DecreaseStockTransaction in context.DecreaseStockTransactions
                           where DecreaseStockTransaction.DecreaseStrockTransactionID.CompareTo(_newTransactionID) >= 0
                           orderby DecreaseStockTransaction.DecreaseStrockTransactionID descending
                           select DecreaseStockTransaction.DecreaseStrockTransactionID);
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null) _newTransactionID = "DS" + (Convert.ToInt64(lastTransactionID.Substring(1)) + 1).ToString();

            OnPropertyChanged("NewTransactionID");
        }

        private void ResetEntryFields()
        {
            NewEntryWarehouse = null;
            NewEntryProduct = null;
            NewEntryUnitName = null;
            NewEntryPiecesPerUnit = null;
            NewEntryUnits = null;
            NewEntryPieces = null;
        }

        private void ResetTransaction()
        {
            ResetEntryFields();
            _lines.Clear();
            SetTransactionID();
        }
        #endregion
    }
}
