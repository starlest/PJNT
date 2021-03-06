﻿namespace ECERP.ViewModels.Purchase
{
    using Models.Inventory;
    using Models.Purchase;
    using MVVMFramework;
    using Utilities.ModelHelpers;

    public class PurchaseTransactionLineVM : ViewModelBase<PurchaseTransactionLine>
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

        public Warehouse Warehouse
        {
            get { return Model.Warehouse; }
            set
            {
                Model.Warehouse = value;
                OnPropertyChanged("Warehouse");
            }
        }

        public PurchaseTransaction PurchaseTransaction
        {
            get { return Model.PurchaseTransaction; }
            set
            {
                Model.PurchaseTransaction = value;
                OnPropertyChanged("PurchaseTransaction");
            }
        }

        public string PurchaseTransactionID => Model.PurchaseTransaction.PurchaseID;

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                UpdateTotal();
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Units");
                OnPropertyChanged("SecondaryUnits");
            }
        }

        public string Unit
            => Model.Item.PiecesPerSecondaryUnit == 0
                ? Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit
                : Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public string SecondaryUnit => Model.Item.PiecesPerSecondaryUnit == 0
            ? null
            : Model.Item.SecondaryUnitName + "/" + Model.Item.PiecesPerSecondaryUnit;

        public string UnitName => Model.Item.PiecesPerSecondaryUnit == 0
            ? Model.Item.UnitName
            : Model.Item.UnitName + "/" + Model.Item.SecondaryUnitName;

        public string QuantityPerUnit => InventoryHelper.GetItemQuantityPerUnit(Model.Item);

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

        public int? SecondaryUnits => Model.Item.PiecesPerSecondaryUnit == 0
            ? (int?) null
            : Model.Quantity % Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public int Pieces => Model.Item.PiecesPerSecondaryUnit == 0
            ? Model.Quantity % Item.PiecesPerUnit
            : Model.Quantity % Model.Item.PiecesPerUnit % Model.Item.PiecesPerSecondaryUnit;

        public decimal Discount
        {
            get { return Model.Discount * Model.Item.PiecesPerUnit; }
            set
            {
                Model.Discount = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("Discount");
                Total = Model.Quantity * (Model.PurchasePrice - Model.Discount);
            }
        }

        public decimal PurchasePrice
        {
            get { return Model.PurchasePrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.PurchasePrice = value / Model.Item.PiecesPerUnit;
                OnPropertyChanged("PurchasePrice");
                Total = Model.Quantity * (Model.PurchasePrice - Model.Discount);
            }
        }

        public decimal PurchasePricePerUnit => Model.PurchasePrice * Model.Item.PiecesPerUnit;

        public decimal Total
        {
            get { return Model.Total; }
            set
            {
                Model.Total = value;
                OnPropertyChanged("Total");
            }
        }

        public int SoldOrReturned
        {
            get { return Model.SoldOrReturned; }
            set
            {
                Model.SoldOrReturned = value;
                OnPropertyChanged("SoldOrReturned");
            }
        }

        public decimal GetNetDiscount()
        {
            var lineDiscount = Model.Discount / Model.Item.PiecesPerUnit;
            var lineSalesPrice = Model.PurchasePrice / Model.Item.PiecesPerUnit;
            if (lineSalesPrice - lineDiscount == 0) return 0;
            var fractionOfTransaction = Model.Quantity * (lineSalesPrice - lineDiscount) /
                                        Model.PurchaseTransaction.GrossTotal;
            var fractionOfTransactionDiscount = fractionOfTransaction * Model.PurchaseTransaction.Discount /
                                                Model.Quantity;
            var discount = (lineDiscount + fractionOfTransactionDiscount) * Model.Item.PiecesPerUnit;
            return discount;
        }

        public void UpdateTotal()
        {
            Total = (Model.PurchasePrice - Model.Discount) * Model.Quantity;
        }
    }
}