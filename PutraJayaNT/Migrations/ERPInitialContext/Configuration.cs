namespace ECERP.Migrations.ERPInitialContext
{
    using System.Data.Entity.Migrations;
    using Utilities;

    internal sealed class Configuration : DbMigrationsConfiguration<ERPInitialContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\ERPInitialContext";
        }

        protected override void Seed(ERPInitialContext context)
        {
        }
    }
}
