namespace ECRP.Services
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

            if (accountGroup.Equals(Constants.BANK))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.CURRENT_ASSET,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(accountClass => accountClass.Name.Equals(Constants.ASSET)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            if (accountGroup.Equals(Constants.OPERATING_EXPENSE))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.OPERATING_EXPENSE,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(accountClass => accountClass.Name.Equals(Constants.EXPENSE)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            if (accountGroup.Equals(Constants.ACCOUNTS_RECEIVABLE))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.ACCOUNTS_RECEIVABLE,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(accountClass => accountClass.Name.Equals(Constants.ASSET)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            if (accountGroup.Equals(Constants.ACCOUNTS_PAYABLE))
            {
                return new LedgerAccount
                {
                    Name = accountName,
                    Notes = Constants.ACCOUNTS_PAYABLE,
                    LedgerAccountClass =
                        ledgerAccountsClasses.First(accountClass => accountClass.Name.Equals(Constants.LIABILITY)),
                    LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
                };
            }

            return new LedgerAccount
            {
                Name = accountName,
                Notes = Constants.EXPENSE,
                LedgerAccountClass =
                    ledgerAccountsClasses.First(accountClass => accountClass.Name.Equals(Constants.EXPENSE)),
                LedgerAccountBalances = new ObservableCollection<LedgerAccountBalance>()
            };
        }
    }
}