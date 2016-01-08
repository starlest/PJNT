using MVVMFramework;
using PutraJayaNT.Models.Accounting;
using System.Collections.ObjectModel;

namespace PutraJayaNT.ViewModels.Accounting
{
    class LedgerAccountVM : ViewModelBase<LedgerAccount>
    {
        ObservableCollection<LedgerTransactionLineVM> _transactionLines;

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
            get {  return Model.Name; }
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

        public ObservableCollection<LedgerTransactionLineVM> TransactionLines
        {
            get
            {
                _transactionLines.Clear();

                foreach (var line in Model.TransactionLines)
                {
                    _transactionLines.Add(new LedgerTransactionLineVM { Model = line } );
                }
                return _transactionLines;
            }
        }

        public override bool Equals(object obj)
        {
            var account = obj as LedgerAccountVM;
            if (account == null) return false;
            else return this.ID.Equals(account.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
