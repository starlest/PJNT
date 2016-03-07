using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;

namespace PutraJayaNT.ViewModels.Customers
{
    public class AlternativeSalesPriceVM : ViewModelBase<AlternativeSalesPrice>
    {
        public Item Item
        {
            get { return Model.Item; }
            set { Model.Item = value; }
        } 

        public string Name => Model.Name;

        public decimal SalesPrice => Model.SalesPrice * Model.Item.PiecesPerUnit;
    }
}
