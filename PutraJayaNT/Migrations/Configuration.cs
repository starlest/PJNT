namespace ECRP.Migrations
{
    using System.Data.Entity.Migrations;
    using Models.Accounting;
    using Utilities;

    internal sealed class Configuration : DbMigrationsConfiguration<ERPContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ERPContext context)
        {
            context.Ledger_Account_Classes.AddOrUpdate(
                new LedgerAccountClass { Name = "Asset" },
                new LedgerAccountClass { Name = "Liability" },
                new LedgerAccountClass { Name = "Equity" },
                new LedgerAccountClass { Name = "Expense" },
                new LedgerAccountClass { Name = "Revenue" }
                );
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}