using PutraJayaNT.Models.Sales;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models
{
    [Table("Customers")]
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

        public decimal SalesReturnCredits { get; set; }

        [Required]
        public bool Active { get; set; }

        public virtual List<SalesTransaction> SalesTransactions { get; set; }
    }
}
