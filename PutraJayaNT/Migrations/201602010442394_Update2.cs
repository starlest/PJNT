namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "AlternativeSalesPrices",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        SalesPrice = c.Decimal(nullable: false, precision: 50, scale: 30),
                    })
                .PrimaryKey(t => new { t.Name, t.ItemID })
                .ForeignKey("Inventory", t => t.ItemID, cascadeDelete: true)
                .Index(t => t.ItemID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("AlternativeSalesPrices", "ItemID", "Inventory");
            DropIndex("AlternativeSalesPrices", new[] { "ItemID" });
            DropTable("AlternativeSalesPrices");
        }
    }
}
