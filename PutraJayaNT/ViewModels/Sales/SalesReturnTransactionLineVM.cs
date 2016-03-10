namespace PutraJayaNT.ViewModels.Sales
{
    using MVVMFramework;
    using Models.Inventory;
    using Models.Sales;

    public class SalesReturnTransactionLineVM : ViewModelBase<SalesReturnTransactionLine>
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

        public string Unit => Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit;

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

        public int Pieces => Model.Quantity%Model.Item.PiecesPerUnit;

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

        public decimal SalesPrice
        {
            get { return Model.SalesPrice * Item.PiecesPerUnit; }
            set
            {
                Model.SalesPrice = value / Item.PiecesPerUnit;
                OnPropertyChanged("SalesPrice");
            }
        }

        public decimal ReturnPrice
        {
            get { return Model.ReturnPrice * Item.PiecesPerUnit; }
            set
            {
                Model.ReturnPrice = value / Item.PiecesPerUnit;
                OnPropertyChanged("ReturnPrice");
            }
        }

        public decimal Discount
        {
            get { return Model.Discount * Item.PiecesPerUnit; }
            set
            {
                Model.Discount = value / Item.PiecesPerUnit;
                OnPropertyChanged("Discount");
            }
        }

        public decimal Total
        {
            get
            {
                Model.Total = Quantity * Model.ReturnPrice;
                return Model.Total;
            }
            set
            {
                Model.Total = value;
                OnPropertyChanged("Total");
            }
        }

        public decimal CostOfGoodsSold
        {
            get { return Model.CostOfGoodsSold; }
            set
            {
                Model.CostOfGoodsSold = value;
                OnPropertyChanged("CostOfGoodsSold");
            }
        }

        public void UpdateTotal()
        {
            OnPropertyChanged("Total");
        }
    }
}
