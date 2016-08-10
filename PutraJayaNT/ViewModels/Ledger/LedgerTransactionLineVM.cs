namespace PutraJayaNT.ViewModels.Ledger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.Accounting;
    using MVVMFramework;
    using Utilities;

    public class LedgerTransactionLineVM : ViewModelBase<LedgerTransactionLine>
    {
        private readonly List<LedgerTransactionLineVM> _opposingLines;

        public LedgerTransactionLineVM()
        {
            _opposingLines = new List<LedgerTransactionLineVM>();
        }

        public LedgerTransaction LedgerTransaction
        {
            get { return Model.LedgerTransaction; }
            set { Model.LedgerTransaction = value; }
        }

        public DateTime Date
        {
            get { return Model.LedgerTransaction.Date; }
            set { Model.LedgerTransaction.Date = value; }
        }

        public string Documentation
        {
            get { return Model.LedgerTransaction.Documentation; }
            set { Model.LedgerTransaction.Documentation = value; }
        }

        public string Description
        {
            get { return Model.LedgerTransaction.Description; }
            set { Model.LedgerTransaction.Description = value; }
        }

        public string Seq
        {
            get { return Model.Seq; }
            set { Model.Seq = value; }
        }

        public decimal Amount
        {
            get { return Model.Amount; }
            set { Model.Amount = value; }
        }

        public LedgerAccount LedgerAccount
        {
            get { return Model.LedgerAccount; }
            set { Model.LedgerAccount = value; }
        }

        public List<LedgerTransactionLineVM> OpposingLines
        {
            get
            {
                _opposingLines.Clear();
                using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
                {
                    var lines = context.Ledger_Transaction_Lines
                        .Include("LedgerTransaction")
                        .Include("LedgerAccount")
                        .Where(e => e.LedgerTransactionID == LedgerTransaction.ID && e.LedgerAccountID != LedgerAccount.ID);
                    foreach (var line in lines)
                        _opposingLines.Add(new LedgerTransactionLineVM { Model = line });
                }

                return _opposingLines;
            }
        }

        public decimal Balance { get; set; }
    }
}
