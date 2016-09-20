namespace ECRP.ViewModels.Inventory
{
    using Models.Inventory;
    using Models.StockCorrection;
    using MVVMFramework;

    public class StockMovementTransactionLineVM : ViewModelBase<StockMovementTransactionLine>
    {
        public StockMovementTransaction StockMovementTransaction => Model.StockMovementTransaction;

        public Item Item
        {
            get { return Model.Item; }
            set
            {
                Model.Item = value;
                OnPropertyChanged("Item");
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
