namespace ECRP.ViewModels.Reports
{
    using System;
    using Models.Inventory;
    using MVVMFramework;
    using Utilities.ModelHelpers;

    public class StockCardLineVM : ViewModelBase
    {
        private DateTime _date;
        private string _documentation;
        private string _description;
        private string _customerSupplier;
        private int _amount;
        private int _balance;

        public Item Item { get; set; }

        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, () => Date); }
        }

        public string Documentation
        {
            get { return _documentation; }
            set { SetProperty(ref _documentation, value, () => Documentation); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value, () => Description); }
        }

        public string CustomerSupplier
        {
            get { return _customerSupplier; }
            set { SetProperty(ref _customerSupplier, value, () => CustomerSupplier); }
        }

        public int Amount
        {
            get { return _amount; }
            set { SetProperty(ref _amount, value, () => Amount); }
        }

        public string DefaultQuantityString => Item.PiecesPerSecondaryUnit == 0 ? "0/0" : "0/0/0";

        public string InQuantity
            => _amount > 0 ? InventoryHelper.ConvertItemQuantityTostring(Item, _amount) : DefaultQuantityString;

        public string OutQuantity
            => _amount < 0 ? InventoryHelper.ConvertItemQuantityTostring(Item, -_amount) : DefaultQuantityString;

        public int Units => _amount / Item.PiecesPerUnit;

        public int? SecondaryUnits => Item.PiecesPerSecondaryUnit == 0
            ? (int?) null
            : _amount % Item.PiecesPerUnit / Item.PiecesPerSecondaryUnit;

        public int Pieces => Item.PiecesPerSecondaryUnit == 0
            ? _amount % Item.PiecesPerUnit
            : _amount % Item.PiecesPerUnit % Item.PiecesPerSecondaryUnit;

        public int Balance
        {
            get { return _balance; }
            set { SetProperty(ref _balance, value, () => Balance); }
        }

        public string StringBalance => _balance / Item.PiecesPerUnit + "/" + _balance % Item.PiecesPerUnit;
    }
}