﻿using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using System.Collections.ObjectModel;

namespace PutraJayaNT.ViewModels
{
    class ItemVM : ViewModelBase<Item>
    {
        int _quantity;

        Supplier _selectedSupplier;

        public string ID
        {
            get { return Model.ItemID; }
            set
            {
                Model.ItemID = value;
                OnPropertyChanged("ID");
            }
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                Model.Name = value;
                OnPropertyChanged("Name");
            }
        }

        public Category Category
        {
            get { return Model.Category; }
            set
            {
                Model.Category = value;
                OnPropertyChanged("Category");
            }
        }

        public decimal PurchasePrice
        {
            get { return Model.PurchasePrice * Model.PiecesPerUnit; }
            set
            {
                Model.PurchasePrice = value / Model.PiecesPerUnit;
                OnPropertyChanged("PurchasePrice");
            }
        }

        public decimal SalesPrice
        {
            get { return Model.SalesPrice * Model.PiecesPerUnit; }
            set
            {
                Model.SalesPrice = value / Model.PiecesPerUnit;
                OnPropertyChanged("SalesPrice");
            }
        }

        public string UnitName
        {
            get { return Model.UnitName; }
            set
            {
                Model.UnitName = value;
                OnPropertyChanged("UnitName");
            }
        }

        public int PiecesPerUnit
        {
            get
            {
                return Model.PiecesPerUnit;
            }
            set
            {
                Model.PiecesPerUnit = value;
                OnPropertyChanged("PiecesPerUnit");
            }
        }

        public int Quantity
        {
            get
            {
                _quantity = 0;
                foreach (var stock in Model.Stocks)
                    _quantity += stock.Pieces;
                return _quantity;
            }
        }

        public int Units
        {
            get { return Quantity / Model.PiecesPerUnit; }
        }

        public int Pieces
        {
            get { return Quantity % Model.PiecesPerUnit; }
        }

        public ObservableCollection<Supplier> Suppliers
        {
            get { return Model.Suppliers; }
            set
            {
                Model.Suppliers = value;
                OnPropertyChanged("Suppliers");
            }
        }

        public ObservableCollection<Stock> Stocks
        {
            get { return Model.Stocks; }
            set
            {
                Model.Stocks = value;
                OnPropertyChanged("Stocks");
            }
        }

        public bool Active
        {
            get { return Model.Active; }
            set
            {
                Model.Active = value;
                OnPropertyChanged("Active");
            }
        }

        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set { SetProperty(ref _selectedSupplier, value, "SelectedSupplier"); }
        }

        public override bool Equals(object obj)
        {
            var item = obj as ItemVM;

            if (item == null) return false;
            else return this.ID.Equals(item.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}