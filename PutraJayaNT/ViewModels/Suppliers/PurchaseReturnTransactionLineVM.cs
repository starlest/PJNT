using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Purchase;
using PutraJayaNT.Utilities;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.ViewModels.Suppliers
{
    class PurchaseReturnTransactionLineVM : ViewModelBase<PurchaseReturnTransactionLine>
    {
        int _pieces;
        int _units;

        decimal _discount;

        public PurchaseReturnTransaction PurchaseReturnTransaction
        {
            get { return Model.PurchaseReturnTransaction; }
            set
            {
                Model.PurchaseReturnTransaction = value;
                OnPropertyChanged("PurchaseReturnTransaction");
            }
        }

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

        public Warehouse ReturnWarehouse
        {
            get { return Model.ReturnWarehouse; }
            set
            {
                Model.ReturnWarehouse = value;
                OnPropertyChanged("ReturnWarehouse");
            }
        }


        public int AvailableReturnQuantity
        {
            get; set;
        }

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Units");
                OnPropertyChanged("Total");
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

                var returnQuantity = (_units * Model.Item.PiecesPerUnit) + value;
                var availableReturnQuantity = AvailableReturnQuantity; // FIX THIS
                var availablePieces = GetStock();

                using (var context = new ERPContext())
                {
                    var returnedItems = context.PurchaseReturnTransactionLines
                    .Where(e => e.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID
                    .Equals(Model.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID) && 
                    e.ItemID.Equals(Model.ItemID));

                    if (returnedItems.Count() != 0)
                    {
                        foreach (var item in returnedItems)
                        {
                            availableReturnQuantity -= item.Quantity;
                        }
                    }
                }

                if (returnQuantity > availableReturnQuantity
                || returnQuantity > availablePieces
                || returnQuantity <= 0)
                {
                    MessageBox.Show(string.Format("The available return quantity is {0} units {1} pieces",
                        availableReturnQuantity / Model.Item.PiecesPerUnit, availableReturnQuantity % Model.Item.PiecesPerUnit), 
                        "Invalid Quantity Input", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _pieces, value, "Pieces");

                Model.Quantity = _pieces + (_units * Model.Item.PiecesPerUnit);
                OnPropertyChanged("Total");
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
                var returnQuantity = (value * Model.Item.PiecesPerUnit) + _pieces;
                var availableReturnQuantity = AvailableReturnQuantity; 
                var availablePieces = GetStock();

                using (var context = new ERPContext())
                {
                    var returnedItems = context.PurchaseReturnTransactionLines
                    .Where(e => e.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID
                    .Equals(Model.PurchaseReturnTransaction.PurchaseTransaction.PurchaseID) &&
                    e.ItemID.Equals(Model.ItemID));

                    if (returnedItems.Count() != 0)
                    {
                        foreach (var item in returnedItems)
                        {
                            availableReturnQuantity -= item.Quantity;
                        }
                    }
                }

                if (returnQuantity > availableReturnQuantity
                || returnQuantity > availablePieces
                || returnQuantity <= 0)
                {
                    MessageBox.Show(string.Format("The available return quantity is {0} units {1} pieces",
                        availableReturnQuantity / Model.Item.PiecesPerUnit, availableReturnQuantity % Model.Item.PiecesPerUnit),
                        "Invalid Quantity Input", MessageBoxButton.OK);
                    return;
                }

                SetProperty(ref _units, value, "Units");

                Model.Quantity = _pieces + (_units * Model.Item.PiecesPerUnit);
                OnPropertyChanged("Total");
            }
        }

        public decimal PurchasePrice
        {
            get
            {
                return Model.PurchasePrice * Model.Item.PiecesPerUnit;
            }
            set
            {
                Model.PurchasePrice = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("PurchasePrice");
            }
        }

        public decimal Discount
        {
            get
            {
                return Model.Discount * Model.Item.PiecesPerUnit;
            }
            set
            {
                Model.Discount = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("Discount");
            }
        }

        public decimal NetDiscount
        {
            get
            {
                return Model.NetDiscount * Model.Item.PiecesPerUnit;
            }
            set
            {
                Model.NetDiscount = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("Discount");
            }
        }

        public decimal Total
        {
            get
            {
                Model.Total = Model.Quantity * (Model.PurchasePrice - Model.NetDiscount);
                return Model.Total;
            }
            set
            {
                Model.Total = value;
                OnPropertyChanged("Total");
            }
        }

        public void UpdateTotal()
        {
            Model.Total = Model.Quantity * (Model.PurchasePrice - Model.NetDiscount);
            OnPropertyChanged("Total");
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

