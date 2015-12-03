using MVVMFramework;
using PutraJayaNT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PutraJayaNT.ViewModels
{
    class SalesReturnTransactionLineVM : ViewModelBase<SalesReturnTransactionLine>
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

        public virtual SalesReturnTransaction SalesReturnTransaction
        {
            get { return Model.SalesReturnTransaction; }
            set
            {
                Model.SalesReturnTransaction = value;
                OnPropertyChanged("SalesReturnTransaction");
            }
        }

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        public decimal SalesPrice
        {
            get { return Model.SalesPrice; }
            set
            {
                Model.SalesPrice = value;
                OnPropertyChanged("SalesPrice");
            }
        }

        public decimal CostOfGoodsSold
        {
            get { return Model.CostOfGoodsSold; }
            set
            {
                Model.CostOfGoodsSold = value;
                OnPropertyChanged("PurchasePrice");
            }
        }
    }
}
