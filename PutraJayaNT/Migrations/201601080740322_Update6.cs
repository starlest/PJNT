namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update6 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SalesCommissions",
                c => new
                    {
                        Salesman_ID = c.Int(nullable: false),
                        Category_ID = c.Int(nullable: false),
                        CommissionPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.Salesman_ID, t.Category_ID })
                .ForeignKey("dbo.Categories", t => t.Category_ID, cascadeDelete: true)
                .ForeignKey("dbo.Salesmen", t => t.Salesman_ID, cascadeDelete: true)
                .Index(t => t.Salesman_ID)
                .Index(t => t.Category_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SalesCommissions", "Salesman_ID", "dbo.Salesmen");
            DropForeignKey("dbo.SalesCommissions", "Category_ID", "dbo.Categories");
            DropIndex("dbo.SalesCommissions", new[] { "Category_ID" });
            DropIndex("dbo.SalesCommissions", new[] { "Salesman_ID" });
            DropTable("dbo.SalesCommissions");
        }
    }
}
