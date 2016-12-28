namespace ECERP.ViewModels.Inventory
{
    using Models.Inventory;
    using Models.StockCorrection;
    using MVVMFramework;
    using Utilities.ModelHelpers;

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

        public string UnitName => Model.Item.PiecesPerSecondaryUnit == 0
            ? Model.Item.UnitName
            : Model.Item.UnitName + "/" + Model.Item.SecondaryUnitName;

        public string QuantityPerUnit => InventoryHelper.GetItemQuantityPerUnit(Model.Item);

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

        public int? SecondaryUnits => Model.Item.PiecesPerSecondaryUnit == 0 ? (int?)null :
            Model.Quantity % Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public int Pieces => Model.Item.PiecesPerSecondaryUnit == 0 ? Model.Quantity % Item.PiecesPerUnit :
            Model.Quantity % Model.Item.PiecesPerUnit % Model.Item.PiecesPerSecondaryUnit;
    }
}