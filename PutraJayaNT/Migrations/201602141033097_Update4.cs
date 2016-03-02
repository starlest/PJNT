using System.Data.Entity.Migrations;

namespace PutraJayaNT.Migrations
{
    public partial class Update4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dates", "Name", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dates", "Name");
        }
    }
}
