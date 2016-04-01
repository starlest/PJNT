namespace PutraJayaNT.ViewModels.Item
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Models;
    using Models.Inventory;
    using MVVMFramework;

    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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

        public string SecondaryUnitName
        {
            get { return Model.SecondaryUnitName; }
            set
            {
                Model.SecondaryUnitName = value;
                OnPropertyChanged("SecondaryUnitName");
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

        public int PiecesPerSecondaryUnit
        {
            get { return Model.PiecesPerSecondaryUnit; }
            set
            {
                Model.PiecesPerSecondaryUnit = value;
                OnPropertyChanged("PiecesPerSecondaryUnit");
            }
        }

        public string Unit => Model.PiecesPerSecondaryUnit == 0 ?
            Model.UnitName + "/" + Model.PiecesPerUnit :
            Model.UnitName + "/" + Model.PiecesPerUnit / Model.PiecesPerSecondaryUnit;

        public string SecondaryUnit => Model.PiecesPerSecondaryUnit == 0 ? null :
            Model.SecondaryUnitName + "/" + Model.PiecesPerSecondaryUnit;

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

        public ObservableCollection<AlternativeSalesPrice> AlternativeSalesPrices => Model.AlternativeSalesPrices;

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
            OnPropertyChanged("SecondaryUnitName");
            OnPropertyChanged("PiecesPerSecondaryUnit");
            OnPropertyChanged("Unit");
            OnPropertyChanged("SalesExpense");
            OnPropertyChanged("Active");
            SelectedSupplier = Suppliers.FirstOrDefault();
        }

        public override bool Equals(object obj)
        {
            var item = obj as ItemVM;
            return item != null && ID.Equals(item.ID);
        }
    }
}
