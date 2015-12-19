using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;

namespace PutraJayaNT.ViewModels.Customers
{
    class SalesReturnTransactionLineVM : ViewModelBase<SalesReturnTransactionLine>
    {
        int _units;
        int _pieces;

        public Item Item
        {
            get { return Model.Item; }
            set
            {
                Model.Item = value;
                OnPropertyChanged("Item");
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

        public virtual SalesReturnTransaction SalesReturnTransaction
        {
            get { return Model.SalesReturnTransaction; }
            set
            {
                Model.SalesReturnTransaction = value;
                OnPropertyChanged("SalesReturnTransaction");
            }
        }

        public string Unit
        {
            get { return Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit; }
        }

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
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
                Model.Quantity = value + (_units * Model.Item.PiecesPerUnit);

                SetProperty(ref _pieces, value, "Pieces");
                OnPropertyChanged("Units");
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
                Model.Quantity = _pieces + (value * Model.Item.PiecesPerUnit);

                SetProperty(ref _units, value, "Units");
                OnPropertyChanged("Pieces");
            }
        }

        public decimal SalesPrice
        {
            get { return Model.SalesPrice * Item.PiecesPerUnit; }
            set
            {
                Model.SalesPrice = value / Item.PiecesPerUnit;
                OnPropertyChanged("SalesPrice");
            }
        }

        public decimal CostOfGoodsSold
        {
            get { return Model.CostOfGoodsSold; }
            set
            {
                Model.CostOfGoodsSold = value;
                OnPropertyChanged("PurchasePrice");
            }
        }
    }
}
