﻿namespace PutraJayaNT.ViewModels.Purchase
{
    using System.Linq;
    using System.Windows;
    using Models.Inventory;
    using Models.Purchase;
    using MVVMFramework;
    using Utilities;

    public class PurchaseTransactionLineVM : ViewModelBase<PurchaseTransactionLine>
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

        public string PurchaseTransactionID => Model.PurchaseTransaction.PurchaseID;

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                Total = value * Model.PurchasePrice;
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }

        public string Unit
        {
            get { return Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit; }
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

        public decimal Discount
        {
            get { return Model.Discount * Model.Item.PiecesPerUnit; }
            set
            {
                Model.Discount = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("Discount");
                Total = Model.Quantity * (Model.PurchasePrice - Model.Discount);
            }
        }

        public decimal PurchasePrice
        {
            get { return Model.PurchasePrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.PurchasePrice = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("PurchasePrice");
                Total = Model.Quantity * (Model.PurchasePrice - Model.Discount);
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
            set
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
            using (var context = new ERPContext())
            {
                var itemStock = context.Stocks.FirstOrDefault(stock => stock.ItemID.Equals(Model.Item.ItemID) && stock.WarehouseID.Equals(Model.Warehouse.ID));
                return itemStock?.Pieces ?? 0;
            }
        }
       
        public decimal GetNetDiscount()
        {
            var lineDiscount = Model.Discount / Model.Item.PiecesPerUnit;
            var lineSalesPrice = Model.PurchasePrice / Model.Item.PiecesPerUnit;
            if (lineSalesPrice - lineDiscount == 0) return 0;
            var fractionOfTransaction = Model.Quantity * (lineSalesPrice - lineDiscount) / Model.PurchaseTransaction.GrossTotal;
            var fractionOfTransactionDiscount = fractionOfTransaction * Model.PurchaseTransaction.Discount / Model.Quantity;
            var discount = (lineDiscount + fractionOfTransactionDiscount) * Model.Item.PiecesPerUnit;
            return discount;
        }

        public PurchaseTransactionLineVM Clone()
        {
            var newLine = new PurchaseTransactionLine
            {
                Item = Model.Item,
                Warehouse = Model.Warehouse,
                PurchasePrice = Model.PurchasePrice * Model.Item.PiecesPerUnit,
                Discount = Model.Discount * Model.Item.PiecesPerUnit,
                SoldOrReturned = Model.SoldOrReturned,
                Total = Model.Total
            };

            return new PurchaseTransactionLineVM { Model = newLine };
        }
    }
}
