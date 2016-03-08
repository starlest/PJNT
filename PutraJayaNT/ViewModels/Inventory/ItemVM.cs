namespace PutraJayaNT.ViewModels.Inventory
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using MVVMFramework;
    using Models;
    using Models.Inventory;

    public class ItemVM : ViewModelBase<Item>
    {
        private Supplier _selectedSupplier;

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

        public string Unit
        {
            get { return Model.UnitName + "/" + Model.PiecesPerUnit; }
        }

        public decimal SalesExpense
        {
            get { return Model.SalesExpense; }
            set
            {
                Model.SalesExpense = value;
                OnPropertyChanged("SalesExpense");
            }
        }

        public ObservableCollection<Supplier> Suppliers => Model.Suppliers;

        public ObservableCollection<Stock> Stocks
        {
            get { return Model.Stocks; }
            set
            {
                Model.Stocks = value;
                OnPropertyChanged("Stocks");
            }
        }

        public ObservableCollection<AlternativeSalesPrice> AlternativeSalesPrices
        {
            get { return Model.AlternativeSalesPrices; }
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

        public void UpdatePropertiesToUI()
        {
            OnPropertyChanged("ID");
            OnPropertyChanged("Name");
            OnPropertyChanged("Category");
            OnPropertyChanged("PurchasePrice");
            OnPropertyChanged("SalesPrice");
            OnPropertyChanged("UnitName");
            OnPropertyChanged("PiecesPerUnit");
            OnPropertyChanged("Unit");
            OnPropertyChanged("SalesExpense");
            OnPropertyChanged("Active");
            SelectedSupplier = Suppliers.FirstOrDefault();
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
