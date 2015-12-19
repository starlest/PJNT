using MVVMFramework;
using PutraJayaNT.Models.Sales;

namespace PutraJayaNT.ViewModels
{
    public class SalesmanVM : ViewModelBase<Salesman>
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

        public override bool Equals(object obj)
        {
            var salesman = obj as SalesmanVM;
            if (salesman == null) return false;
            else return this.ID.Equals(salesman.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
