namespace PutraJayaNT.ViewModels.Sales
{
    using MVVMFramework;
    using Models.Inventory;
    using Models.Sales;
    using Models.Salesman;

    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class SalesTransactionLineVM : ViewModelBase<SalesTransactionLine>
    {
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
                // TODO Remove this
                UpdateTotal();
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Units");
            }
        }

        public string Unit => Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit;

        public int Pieces => Model.Quantity % Model.Item.PiecesPerUnit;

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

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

        public decimal NetDiscount => GetNetDiscount();

        public decimal NetTotal => (SalesPrice - NetDiscount) / Item.PiecesPerUnit * Quantity;

        public int StockDeducted { get; set; }

        public override bool Equals(object obj)
        {
            var line = obj as SalesTransactionLineVM;
            if (line == null) return false;
            return Item.ItemID.Equals(line.Item.ItemID) &&
                   Warehouse.ID.Equals(line.Warehouse.ID) &&
                   SalesPrice.Equals(line.SalesPrice) &&
                   Discount.Equals(line.Discount);
        }

        #region Helper Methods
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
            return new SalesTransactionLineVM { Model = cloneLine, StockDeducted = StockDeducted };
        }

        public decimal GetNetDiscount()
        {
            var lineDiscount = Model.Discount / Model.Item.PiecesPerUnit;
            var lineSalesPrice = Model.SalesPrice / Model.Item.PiecesPerUnit;
            if (lineSalesPrice - lineDiscount == 0) return 0;
            var fractionOfTransaction = Model.Quantity * (lineSalesPrice - lineDiscount) / Model.SalesTransaction.GrossTotal;
            var fractionOfTransactionDiscount = fractionOfTransaction * Model.SalesTransaction.Discount / Model.Quantity;
            var discount = (lineDiscount + fractionOfTransactionDiscount) * Model.Item.PiecesPerUnit;
            return discount;
        }

        public void UpdateTotal()
        {
            Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
        }
        #endregion
    }
}
