﻿namespace PutraJayaNT.ViewModels.Reports
{
    using MVVMFramework;
    using Models;
    using Models.Inventory;
    using System.Linq;

    public class InventoryReportLineVM : ViewModelBase
    {
        private Item _item;
        private int _quantity;

        public InventoryReportLineVM(Item item)
        {
            _item = item;
        }

        public Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value, "Item"); }
        }

        public decimal SalesPrice => _item.SalesPrice * _item.PiecesPerUnit;

        public decimal PurchasePrice => _item.PurchasePrice * _item.PiecesPerUnit;

        public string Unit => _item.UnitName + "/" + _item.PiecesPerUnit;

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                SetProperty(ref _quantity, value, "Quantity");
                OnPropertyChanged("Units");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("InventoryValue");
            }
        }

        public int Units => _quantity / _item.PiecesPerUnit;

        public int Pieces => _quantity % _item.PiecesPerUnit;

        public decimal InventoryValue => _item.PurchasePrice * _quantity;

        public Supplier SelectedSupplier => _item.Suppliers.FirstOrDefault();
    }
}
