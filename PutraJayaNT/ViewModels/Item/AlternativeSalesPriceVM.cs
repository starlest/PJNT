using MVVMFramework;
using PutraJayaNT.Models.Inventory;

namespace PutraJayaNT.ViewModels.Item
{
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
