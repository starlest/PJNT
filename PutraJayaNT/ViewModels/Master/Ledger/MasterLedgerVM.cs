namespace ECRP.ViewModels.Master.Ledger
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Accounting;
    using MVVMFramework;
    using Utilities;
    using ViewModels.Ledger;

    internal class MasterLedgerVM : ViewModelBase
    {
        private string _selectedClass;

        private string _newEntryName;
        private string _newEntryGroup;
        private ICommand _newEntryCommand;

        public MasterLedgerVM()
        {
            DisplayAccounts = new ObservableCollection<LedgerAccountVM>();

            Classes = new ObservableCollection<string>
            {
                Constants.ALL,
                Constants.ASSET,
                Constants.LIABILITY,
                Constants.EQUITY,
                Constants.EXPENSE,
                Constants.REVENUE
            };

            Groups = new ObservableCollection<string>
            {
                Constants.BANK,
                Constants.OPERATING_EXPENSE,
                Constants.ACCOUNTS_RECEIVABLE,
                Constants.ACCOUNTS_PAYABLE
            };

            SelectedClass = Classes.FirstOrDefault();
        }

        public ObservableCollection<LedgerAccountVM> DisplayAccounts { get; }

        public ObservableCollection<string> Classes { get; }

        public ObservableCollection<string> Groups { get; }

        public string SelectedClass
        {
            get { return _selectedClass; }
            set
            {
                SetProperty(ref _selectedClass, value, () => SelectedClass);
                UpdateDisplayAccounts();
            }
        }

        private void UpdateDisplayAccounts()
        {
            DisplayAccounts.Clear();

            Func<LedgerAccount, bool> searchCondition;

            if (_selectedClass == Constants.ALL)
                searchCondition = account => !account.Name.Equals("- Accounts Payable");
            else
                searchCondition = account => account.Class == _selectedClass && !account.Name.Equals("- Accounts Payable");
            
            using (var context = UtilityMethods.createContext())
            {
                var accounts = context.Ledger_Accounts
                    .Include("LedgerAccountClass")
                    .Include("LedgerGeneral")
                    .Where(searchCondition)
                    .OrderBy(account => account.LedgerAccountClass.ID)
                    .ThenBy(account => account.Notes)
                    .ThenBy(account => account.Name);

                foreach (var account in accounts)
                    DisplayAccounts.Add(new LedgerAccountVM { Model = account });
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
                    if (MessageBox.Show("Confirm adding this account?", "Confirmation", MessageBoxButton.YesNo) ==
                        MessageBoxResult.No) return;

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

            if (_newEntryGroup.Equals(Constants.BANK))
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