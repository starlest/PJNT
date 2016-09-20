﻿namespace ECRP.Models.Accounting
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using MVVMFramework;

    [Table("Ledger_Transactions")]
    public class LedgerTransaction : ObservableObject
    { 
        public LedgerTransaction()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            LedgerTransactionLines = new ObservableCollection<LedgerTransactionLine>();
        }

        [Key]
        public int ID { get; set; }

        [Required, Index]
        public DateTime Date { get; set; }

        public string Documentation { get; set; }

        public string Description { get; set; }

        public virtual User User { get; set; }

        public virtual ObservableCollection<LedgerTransactionLine> LedgerTransactionLines { get; set; }
    }
}
