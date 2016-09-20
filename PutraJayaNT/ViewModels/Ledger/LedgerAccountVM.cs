namespace ECRP.ViewModels.Ledger
{
    using System.Collections.ObjectModel;
    using Models.Accounting;
    using MVVMFramework;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class LedgerAccountVM : ViewModelBase<LedgerAccount>
    {
        private readonly ObservableCollection<LedgerTransactionLineVM> _transactionLines;

        public LedgerAccountVM()
        {
            _transactionLines = new ObservableCollection<LedgerTransactionLineVM>();
        }

        public int ID
        {
            get { return Model.ID; }
            set { Model.ID = value; }
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public string Notes
        {
            get { return Model.Notes; }
            set { Model.Notes = value; }
        }

        public string Class
        {
            get { return Model.Class; }
            set { Model.Class = value; }
        }

        public int PeriodYear
        {
            get { return Model.LedgerGeneral.PeriodYear; }
            set { Model.LedgerGeneral.PeriodYear = value; }
        }

        public int Period
        {
            get { return Model.LedgerGeneral.Period; }
            set { Model.LedgerGeneral.Period = value; }
        }

        public decimal Debit
        {
            get { return Model.LedgerGeneral.Debit; }
            set { Model.LedgerGeneral.Debit = value; }
        }

        public decimal Credit
        {
            get { return Model.LedgerGeneral.Credit; }
            set { Model.LedgerGeneral.Credit = value; }
        }

        public LedgerAccountClass LedgerAccountClass
        {
            get { return Model.LedgerAccountClass; }
            set { Model.LedgerAccountClass = value; }
        }

        public ObservableCollection<LedgerTransactionLineVM> TransactionLines
        {
            get
            {
                _transactionLines.Clear();

                foreach (var line in Model.LedgerTransactionLines)
                {
                    _transactionLines.Add(new LedgerTransactionLineVM { Model = line });
                }
                return _transactionLines;
            }
        }

        public override bool Equals(object obj)
        {
            var account = obj as LedgerAccountVM;
            return account != null && ID.Equals(account.ID);
        }
    }
}