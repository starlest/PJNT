namespace ECRP.ViewModels.Item
{
    using Models.Inventory;
    using MVVMFramework;

    public class AlternativeSalesPriceVM : ViewModelBase<AlternativeSalesPrice>
    {
        public Models.Inventory.Item Item
        {
            get { return Model.Item; }
            set { Model.Item = value; }
        } 

        public string Name => Model.Name;

        public decimal SalesPrice => Model.SalesPrice * Model.Item.PiecesPerUnit;
    }
}
