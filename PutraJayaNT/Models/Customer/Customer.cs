namespace ECRP.Models.Customer
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Sales;

    [Table("Customers")]
    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Customer
    {
        public Customer()
        {
            Active = true;
        }

        public int ID { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual CustomerGroup Group { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string Telephone { get; set; }

        public string NPWP { get; set; }

        [Required]
        public int CreditTerms { get; set; }

        [Required]
        public int MaxInvoices { get; set; }

        public decimal SalesReturnCredits { get; set; }

        [Required]
        public bool Active { get; set; }

        public virtual List<SalesTransaction> SalesTransactions { get; set; }

        public override bool Equals(object obj)
        {
            var customer = obj as Customer;
            return customer != null && ID.Equals(customer.ID);
        }
    }
}
