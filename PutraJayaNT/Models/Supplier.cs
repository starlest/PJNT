﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PutraJayaNT.Models.Inventory;

namespace PutraJayaNT.Models
{
    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Supplier
    {
        public Supplier()
        {
            Active = true;
        }

        public int ID { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public int GSTID { get; set; }

        public decimal PurchaseReturnCredits { get; set; }

        public bool Active { get; set; }

        public virtual ObservableCollection<Item> Items { get; set; }

        public override string ToString() { return Name; }

        public override bool Equals(object obj)
        {
            var supplier = obj as Supplier;
            return supplier != null && ID.Equals(supplier.ID);
        }
    }
}
