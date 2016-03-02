using System.Data.Entity.Migrations;

namespace PutraJayaNT.Migrations
{
    public partial class Update7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CanDeleteInvoice", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "CanDeleteInvoice");
        }
    }
}
