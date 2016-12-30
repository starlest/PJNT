namespace ECERP.Migrations.ERPContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Customers", "C_ID", c => c.Int());
            CreateIndex("dbo.Customers", "C_ID");
            AddForeignKey("dbo.Customers", "C_ID", "dbo.Cities", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Customers", "C_ID", "dbo.Cities");
            DropIndex("dbo.Customers", new[] { "C_ID" });
            DropColumn("dbo.Customers", "C_ID");
            DropTable("dbo.Cities");
        }
    }
}
