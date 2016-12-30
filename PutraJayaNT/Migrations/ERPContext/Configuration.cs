namespace ECERP.Migrations.ERPContext
{
    using System.Data.Entity.Migrations;
    using Models.Accounting;
    using Utilities;

    internal sealed class Configuration : DbMigrationsConfiguration<ERPContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\ERPContext";
        }

        protected override void Seed(ERPContext context)
        {
            // Add "-" supplier manually

            //            context.SystemParameters.AddOrUpdate(
            //                parameter => parameter.Key,
            //                new SystemParameter { Key = "TelegramKey" },
            //                new SystemParameter { Key = "ThemeColor", Value = "Blue" },
            //                new SystemParameter { Key = "CurrentDate", Value = DateTime.Now.Date.ToShortDateString() }
            //                );

            context.Ledger_Account_Classes.AddOrUpdate(
                accountClass => accountClass.Name,
                new LedgerAccountClass { Name = Constants.Accounting.ASSET },
                new LedgerAccountClass { Name = Constants.Accounting.LIABILITY },
                new LedgerAccountClass { Name = Constants.Accounting.EQUITY },
                new LedgerAccountClass { Name = Constants.Accounting.EXPENSE },
                new LedgerAccountClass { Name = Constants.Accounting.REVENUE }
                );

            context.Ledger_Account_Groups.AddOrUpdate(
                accountGroup => accountGroup.Name,
                new LedgerAccountGroup { Name = Constants.Accounting.CURRENT_ASSET },
                new LedgerAccountGroup { Name = Constants.Accounting.OPERATING_EXPENSE },
                new LedgerAccountGroup { Name = Constants.Accounting.ACCOUNTS_PAYABLE },
                new LedgerAccountGroup { Name = Constants.Accounting.ACCOUNTS_RECEIVABLE },
                new LedgerAccountGroup { Name = Constants.Accounting.COST_OF_GOODS_SOLD },
                new LedgerAccountGroup { Name = Constants.Accounting.INCOME },
                new LedgerAccountGroup { Name = Constants.Accounting.INVENTORY }
                );
        }
    }
}
