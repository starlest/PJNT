namespace ECRP.ViewModels.Sales
{
    using Models.Inventory;
    using Models.Sales;
    using MVVMFramework;
    using Utilities.ModelHelpers;

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

        public string Unit => Model.Item.PiecesPerSecondaryUnit == 0
            ? Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit
            : Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public string SecondaryUnit => Model.Item.PiecesPerSecondaryUnit == 0
            ? null
            : Model.Item.SecondaryUnitName + "/" + Model.Item.PiecesPerSecondaryUnit;

        public string UnitName => Model.Item.PiecesPerSecondaryUnit == 0
            ? Model.Item.UnitName
            : Model.Item.UnitName + "/" + Model.Item.SecondaryUnitName;

        public string QuantityPerUnit => InventoryHelper.GetItemQuantityPerUnit(Model.Item);

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

        public int? SecondaryUnits => Model.Item.PiecesPerSecondaryUnit == 0
            ? (int?) null
            : Model.Quantity % Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public int Pieces => Model.Item.PiecesPerSecondaryUnit == 0
            ? Model.Quantity % Item.PiecesPerUnit
            : Model.Quantity % Model.Item.PiecesPerUnit % Model.Item.PiecesPerSecondaryUnit;

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                UpdateTotal();
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Units");
                OnPropertyChanged("SecondaryUnits");
                OnPropertyChanged("Pieces");
            }
        }

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
            get { return Model.Total; }
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