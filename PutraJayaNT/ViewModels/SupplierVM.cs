namespace PutraJayaNT.ViewModels.Suppliers
{
    using System.Collections.ObjectModel;
    using Models;
    using Models.Inventory;
    using MVVMFramework;

    public class SupplierVM : ViewModelBase<Supplier>
    {
        public int ID
        {
            get { return Model.ID; }
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

        public string Address
        {
            get { return Model.Address; }
            set
            {
                Model.Address = value;
                OnPropertyChanged("Address");
            }
        }

        public int GSTID
        {
            get { return Model.GSTID; }
            set
            {
                Model.GSTID = value;
                OnPropertyChanged("GSTID");
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

        public ObservableCollection<Item> Items
        {
            get { return Model.Items; }
        }

        public void UpdatePropertiesToUI()
        {
            OnPropertyChanged("ID");
            OnPropertyChanged("Name");
            OnPropertyChanged("Address");
            OnPropertyChanged("GSTID");
            OnPropertyChanged("Active");
        }

        public override bool Equals(object obj)
        {
            var supplier = obj as SupplierVM;

            if (supplier == null) return false;
            else return this.ID.Equals(supplier.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
