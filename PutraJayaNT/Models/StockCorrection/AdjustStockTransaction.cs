﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PutraJayaNT.Models.StockCorrection
{
    public class AdjustStockTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string AdjustStrockTransactionID { get; set; }

        public DateTime Date { get; set; }

        public virtual ObservableCollection<AdjustStockTransactionLine> AdjustStockTransactionLines { get; set; }

        public virtual User User { get; set; }
    }
}