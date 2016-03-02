using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    [Table("CustomerGroups")]
    public class CustomerGroup
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int CreditTerms { get; set; }

        public int MaxInvoices { get; set; }

        public override bool Equals(object obj)
        {
            var customerGroup = obj as CustomerGroup;
            if (customerGroup == null) return false;
            else return this.ID.Equals(customerGroup.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
