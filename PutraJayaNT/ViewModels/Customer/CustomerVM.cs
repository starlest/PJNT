namespace PutraJayaNT.ViewModels.Customer
{
    using Models.Customer;
    using MVVMFramework;

    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class CustomerVM : ViewModelBase<Customer>
    {
        public int ID
        {
            get { return Model.ID; }
            set
            {
                Model.ID = value;
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

        public CustomerGroup Group
        {
            get { return Model.Group; }
            set
            {
                Model.Group = value;
                OnPropertyChanged("Group");
            }
        }

        public string City
        {
            get { return Model.City; }
            set
            {
                Model.City = value;
                OnPropertyChanged("City");
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

        public string Telephone
        {
            get { return Model.Telephone; }
            set
            {
                Model.Telephone = value;
                OnPropertyChanged("Telephone");
            }
        }

        public string NPWP
        {
            get { return Model.NPWP; }
            set
            {
                Model.NPWP = value;
                OnPropertyChanged("NPWP");
            }
        }

        public int CreditTerms
        {
            get { return Model.CreditTerms; }
            set
            {
                Model.CreditTerms = value;
                OnPropertyChanged("CreditTerms");
            }
        }

        public int MaxInvoices
        {
            get { return Model.MaxInvoices; }
            set
            {
                Model.MaxInvoices = value;
                OnPropertyChanged("MaxInvoices");
            }
        }

        public decimal SalesReturnCredits
        {
            get { return Model.SalesReturnCredits; }
            set
            {
                Model.SalesReturnCredits = value;
                OnPropertyChanged("SalesReturnCredits");
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

        public void UpdatePropertiesToUI()
        {
            OnPropertyChanged("ID");
            OnPropertyChanged("Name");
            OnPropertyChanged("Group");
            OnPropertyChanged("City");
            OnPropertyChanged("Address");
            OnPropertyChanged("Telephone");
            OnPropertyChanged("NPWP");
            OnPropertyChanged("CreditTerms");
            OnPropertyChanged("MaxInvoices");
            OnPropertyChanged("SalesReturnCredits");
            OnPropertyChanged("Active");
        }

        public override string ToString()
        {
            return Model.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var customer = obj as CustomerVM;
            return customer != null && ID.Equals(customer.ID);
        }
    }
}