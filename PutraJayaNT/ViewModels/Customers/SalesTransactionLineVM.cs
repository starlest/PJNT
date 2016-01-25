
namespace PutraJayaNT.ViewModels.Customers
{
    using MVVMFramework;
    using PutraJayaNT.Models.Inventory;
    using PutraJayaNT.Models.Sales;
    using PutraJayaNT.Models.Salesman;
    using PutraJayaNT.Utilities;
    using System;

    public class SalesTransactionLineVM : ViewModelBase<SalesTransactionLine>
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

        public SalesTransaction SalesTransaction
        {
            get { return Model.SalesTransaction; }
            set
            {
                Model.SalesTransaction = value;
                OnPropertyChanged("SalesTransaction");
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

        public Salesman Salesman
        {
            get { return Model.Salesman; }
            set
            {
                Model.Salesman = value;
                OnPropertyChanged("Salesman");
            }
        }

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Units");
            }
        }

        public string Unit
        {
            get { return Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit; }
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
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;

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
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;

                SetProperty(ref _units, value, "Units");
                OnPropertyChanged("Pieces");
            }
        }

        public decimal SalesPrice
        {
            get { return Model.SalesPrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.SalesPrice = value / Model.Item.PiecesPerUnit;
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
                OnPropertyChanged("SalesPrice");
            }
        }

        public decimal Discount
        {
            get { return Model.Discount * Model.Item.PiecesPerUnit; }
            set
            {
                Model.Discount = (value <= 0 ? 0 : (value/ Model.Item.PiecesPerUnit) );
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
                OnPropertyChanged("Discount");
            }
        }

        public decimal Total
        {
            get { return Model.Total; }
            set
            {
                Model.Total = value;
                OnPropertyChanged("Total");
            }
        }

        public decimal NetDiscount
        {
            get
            {
                return GetNetDiscount();
            }
        }

        public decimal NetTotal
        {
            get
            {
                return ((this.SalesPrice - this.NetDiscount) / this.Item.PiecesPerUnit) * this.Quantity;
            }
        }

        public int StockDeducted { get; set; }

        public int GetStock()
        {
            int s = 0;
            foreach (var stock in Model.Item.Stocks)
            {
                if (stock.WarehouseID.Equals(Model.Warehouse.ID))
                {
                    s = stock.Pieces;
                    break;
                }
            }

            s += StockDeducted;

            return s;
        }

        public decimal GetNetDiscount()
        {
            var lineDiscount = Model.Discount / Model.Item.PiecesPerUnit;
            var lineSalesPrice = Model.SalesPrice / Model.Item.PiecesPerUnit;
            if ((lineSalesPrice - lineDiscount) == 0) return 0;
            var fractionOfTransaction = (Model.Quantity * (lineSalesPrice - lineDiscount)) / Model.SalesTransaction.GrossTotal;
            var fractionOfTransactionDiscount = (fractionOfTransaction * Model.SalesTransaction.Discount) / Model.Quantity;
            var discount = (lineDiscount + fractionOfTransactionDiscount) * Model.Item.PiecesPerUnit;
            return discount;
        }

        public override bool Equals(object obj)
        {
            var line = obj as SalesTransactionLineVM;

            if (line == null) return false;
            else return this.Item.ItemID.Equals(line.Item.ItemID) &&
                    this.Warehouse.ID.Equals(line.Warehouse.ID) &&
                    this.SalesPrice.Equals(line.SalesPrice) &&
                    this.Discount.Equals(line.Discount);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public SalesTransactionLineVM Clone()
        {
            var cloneLine = new SalesTransactionLine
            {
                SalesTransaction = Model.SalesTransaction,
                Item = Model.Item,
                Warehouse = Model.Warehouse,
                SalesPrice = Model.SalesPrice,
                Discount = Model.Discount,
                Quantity = Model.Quantity,
                Total = Model.Total,
                Salesman = Model.Salesman
            };
            return new SalesTransactionLineVM { Model = cloneLine };
        }
    }
}
