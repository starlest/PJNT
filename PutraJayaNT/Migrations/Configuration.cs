namespace ECRP.Migrations
{
    using System.Data.Entity.Migrations;
    using Models;
    using Models.Accounting;
    using Utilities;

    internal sealed class Configuration : DbMigrationsConfiguration<ERPContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ERPContext context)
        {
            // Add "-" supplier manually

            context.SystemParameters.AddOrUpdate(
                parameter => parameter.Key,
                new SystemParameter { Key = "ServerName" },
                new SystemParameter { Key = "TelegramKey" },
                new SystemParameter { Key = "ThemeColor", Value = "Blue" }
                );

            context.Ledger_Account_Classes.AddOrUpdate(
                accountClass => accountClass.Name,
                new LedgerAccountClass { Name = Constants.LedgerAccountClasses.ASSET },
                new LedgerAccountClass { Name = Constants.LedgerAccountClasses.LIABILITY },
                new LedgerAccountClass { Name = Constants.LedgerAccountClasses.EQUITY },
                new LedgerAccountClass { Name = Constants.LedgerAccountClasses.EXPENSE },
                new LedgerAccountClass { Name = Constants.LedgerAccountClasses.REVENUE }
                );

            context.Ledger_Account_Groups.AddOrUpdate(
                accountGroup => accountGroup.Name,
                new LedgerAccountGroup { Name = Constants.CURRENT_ASSET },
                new LedgerAccountGroup { Name = Constants.OPERATING_EXPENSE },
                new LedgerAccountGroup { Name = Constants.ACCOUNTS_PAYABLE },
                new LedgerAccountGroup { Name = Constants.ACCOUNTS_RECEIVABLE },
                new LedgerAccountGroup { Name = Constants.COST_OF_GOODS_SOLD },
                new LedgerAccountGroup { Name = Constants.INCOME },
                new LedgerAccountGroup { Name = Constants.INVENTORY }
                );
        }
    }
}