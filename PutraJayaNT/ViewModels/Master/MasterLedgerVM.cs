namespace ECRP.ViewModels.Master
{
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Ledger;
    using Models.Accounting;
    using MVVMFramework;
    using Utilities;

    class MasterLedgerVM : ViewModelBase
    {
        ObservableCollection<LedgerAccountVM> _displayAccounts;
        ObservableCollection<string> _classes;
        ObservableCollection<string> _groups;

        string _selectedClass;

        string _newEntryName;
        string _newEntryGroup;
        ICommand _newEntryCommand;

        public MasterLedgerVM()
        {
            _displayAccounts = new ObservableCollection<LedgerAccountVM>();
            _classes = new ObservableCollection<string>
            {
                "All", "Asset", "Liability", "Equity", "Expense", "Revenue"
            };
            _groups = new ObservableCollection<string>
            {
                "Bank", "Operating Expense", "Accounts Receivable", "Accounts Payable"
            };

            SelectedClass = _classes.FirstOrDefault();
        }

        public ObservableCollection<LedgerAccountVM> DisplayAccounts
        {
            get { return _displayAccounts; }
        }

        public ObservableCollection<string> Classes
        {
            get { return _classes; }
        }

        public ObservableCollection<string> Groups
        {
            get { return _groups; }
        }

        public string SelectedClass
        {
            get { return _selectedClass; }
            set
            {
                _displayAccounts.Clear();
                SetProperty(ref _selectedClass, value, "SelectedClass");

                if (value == "All")
                {
                    using (var context = UtilityMethods.createContext())
                    {
                        var accounts = context.Ledger_Accounts
                            .Include("LedgerGeneral")
                            .Where(e => !e.Name.Equals("- Accounts Payable"))
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
                    using (var context = UtilityMethods.createContext())
                    {
                        var accounts = context.Ledger_Accounts
                            .Where(e => e.Class == value && !e.Name.Equals("- Accounts Payable"))
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

        #region New Entry Properties
        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, "NewEntryName"); }
        }

        public string NewEntryGroup
        {
            get { return _newEntryGroup; }
            set { SetProperty(ref _newEntryGroup, value, "NewEntryGroup"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (MessageBox.Show("Confirm adding this account?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

                    using (var context = UtilityMethods.createContext())
                    {
                        CreateNewAccount(context);
                        context.SaveChanges();
                    }

                    MessageBox.Show("Successfully added account!", "Success", MessageBoxButton.OK);
                    ResetEntryFields();
                }));
            }
        }
        #endregion

        private void CreateNewAccount(ERPContext context)
        {
            LedgerAccount newAccount;
                
            if (_newEntryGroup.Equals("Bank"))
            {
                newAccount = new LedgerAccount
                {
                    Name = _newEntryName,
                    Class = "Asset",
                    Notes = "Current Asset",
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            else if (_newEntryGroup.Equals("Operating Expense"))
            {
                newAccount = new LedgerAccount
                {
                    Name = _newEntryName,
                    Class = "Expense",
                    Notes = "Operating Expense",
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            else if (_newEntryGroup.Equals("Accounts Receivable"))
            {
                newAccount = new LedgerAccount
                {
                    Name = _newEntryName,
                    Class = "Asset",
                    Notes = "Accounts Receivable",
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            else if (_newEntryGroup.Equals("Accounts Payable"))
            {
                newAccount = new LedgerAccount
                {
                    Name = _newEntryName,
                    Class = "Liability",
                    Notes = "Accounts Payable",
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            else
            {
                newAccount = new LedgerAccount
                {
                    Name = _newEntryName,
                    Class = "Expense",
                    Notes = "Operating Expense",
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            var newAccountGeneralLedger = new LedgerGeneral
            {
                LedgerAccount = newAccount,
                PeriodYear = context.Ledger_General.FirstOrDefault().PeriodYear,
                Period = context.Ledger_General.FirstOrDefault().Period
            };

            var newAccountBalances = new LedgerAccountBalance
            {
                LedgerAccount = newAccount,
                PeriodYear = context.Ledger_General.FirstOrDefault().PeriodYear
            };

            newAccount.LedgerGeneral = newAccountGeneralLedger;
            newAccount.LedgerAccountBalances.Add(newAccountBalances);
            context.Ledger_Accounts.Add(newAccount);
        }

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryGroup = null;
        }
    }
}
