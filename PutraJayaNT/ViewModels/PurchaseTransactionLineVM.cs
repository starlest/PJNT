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
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Total");
            }
        }

        public int Units
        {
            get { return Model.Quantity / Model.Item.PiecesPerUnit; }
        }

        public int Pieces
        {
            get { return Model.Quantity % Model.Item.PiecesPerUnit; }
        }

        public decimal PurchasePrice
        {
            get { return Model.PurchasePrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.PurchasePrice = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("PurchasePricePerUnit");
                OnPropertyChanged("Total");
            }
        }

        public decimal PurchasePricePerUnit
        {
            get { return Model.PurchasePrice * Model.Item.PiecesPerUnit; }
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

        public string PurchaseID
        {
            get { return Model.PurchaseID; }
        }
    }
}
