namespace PutraJayaNT.ViewModels.Customers
{
    using MVVMFramework;
    using PutraJayaNT.Models;
    using PutraJayaNT.Models.Sales;
    using PutraJayaNT.Models.Salesman;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Utilities;

    public class SalesTransactionMultiPurposeVM : ViewModelBase<SalesTransaction>
    {
        ObservableCollection<Salesman> _salesmen;
        bool _isSelected;

        public SalesTransactionMultiPurposeVM()
        {
            _salesmen = new ObservableCollection<Salesman>();

            using (var context = new ERPContext())
            {
                var salesmen = context.Salesmans.ToList();

                foreach (var salesman in salesmen)
                    _salesmen.Add(salesman);
            }
        }

        public string SalesTransactionID
        {
            get { return Model.SalesTransactionID; }
        }

        public Customer Customer
        {
            get { return Model.Customer; }
        }

        public decimal Total
        {
            get { return Model.Total; }
        }

        public decimal Paid
        {
            get { return Model.Paid; }
        }

        public decimal Remaining
        {
            get { return Model.Total - Model.Paid; }
        }

        public DateTime When
        {
            get { return Model.When; }
        }

        public DateTime DueDate
        {
            get { return Model.DueDate; }
        }

        public User User
        {
            get { return Model.User; }
        }

        public Salesman CollectionSalesman
        {
            get { return Model.CollectionSalesman; }
            set
            {
                Model.CollectionSalesman = value;

                using (var context = new ERPContext())
                {
                    var transaction = context.SalesTransactions.Where(e => e.SalesTransactionID.Equals(Model.SalesTransactionID)).FirstOrDefault();
                    transaction.CollectionSalesman = context.Salesmans.Where(e => e.ID.Equals(value.ID)).FirstOrDefault();
                    context.SaveChanges();
                }

                OnPropertyChanged("CollectionSalesman");
            }
        }

        public ObservableCollection<Salesman> Salesmen
        {
            get { return _salesmen; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, "IsSelected"); }
        }

        public override bool Equals(object obj)
        {
            var line = obj as SalesTransactionMultiPurposeVM;

            if (line == null) return false;
            else return this.SalesTransactionID.Equals(line.SalesTransactionID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
