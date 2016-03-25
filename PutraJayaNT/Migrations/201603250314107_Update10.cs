namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Inventory", "SecondaryUnitName", c => c.String(unicode: false));
            AddColumn("dbo.Inventory", "PiecesPerSecondaryUnit", c => c.Int(nullable: false));
            DropColumn("dbo.Inventory", "PiecesPerPack");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Inventory", "PiecesPerPack", c => c.Int(nullable: false));
            DropColumn("dbo.Inventory", "PiecesPerSecondaryUnit");
            DropColumn("dbo.Inventory", "SecondaryUnitName");
        }
    }
}
