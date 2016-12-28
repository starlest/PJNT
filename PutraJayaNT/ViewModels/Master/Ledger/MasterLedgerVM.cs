namespace ECERP.ViewModels.Master.Ledger
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Accounting;
    using MVVMFramework;
    using Services;
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
                Constants.LedgerAccountClasses.ASSET,
                Constants.LedgerAccountClasses.LIABILITY,
                Constants.LedgerAccountClasses.EQUITY,
                Constants.LedgerAccountClasses.EXPENSE,
                Constants.LedgerAccountClasses.REVENUE
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

        #region Collections

        public ObservableCollection<LedgerAccountVM> DisplayAccounts { get; }

        public ObservableCollection<string> Classes { get; }

        public ObservableCollection<string> Groups { get; }

        #endregion

        public string SelectedClass
        {
            get { return _selectedClass; }
            set
            {
                SetProperty(ref _selectedClass, value, () => SelectedClass);
                UpdateDisplayAccounts();
            }
        }

        #region New Entry Properties

        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, () => NewEntryName); }
        }

        public string NewEntryGroup
        {
            get { return _newEntryGroup; }
            set { SetProperty(ref _newEntryGroup, value, () => NewEntryGroup); }
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
                        AccountingService.CreateNewAccount(context, _newEntryName, _newEntryGroup);
                    }
                    MessageBox.Show("Successfully added account!", "Success", MessageBoxButton.OK);
                    ResetEntryFields();
                }));
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateDisplayAccounts()
        {
            DisplayAccounts.Clear();

            Func<LedgerAccount, bool> searchCondition;

            if (_selectedClass == Constants.ALL)
                searchCondition = account => !account.Name.Equals("- Accounts Payable");
            else
                searchCondition =
                    account => account.LedgerAccountClass.Name == _selectedClass && !account.Name.Equals("- Accounts Payable");

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

        private void ResetEntryFields()
        {
            NewEntryName = null;
            NewEntryGroup = null;
        }

        #endregion
    }
}