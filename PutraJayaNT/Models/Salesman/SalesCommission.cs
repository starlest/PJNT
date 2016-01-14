﻿using PutraJayaNT.Models.Inventory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.Salesman
{
    public class SalesCommission
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Salesman_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Category_ID { get; set; }

        public decimal Percentage { get; set; }

        [ForeignKey("Salesman_ID")]
        public virtual Salesman Salesman { get; set; }

        [ForeignKey("Category_ID")]
        public virtual Category Category { get; set; }
    }
}