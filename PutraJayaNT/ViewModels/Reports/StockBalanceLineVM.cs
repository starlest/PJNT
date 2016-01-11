using MVVMFramework;
using PutraJayaNT.Models.Inventory;
using System;

namespace PutraJayaNT.ViewModels.Reports
{
    class StockBalanceLineVM : ViewModelBase
    {
        DateTime _date;
        string _documentation;
        string _description;
        string _customerSupplier;
        int _amount;
        int _balance;

        public Item Item { get; set; }

        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, "Date"); }
        }

        public string Documentation
        {
            get { return _documentation; }
            set { SetProperty(ref _documentation, value, "Documentation"); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value, "Description"); }
        }

        public string CustomerSupplier
        {
            get { return _customerSupplier; }
            set { SetProperty(ref _customerSupplier, value, "CustomerSupplier"); }
        }

        public int Amount
        {
            get { return _amount; }
            set { SetProperty(ref _amount, value, "Amount"); }
        }

        public string InQuantity
        {
            get { return _amount > 0 ? Units + "/" + Pieces : "0/0"; }
        }

        public string OutQuantity
        {
            get { return _amount < 0 ? -Units + "/" + -Pieces : "0/0"; }
        }

        public int Units
        {
            get { return _amount / Item.PiecesPerUnit; }
        }

        public int Pieces
        {
            get { return _amount % Item.PiecesPerUnit; }
        }

        public int Balance
        {
            get { return _balance; }
            set { SetProperty(ref _balance, value, "Balance"); }
        }

        public string StringBalance
        {
            get { return (_balance / Item.PiecesPerUnit) + "/" + (_balance % Item.PiecesPerUnit); }
        }
    }
}
