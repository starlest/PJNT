using System.Collections.ObjectModel;

namespace PutraJayaNT.Models.Sales
{
    public class Salesman
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ObservableCollection<SalesTransaction> SalesTransactions { get; set; }
    }
}
