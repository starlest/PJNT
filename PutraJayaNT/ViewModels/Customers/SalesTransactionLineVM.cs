using MVVMFramework;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Inventory;
using PutraJayaNT.Models.Sales;
using System.Windows;

namespace PutraJayaNT.ViewModels.Customers
{
    public class SalesTransactionLineVM : ViewModelBase<SalesTransactionLine>
    {
        int _pieces;
        int _units;

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

        public int Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Pieces");
                OnPropertyChanged("Units");
                OnPropertyChanged("Total");
            }
        }

        public int Pieces
        {
            get
            {
                _pieces = Model.Quantity % Model.Item.PiecesPerUnit;
                return _pieces;
            }
            set
            {
                //if (value >= Model.Item.PiecesPerUnit || value < 0)
                //{
                //    MessageBox.Show(string.Format("Please enter a value between {0} - {1}", 0, Model.Item.PiecesPerUnit - 1), "Invalid Quantity", MessageBoxButton.OK);
                //    return;
                //}

                //if (GetStock() < (value + (_units * Model.Item.PiecesPerUnit)))
                //{
                //    MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                //        Model.Item.Name, (GetStock() / Model.Item.PiecesPerUnit), (GetStock() % Model.Item.PiecesPerUnit)),
                //        "Insufficient Stock", MessageBoxButton.OK);
                //    return;
                //}

                

                Model.Quantity = value + (_units * Model.Item.PiecesPerUnit);
                Total = Model.Quantity * (Model.SalesPrice - Model.Discount);

                SetProperty(ref _pieces, value, "Pieces");
            }
        }

        public int Units
        {
            get
            {
                _units = Model.Quantity / Model.Item.PiecesPerUnit;
                return _units;
            }
            set
            {
                //if (value < 0)
                //{
                //    MessageBox.Show("Please enter a valid value", "Invalid Quantity", MessageBoxButton.OK);
                //    return;
                //}

                //if (GetStock() < (_pieces + (value * Model.Item.PiecesPerUnit)))
                //{
                //    MessageBox.Show(string.Format("{0} has only {1} units, {2} pieces left.",
                //        Model.Item.Name, (GetStock() / Model.Item.PiecesPerUnit), (GetStock() % Model.Item.PiecesPerUnit)),
                //        "Insufficient Stock", MessageBoxButton.OK);
                //    return;
                //}

                

                Model.Quantity = _pieces + (value * Model.Item.PiecesPerUnit);
                Total = Model.Quantity * (Model.SalesPrice - Model.Discount);

                SetProperty(ref _units, value, "Units");
            }
        }

        public decimal SalesPrice
        {
            get { return Model.SalesPrice * Model.Item.PiecesPerUnit; }
            set
            {
                Model.SalesPrice = value / Model.Item.PiecesPerUnit;
                Total = Model.Quantity * (Model.SalesPrice - Model.Discount);
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

        public int StockDeducted { get; set; }

        public int GetStock()
        {
            int s = 0;
            foreach (var stock in Model.Item.Stocks)
            {
                if (stock.WarehouseID.Equals(Model.Warehouse.ID))
                {
                    s = stock.Pieces;
                    break;
                }
            }

            s += StockDeducted;

            return s;
        }

        public override bool Equals(object obj)
        {
            var line = obj as SalesTransactionLineVM;

            if (line == null) return false;
            else return this.Item.ItemID.Equals(line.Item.ItemID) &&
                    this.Warehouse.ID.Equals(line.Warehouse.ID) &&
                    this.SalesPrice.Equals(line.SalesPrice) &&
                    this.Discount.Equals(line.Discount);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
