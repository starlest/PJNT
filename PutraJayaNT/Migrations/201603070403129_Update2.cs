namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Salesmen", "Name", c => c.String(nullable: false, maxLength: 100, storeType: "nvarchar"));
            CreateIndex("dbo.Salesmen", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Salesmen", new[] { "Name" });
            AlterColumn("dbo.Salesmen", "Name", c => c.String(unicode: false));
        }
    }
}
