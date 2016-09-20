namespace ECRP.ViewModels.Purchase
{
    using Models.Inventory;
    using Models.Purchase;
    using MVVMFramework;

    internal class PurchaseReturnTransactionLineVM : ViewModelBase<PurchaseReturnTransactionLine>
    {
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

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Units");
            }
        }

        public int Pieces => Model.Quantity % Model.Item.PiecesPerUnit;

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

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

        public decimal ReturnPrice
        {
            get
            {
                return Model.ReturnPrice * Model.Item.PiecesPerUnit;
            }
            set
            {
                Model.ReturnPrice = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("ReturnPrice");
            }
        }

        public decimal Discount
        {
            get { return Model.Discount * Model.Item.PiecesPerUnit; }
            set
            {
                Model.Discount = value / Model.Item.PiecesPerUnit;
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

        public void UpdateTotal()
        {
            OnPropertyChanged("Total");
        }
    }
}

