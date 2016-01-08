using MVVMFramework;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Accounting;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace PutraJayaNT.ViewModels.Master
{
    class MasterLedgerVM : ViewModelBase
    {
        ObservableCollection<LedgerAccountVM> _displayAccounts;
        ObservableCollection<string> _classes;

        string _selectedClass;

        public MasterLedgerVM()
        {
            _displayAccounts = new ObservableCollection<LedgerAccountVM>();
            _classes = new ObservableCollection<string>();

            _classes.Add("All");
            _classes.Add("Asset");
            _classes.Add("Liability");
            _classes.Add("Equity");
            _classes.Add("Expense");
            _classes.Add("Revenue");

            SelectedClass = "All";
        }

        public ObservableCollection<LedgerAccountVM> DisplayAccounts
        {
            get { return _displayAccounts; }
        }

        public ObservableCollection<string> Classes
        {
            get { return _classes; }
        }

        public string SelectedClass
        {
            get { return _selectedClass; }
            set {
                _displayAccounts.Clear();
                SetProperty(ref _selectedClass, value, "SelectedClass");

                if (value == "All")
                {
                    using (var context = new ERPContext())
                    {
                        var accounts = context.Ledger_Accounts
                            .Include("LedgerGeneral")
                            .OrderBy(e => e.Class)
                            .ThenBy(e => e.Notes)
                            .ThenBy(e => e.Name);
                        
                        foreach (var account in accounts)
                            _displayAccounts.Add(new LedgerAccountVM { Model = account });
                    }
                }

                else
                {
                    _displayAccounts.Clear();
                    using (var context = new ERPContext())
                    {
                        var accounts = context.Ledger_Accounts
                            .Where(e => e.Class == value)
                            .Include("LedgerGeneral")
                            .OrderBy(e => e.Class)
                            .ThenBy(e => e.Notes)
                            .ThenBy(e => e.Name);

                        foreach (var account in accounts)
                            _displayAccounts.Add(new LedgerAccountVM { Model = account });
                    }
                }
            }
        }
    }
}
