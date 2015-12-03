using MVVMFramework;
using PutraJayaNT.Models;

namespace PutraJayaNT.ViewModels
{
    class PurchaseTransactionLineVM : ViewModelBase<PurchaseTransactionLine>
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

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Total");
            }
        }

        public decimal PurchasePrice
        {
            get { return Model.PurchasePrice; }
            set
            {
                Model.PurchasePrice = value;
                OnPropertyChanged("PurchasePrice");
                OnPropertyChanged("Total");
            }
        }

        public decimal Total
        {
            get
            {
                Model.Total = Model.PurchasePrice * Model.Quantity;
                return Model.Total;
            }
            private set { Model.Total = value; }
        }
    }
}
