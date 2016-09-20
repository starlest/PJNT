namespace ECRP.ViewModels.Sales
{
    using System;
    using Models.Inventory;
    using Models.Sales;
    using Models.Salesman;
    using MVVMFramework;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class SalesTransactionLineVM : ViewModelBase<SalesTransactionLine>
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

        public SalesTransaction SalesTransaction
        {
            get { return Model.SalesTransaction; }
            set
            {
                Model.SalesTransaction = value;
                OnPropertyChanged("SalesTransaction");
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

        public Salesman Salesman
        {
            get { return Model.Salesman; }
            set
            {
                Model.Salesman = value;
                OnPropertyChanged("Salesman");
            }
        }

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

        public string Unit => Model.Item.PiecesPerSecondaryUnit == 0 ? 
            Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit :
            Model.Item.UnitName + "/" + Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public string SecondaryUnit => Model.Item.PiecesPerSecondaryUnit == 0 ? null : 
            Model.Item.SecondaryUnitName + "/" + Model.Item.PiecesPerSecondaryUnit;

        public int Units => Model.Quantity / Model.Item.PiecesPerUnit;

        public int? SecondaryUnits => Model.Item.PiecesPerSecondaryUnit == 0 ? (int?) null : 
            Model.Quantity % Model.Item.PiecesPerUnit / Model.Item.PiecesPerSecondaryUnit;

        public int Pieces => Model.Item.PiecesPerSecondaryUnit == 0 ? Model.Quantity % Item.PiecesPerUnit :
            Model.Quantity % Model.Item.PiecesPerUnit % Model.Item.PiecesPerSecondaryUnit;

        public decimal SalesPrice
        {
            get { return Model.SalesPrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.SalesPrice = value / Model.Item.PiecesPerUnit;
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
                OnPropertyChanged("SalesPrice");
            }
        }

        public decimal Discount
        {
            get { return Model.Discount * Model.Item.PiecesPerUnit; }
            set
            {
                Model.Discount = (value <= 0 ? 0 : (value/ Model.Item.PiecesPerUnit) );
                Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
                OnPropertyChanged("Discount");
            }
        }

        public decimal Total
        {
            get { return Model.Total; }
            set
            {
                Model.Total = value;
                OnPropertyChanged("Total");
            }
        }

        public decimal NetTotal => GetNetLinePrice() / Model.Item.PiecesPerUnit * Model.Quantity;

        public override bool Equals(object obj)
        {
            var line = obj as SalesTransactionLineVM;
            if (line == null) return false;
            return Item.ItemID.Equals(line.Item.ItemID) &&
                   Warehouse.ID.Equals(line.Warehouse.ID) &&
                   SalesPrice.Equals(line.SalesPrice) &&
                   Discount.Equals(line.Discount);
        }

        #region Helper Methods
        public SalesTransactionLineVM Clone()
        {
            var cloneLine = new SalesTransactionLine
            {
                SalesTransaction = Model.SalesTransaction,
                Item = Model.Item,
                Warehouse = Model.Warehouse,
                SalesPrice = Model.SalesPrice,
                Discount = Model.Discount,
                Quantity = Model.Quantity,
                Total = Model.Total,
                Salesman = Model.Salesman
            };
            return new SalesTransactionLineVM { Model = cloneLine };
        }

        public decimal GetNetLinePrice()
        {
            var lineDiscount = Model.Discount;
            var lineSalesPrice = Model.SalesPrice;
            if (lineSalesPrice - lineDiscount == 0) return 0;
            var fractionOfTransaction = Model.Quantity * (lineSalesPrice - lineDiscount) / Model.SalesTransaction.GrossTotal;
            var fractionOfTransactionDiscount = fractionOfTransaction * Model.SalesTransaction.Discount / Model.Quantity;
            var fractionOfTransactionTax = fractionOfTransaction * Model.SalesTransaction.Tax / Model.Quantity;
            var netLinePrice = lineSalesPrice - lineDiscount - fractionOfTransactionDiscount + fractionOfTransactionTax;
            return Math.Round(netLinePrice * Model.Item.PiecesPerUnit, 2);
        }

        public void UpdateTotal()
        {
            Total = (Model.SalesPrice - Model.Discount) * Model.Quantity;
        }
        #endregion
    }
}
