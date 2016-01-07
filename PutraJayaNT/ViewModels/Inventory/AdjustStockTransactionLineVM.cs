using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.StockCorrection;

namespace PutraJayaNT.ViewModels.Inventory
{
    class AdjustStockTransactionLineVM : ViewModelBase<AdjustStockTransactionLine>
    {
        int _units;
        int _pieces;

        public AdjustStockTransaction DecreaseStockTransaction
        {
            get { return Model.AdjustStockTransaction; }
        }

        public Item Item
        {
            get { return Model.Item; }
            set
            {
                Model.Item = value;
                OnPropertyChanged("Warehouse");
            }
        }

        public Warehouse Warehouse
        {
            get { return Model.Warehouse; }
            set
            {
                Model.Warehouse = value;
                OnPropertyChanged("Warehouse");
            }
        }

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }

        public int Units
        {
            get
            {
                _units = Model.Quantity / Model.Item.PiecesPerUnit;
                return _units;
            }
            set
            {
                Model.Quantity = (value * Model.Item.PiecesPerUnit) + _pieces;
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }

        public int Pieces
        {
            get
            {
                _pieces = Model.Quantity % Model.Item.PiecesPerUnit;
                return _pieces;
            }
            set
            {
                Model.Quantity = (_units * Model.Item.PiecesPerUnit) + value;
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }

        public string Unit
        {
            get { return Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit; }
        }
    }
}
