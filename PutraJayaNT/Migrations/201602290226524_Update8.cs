using System.Data.Entity.Migrations;

namespace PutraJayaNT.Migrations
{
    public partial class Update8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerGroups", "CreditTerms", c => c.Int(nullable: false));
            AddColumn("dbo.CustomerGroups", "MaxInvoices", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CustomerGroups", "MaxInvoices");
            DropColumn("dbo.CustomerGroups", "CreditTerms");
        }
    }
}
