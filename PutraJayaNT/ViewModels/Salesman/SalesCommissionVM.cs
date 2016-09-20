namespace ECRP.ViewModels.Salesman
{
    using Models.Inventory;
    using Models.Salesman;
    using MVVMFramework;

    public class SalesCommissionVM : ViewModelBase<SalesCommission>
    {
        public Salesman Salesman
        {
            get { return Model.Salesman; }
            set
            {
                Model.Salesman = value;
                OnPropertyChanged("Salesman");
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

        public decimal Percentage
        {
            get { return Model.Percentage; }
            set
            {
                Model.Percentage = value;
                OnPropertyChanged("Percentage");
            }
        }

        public decimal Total { get; set; }

        public decimal Commission { get; set; }

        public void UpdatePropertiesToUI()
        {
            OnPropertyChanged("Category");
            OnPropertyChanged("Percentage");
        }
    }
}
