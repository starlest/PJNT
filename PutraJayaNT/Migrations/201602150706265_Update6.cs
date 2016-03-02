using System.Data.Entity.Migrations;

namespace PutraJayaNT.Migrations
{
    public partial class Update6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "ViewOnly", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ViewOnly");
        }
    }
}
