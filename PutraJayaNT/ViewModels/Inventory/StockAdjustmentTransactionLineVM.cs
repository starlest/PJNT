namespace ECRP.ViewModels.Inventory
{
    using Models.Inventory;
    using Models.StockCorrection;
    using MVVMFramework;

    internal class StockAdjustmentTransactionLineVM : ViewModelBase<StockAdjustmentTransactionLine>
    {
        public StockAdjustmentTransaction StockAdjustmentTransaction => Model.StockAdjustmentTransaction;

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
            get { return Model.Quantity / Model.Item.PiecesPerUnit; }
            set
            {
                var pieces = Model.Quantity % Model.Item.PiecesPerUnit;
                Model.Quantity = value * Model.Item.PiecesPerUnit + pieces;
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }

        public int Pieces
        {
            get { return Model.Quantity % Model.Item.PiecesPerUnit; }
            set
            {
                var units = Model.Quantity / Model.Item.PiecesPerUnit;
                Model.Quantity = units * Model.Item.PiecesPerUnit + value;
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
            }
        }

        public string Unit => Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit;
    }
}
