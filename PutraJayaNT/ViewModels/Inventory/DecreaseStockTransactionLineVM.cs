using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.StockCorrection;

namespace PutraJayaNT.ViewModels.Inventory
{
    class DecreaseStockTransactionLineVM : ViewModelBase<DecreaseStockTransactionLine>
    {
        int _units;
        int _pieces;

        public DecreaseStockTransaction DecreaseStockTransaction
        {
            get { return Model.DecreaseStockTransaction; }
        }

        public Item Item
        {
            get { return Model.Item; }
        }

        public Warehouse Warehouse
        {
            get { return Model.Warehouse; }
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
                Model.Quantity = (_units * Model.Item.PiecesPerUnit) + _pieces;
                OnPropertyChanged("Quantity");
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
                Model.Quantity = (_units * Model.Item.PiecesPerUnit) + _pieces;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }
    }
}
