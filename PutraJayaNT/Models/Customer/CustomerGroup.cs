using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Customer
{
    [Table("CustomerGroups")]
    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class CustomerGroup
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int CreditTerms { get; set; }

        public int MaxInvoices { get; set; }

        public override bool Equals(object obj)
        {
            var customerGroup = obj as CustomerGroup;
            return customerGroup != null && ID.Equals(customerGroup.ID);
        }
    }
}
