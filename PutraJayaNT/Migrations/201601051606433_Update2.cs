namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StockBalances",
                c => new
                    {
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Year = c.Int(nullable: false),
                        BeginningBalance = c.Int(nullable: false),
                        Balance1 = c.Int(nullable: false),
                        Balance2 = c.Int(nullable: false),
                        Balance3 = c.Int(nullable: false),
                        Balance4 = c.Int(nullable: false),
                        Balance5 = c.Int(nullable: false),
                        Balance6 = c.Int(nullable: false),
                        Balance7 = c.Int(nullable: false),
                        Balance8 = c.Int(nullable: false),
                        Balance9 = c.Int(nullable: false),
                        Balance10 = c.Int(nullable: false),
                        Balance11 = c.Int(nullable: false),
                        Balance12 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItemID, t.Year })
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .Index(t => t.ItemID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StockBalances", "ItemID", "dbo.Inventory");
            DropIndex("dbo.StockBalances", new[] { "ItemID" });
            DropTable("dbo.StockBalances");
        }
    }
}
