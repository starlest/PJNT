using PutraJayaNT.Models.Sales;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Salesman
{
    public class Salesman
    {
        public int ID { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual ObservableCollection<SalesTransactionLine> SalesTransactionLines { get; set; }

        public virtual ObservableCollection<SalesCommission> SalesCommissions { get; set; } 

        public override bool Equals(object obj)
        {
            var salesman = obj as Salesman;
            if (salesman == null) return false;
            else return this.ID.Equals(salesman.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
