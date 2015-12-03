using MVVMFramework;
using PUJASM.ERP.Models.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUJASM.ERP.ViewModels
{
    class LedgerAccountVM : ViewModelBase<LedgerAccount>
    {
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
    }
}
