using MVVMFramework;
using PutraJayaNT.Models;
using System.Collections.ObjectModel;

namespace PutraJayaNT.ViewModels
{
    class ItemVM : ViewModelBase<Item>
    {
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

        public decimal Price
        {
            get { return Model.Price; }
            set
            {
                Model.Price = value;
                OnPropertyChanged("Price");
            }
        }

        public int Stock
        {
            get { return Model.Stock; }
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
