namespace ECERP.Services
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Models.Accounting;
    using Utilities;

    internal static class AccountingService
    {
        private static readonly List<string> _protectedAccounts = new List<string>
        {
            "Sales Returns And Allowances",
            "Cost of Goods Sold",
            "- Accounts Payable",
            "Inventory"
        };

        public static List<string> GetProtectedAccounts() => _protectedAccounts;

        public static void CreateNewAccount(ERPContext context, string accountName, string accountGroup)
        {
            var newLedgerAccount = CreateLedgerAccount(context, accountName, accountGroup);

            var newAccountGeneralLedger = new LedgerGeneral
            {
                LedgerAccount = newLedgerAccount,
                PeriodYear = context.Ledger_General.First().PeriodYear,
                Period = context.Ledger_General.First().Period
            };

            var newAccountBalances = new LedgerAccountBalance
            {
                LedgerAccount = newLedgerAccount,
                PeriodYear = context.Ledger_General.First().PeriodYear
            };

            newLedgerAccount.LedgerGeneral = newAccountGeneralLedger;
            newLedgerAccount.LedgerAccountBalances.Add(newAccountBalances);
            context.Ledger_Accounts.Add(newLedgerAccount);

            context.SaveChanges();
        }

        private static LedgerAccount CreateLedgerAccount(ERPContext context, string accountName, string accountGroup)
        {
            var ledgerAccountsClasses = context.Ledger_Account_Classes.ToList();
            var ledgerAccountsGroups = context.Ledger_Account_Groups.ToList();

            if (accountGroup.Equals(Constants.Accounting.BANK))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.Accounting.CURRENT_ASSET,
                    LedgerAccountClass =
                        ledgerAccountsClasses.Single(
                            accountClass => accountClass.Name.Equals(Constants.Accounting.ASSET)),
                    LedgerAccountGroup =
                        ledgerAccountsGroups.Single(group => group.Name.Equals(accountGroup)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            if (accountGroup.Equals(Constants.Accounting.OPERATING_EXPENSE))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.Accounting.OPERATING_EXPENSE,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(
                            accountClass => accountClass.Name.Equals(Constants.Accounting.EXPENSE)),
                    LedgerAccountGroup =
                        ledgerAccountsGroups.Single(group => group.Name.Equals(accountGroup)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            if (accountGroup.Equals(Constants.Accounting.ACCOUNTS_RECEIVABLE))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.Accounting.ACCOUNTS_RECEIVABLE,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(
                            accountClass => accountClass.Name.Equals(Constants.Accounting.ASSET)),
                    LedgerAccountGroup =
                        ledgerAccountsGroups.Single(group => group.Name.Equals(accountGroup)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            if (accountGroup.Equals(Constants.Accounting.ACCOUNTS_PAYABLE))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.Accounting.ACCOUNTS_PAYABLE,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(
                            accountClass => accountClass.Name.Equals(Constants.Accounting.LIABILITY)),
                    LedgerAccountGroup =
                        ledgerAccountsGroups.Single(group => group.Name.Equals(accountGroup)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            return new LedgerAccount
            {
                Name = accountName,
                Notes = Constants.Accounting.EXPENSE,
                LedgerAccountClass =
                    ledgerAccountsClasses.First(
                        accountClass => accountClass.Name.Equals(Constants.Accounting.EXPENSE)),
                LedgerAccountGroup =
                    ledgerAccountsGroups.Single(group => group.Name.Equals(accountGroup)),
                LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
            };
        }
    }
}