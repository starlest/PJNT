namespace PutraJayaNT.ViewModels.Sales
{
    using System;
    using MVVMFramework;
    using Models;
    using Models.Sales;
    using Utilities;
    using Utilities.Database.Sales;
    using Utilities.Database.Salesman;
    using Models.Salesman;
    using Models.Customer;
    using System.Collections.ObjectModel;

    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class SalesTransactionVM : ViewModelBase<SalesTransaction>
    {
        private bool _isSelected;

        public SalesTransactionVM()
        {
            CollectionSalesmans = new ObservableCollection<Salesman>();
            UpdateCollectionSalesmans();
        }

        public ObservableCollection<Salesman> CollectionSalesmans { get; }

        public string SalesTransactionID => Model.SalesTransactionID;

        public Customer Customer => Model.Customer;

        public decimal Total => Model.Total;

        public decimal Paid => Model.Paid;

        public decimal Remaining => Model.Total - Model.Paid;

        public DateTime Date => Model.Date;

        public DateTime DueDate => Model.DueDate;

        public User User => Model.User;

        public Salesman CollectionSalesman
        {
            get { return Model.CollectionSalesman; }
            set
            {
                if (value == null) return;
                Model.CollectionSalesman = value;
                OnPropertyChanged("CollectionSalesman");
                SaveCollectionSalesmanToTransactionInDatabase();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, "IsSelected"); }
        }

        public override bool Equals(object obj)
        {
            var line = obj as SalesTransactionVM;
            return line != null && SalesTransactionID.Equals(line.SalesTransactionID);
        }

        private void UpdateCollectionSalesmans()
        {
            var collectionSalesmansFromDatabase = DatabaseSalesmanHelper.GetAllIncludingEmptySalesman();
            foreach (var salesman in collectionSalesmansFromDatabase)
                CollectionSalesmans.Add(salesman);
        }

        private void SaveCollectionSalesmanToTransactionInDatabase()
        {
            using (var context = new ERPContext())
            {
                var transactionFromDatabase =
                    DatabaseSalesTransactionHelper.FirstOrDefault(transaction => transaction.SalesTransactionID.Equals(Model.SalesTransactionID));
                transactionFromDatabase.CollectionSalesman = DatabaseSalesmanHelper.FirstOrDefault(salesman => salesman.ID.Equals(Model.CollectionSalesman.ID));
                context.SaveChanges();
            }
        }
    }
}
