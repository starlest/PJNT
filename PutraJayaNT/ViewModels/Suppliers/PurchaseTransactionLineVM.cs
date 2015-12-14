using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using System.Windows;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PurchaseTransactionLineVM : ViewModelBase<PurchaseTransactionLine>
    {
        int _pieces;
        int _units;

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

        public PurchaseTransaction PurchaseTransaction
        {
            get { return Model.PurchaseTransaction; }
            set
            {
                Model.PurchaseTransaction = value;
                OnPropertyChanged("PurchaseTransaction");
            }
        }

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                Total = value * PurchasePrice;
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
                if (value < 0)
                {
                    MessageBox.Show("Please enter a valid value", "Invalid Quantity", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _units, value, "Units");

                Model.Quantity = _pieces + (_units * Model.Item.PiecesPerUnit);
                Total = Model.Quantity * Model.PurchasePrice;
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
                if (value >= Model.Item.PiecesPerUnit || value < 0)
                {
                    MessageBox.Show(string.Format("Please enter a value between {0} - {1}", 0, Model.Item.PiecesPerUnit - 1), "Invalid Quantity", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _pieces, value, "Pieces");

                Model.Quantity = _pieces + (_units * Model.Item.PiecesPerUnit);
                Total = Model.Quantity * Model.PurchasePrice;
            }
        }

        public decimal PurchasePrice
        {
            get { return Model.PurchasePrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.PurchasePrice = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("PurchasePricePerUnit");
                OnPropertyChanged("Total");
            }
        }

        public decimal PurchasePricePerUnit
        {
            get { return Model.PurchasePrice * Model.Item.PiecesPerUnit; }
        }


        public decimal Total
        {
            get
            {
                return Model.Total;
            }
            private set
            {
                Model.Total = value;
                OnPropertyChanged("Total");
            }
        }

        public int SoldOrReturned
        {
            get { return Model.SoldOrReturned; }
            set
            {
                Model.SoldOrReturned = value;
                OnPropertyChanged("SoldOrReturned");
            }
        }
            
        public int GetStock()
        {
            int s = 0;
            foreach (var stock in Model.Item.Stocks)
            {
                if (stock.Warehouse.Equals(Model.Warehouse))
                {
                    s = stock.Pieces;
                    break;
                }
            }
            return s;
        }
    }
}
